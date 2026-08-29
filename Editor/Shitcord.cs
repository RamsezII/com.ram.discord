using System;
using _ARK_;
using Discord.Sdk;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    [InitializeOnLoad]
    static partial class Shitcord
    {
        static Client client;
        static bool is_initialized;
        static PlayModeStateChange playModeState;
        const string button_prefixe = "Assets/" + nameof(_DISCORD_) + "/";

        //----------------------------------------------------------------------------------------------------------

        static Shitcord()
        {
            Debug.Log($"{typeof(Shitcord)}.CONSTRUCTOR");

            AssemblyReloadEvents.beforeAssemblyReload += StopPresence;
            AssemblyReloadEvents.afterAssemblyReload += StartPresence;
            EditorApplication.quitting += StopPresence;
            EditorApplication.update += RunCallbacks;
            EditorApplication.delayCall += StartPresence;

            EditorApplication.playModeStateChanged += static value =>
            {
                playModeState = value;
                UpdatePresence();
            };
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            StartPresence();
        }

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(StartPresence))]
        static void StartPresence()
        {
            LoadRText();
            if (!activate_presence)
                return;

            if (is_initialized)
            {
                UpdatePresence();
                return;
            }

            Debug.Log($"{typeof(Shitcord)}.START_PRESENCE");

            try
            {
                client = new Client();
                client.SetApplicationId(application_id);
                client.SetStatusChangedCallback(OnClientStatusChanged);
                client.GetDiscordClientConnectedUser(application_id, OnDiscordClientConnectedUser);
                is_initialized = true;
            }
            catch (Exception exception) when (exception is DllNotFoundException || exception is EntryPointNotFoundException)
            {
                Debug.LogException(exception);
                client?.Dispose();
                client = null;
                return;
            }

            UpdatePresence();
        }

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(UpdatePresence))]
        static void UpdatePresence()
        {
            LoadRText();

            if (!activate_presence || !is_initialized || client == null)
                return;

            Debug.Log($"{typeof(Shitcord)}.UPDATE_PRESENCE");

            using Activity activity = new();
            using ActivityTimestamps timestamps = new();

            timestamps.SetStart((ulong)NUCLEOR.timestamp_editorStart.ToUnixTimeSeconds());

            activity.SetName(application_name);
            activity.SetType(ActivityTypes.Playing);
            activity.SetStatusDisplayType(StatusDisplayTypes.Name);

            if (show_unityVersion)
                activity.SetDetails("Unity " + Application.unityVersion);

            if (show_unityState)
                activity.SetState($"{playModeState}");

            activity.SetTimestamps(timestamps);

            client.UpdateRichPresence(activity, OnRichPresenceUpdated);
        }

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(StopPresence))]
        static void StopPresence()
        {
            if (!is_initialized || client == null)
                return;

            Debug.Log($"{typeof(Shitcord)}.SHUTDOWN");

            try
            {
                client.ClearRichPresence();
                RunCallbacks();
                client.Dispose();
            }
            finally
            {
                client = null;
                is_initialized = false;
            }
        }

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(RestartPresence))]
        static void RestartPresence()
        {
            StopPresence();
            StartPresence();
        }

        //----------------------------------------------------------------------------------------------------------

        static void RunCallbacks()
        {
            if (!is_initialized || client == null)
            {
                return;
            }

            NativeMethods.Discord_RunCallbacks();
        }

        static void OnClientStatusChanged(Client.Status status, Client.Error error, int errorDetail)
        {
            Debug.Log($"{typeof(Shitcord)}.STATUS status={status} error={error} detail={errorDetail}");
        }

        static void OnRichPresenceUpdated(ClientResult result)
        {
            try
            {
                if (result.Successful())
                {
                    Debug.Log($"{typeof(Shitcord)}.RICH_PRESENCE_UPDATED {result}");
                    return;
                }

                if (result == null)
                    Debug.LogError($"{typeof(Shitcord)}.RICH_PRESENCE_UPDATE_FAILED {nameof(result)}={result}");
                else
                    Debug.LogError($"{typeof(Shitcord)}.RICH_PRESENCE_UPDATE_FAILED type={result.Type()} status={result.Status()} code={result.ErrorCode()} error={result.Error()} details={result}");
            }
            finally
            {
                result.Dispose();
            }
        }

        static void OnDiscordClientConnectedUser(ClientResult result, UserHandle user)
        {
            try
            {
                if (!result.Successful())
                {
                    Debug.LogWarning($"{typeof(Shitcord)}.CLIENT_USER_UNAVAILABLE type={result.Type()} status={result.Status()} code={result.ErrorCode()} error={result.Error()} details={result}");
                    return;
                }
                else if (user == null)
                    Debug.LogWarning($"{typeof(Shitcord)}.CLIENT_USER_NULL");
                else
                    Debug.Log($"{typeof(Shitcord)}.CLIENT_USER display_name={user.DisplayName()} id={user.Id()}");
            }
            finally
            {
                user?.Dispose();
                result.Dispose();
            }
        }
    }
}