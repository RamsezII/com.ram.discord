#if UNITY_EDITOR
using System;
using System.IO.Pipes;
using _ARK_;
using Discord.Sdk;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    [InitializeOnLoad]
    static class ShitDiscordAJO
    {
        const ulong application_id = 1446532383409967194;
        static Client client;
        static bool is_initialized;
        static PlayModeStateChange playModeState;

        //----------------------------------------------------------------------------------------------------------

        static ShitDiscordAJO()
        {
            Debug.Log($"{typeof(ShitDiscordAJO)}.CONSTRUCTOR");

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

        //----------------------------------------------------------------------------------------------------------

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(StartPresence))]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartPresence()
        {
            if (is_initialized)
            {
                UpdatePresence();
                return;
            }

            Debug.Log($"{typeof(ShitDiscordAJO)}.START_PRESENCE");

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
            if (!is_initialized || client == null)
                return;

            Debug.Log($"{typeof(ShitDiscordAJO)}.UPDATE_PRESENCE");

            using Activity activity = new();
            using ActivityTimestamps timestamps = new();

            timestamps.SetStart((ulong)NUCLEOR.timestamp_editorStart.ToUnixTimeSeconds());

            activity.SetName("Grand Theft Auto VI");
            //activity.SetName("𝐒 𝐇 𝐈 𝐓 𝐒 𝐓 𝐎 𝐑 𝐌");
            //activity.SetType(ActivityTypes.Playing);
            //activity.SetStatusDisplayType(StatusDisplayTypes.Name);
            //activity.SetDetails("Unity " + Application.unityVersion);
            //activity.SetState($"{playModeState}");
            activity.SetTimestamps(timestamps);

            client.UpdateRichPresence(activity, OnRichPresenceUpdated);
        }

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(StopPresence))]
        static void StopPresence()
        {
            if (!is_initialized || client == null)
                return;

            Debug.Log($"{typeof(ShitDiscordAJO)}.SHUTDOWN");

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

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(ProbeDiscordRpcPipes))]
        static void ProbeDiscordRpcPipes()
        {
#if UNITY_EDITOR_WIN
            bool found_pipe = false;

            for (int i = 0; i < 10; i++)
            {
                string pipe_name = $"discord-ipc-{i}";

                try
                {
                    using NamedPipeClientStream pipe = new(".", pipe_name, PipeDirection.InOut);
                    pipe.Connect(100);

                    if (pipe.IsConnected)
                    {
                        found_pipe = true;
                        Debug.Log($"{typeof(ShitDiscordAJO)}.PIPE_OK {pipe_name}");
                    }
                }
                catch (TimeoutException)
                {
                    Debug.Log($"{typeof(ShitDiscordAJO)}.PIPE_MISS {pipe_name}");
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"{typeof(ShitDiscordAJO)}.PIPE_ERROR {pipe_name} {exception.GetType().Name}: {exception.Message}");
                }
            }

            if (!found_pipe)
            {
                Debug.LogWarning($"{typeof(ShitDiscordAJO)}.NO_DISCORD_RPC_PIPE_FOUND. Discord desktop is likely not exposing RPC to the Unity editor.");
            }
#else
            Debug.LogWarning($"{typeof(ShitDiscordAJO)}.{nameof(ProbeDiscordRpcPipes)} is only implemented for the Windows Unity editor.");
#endif
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
            Debug.Log($"{typeof(ShitDiscordAJO)}.STATUS status={status} error={error} detail={errorDetail}");
        }

        static void OnRichPresenceUpdated(ClientResult result)
        {
            try
            {
                if (result.Successful())
                {
                    Debug.Log($"{typeof(ShitDiscordAJO)}.RICH_PRESENCE_UPDATED {result}");
                    return;
                }

                if (result == null)
                    Debug.LogError($"{typeof(ShitDiscordAJO)}.RICH_PRESENCE_UPDATE_FAILED {nameof(result)}={result}");
                else
                    Debug.LogError($"{typeof(ShitDiscordAJO)}.RICH_PRESENCE_UPDATE_FAILED type={result.Type()} status={result.Status()} code={result.ErrorCode()} error={result.Error()} details={result}");
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
                    Debug.LogWarning($"{typeof(ShitDiscordAJO)}.CLIENT_USER_UNAVAILABLE type={result.Type()} status={result.Status()} code={result.ErrorCode()} error={result.Error()} details={result}");
                    return;
                }
                else if (user == null)
                    Debug.LogWarning($"{typeof(ShitDiscordAJO)}.CLIENT_USER_NULL");
                else
                    Debug.Log($"{typeof(ShitDiscordAJO)}.CLIENT_USER display_name={user.DisplayName()} id={user.Id()}");
            }
            finally
            {
                user?.Dispose();
                result.Dispose();
            }
        }
    }
}
#endif
