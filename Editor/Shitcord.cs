using System;
using _ARK_;
using Discord.Sdk;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    [InitializeOnLoad]
    partial class Shitcord : ArkComponent2
    {
        [NoAutoStaticsCleanup] static Client client;
        [NoAutoStaticsCleanup] static ulong client_application_id;
        [NoAutoStaticsCleanup] static ulong timestamp_editorStart;
        [NoAutoStaticsCleanup] static PlayModeStateChange playModeState;
        [NoAutoStaticsCleanup] static bool is_stopped;
        [NoAutoStaticsCleanup] static bool presence_confirmed;
        [NoAutoStaticsCleanup] static double next_presence_update;
        [NoAutoStaticsCleanup] static string last_presence_error;
        const string button_prefixe = "Assets/" + nameof(_DISCORD_) + "/";
        const string stopped_session_key = "_DISCORD_.Shitcord.Stopped";
        const double retry_delay = 10;
        const double refresh_delay = 60;

        //----------------------------------------------------------------------------------------------------------

        static Shitcord() => InitializeEditor();

        static void InitializeEditor()
        {
            timestamp_editorStart = (ulong)DateTimeOffset.UtcNow.AddSeconds(-EditorApplication.timeSinceStartup).ToUnixTimeSeconds();
            playModeState = EditorApplication.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode;
            is_stopped = SessionState.GetBool(stopped_session_key, false);
            next_presence_update = 0;

            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting -= OnQuitEditor;
            EditorApplication.quitting += OnQuitEditor;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(button_prefixe + nameof(StartPresence))]
        static void StartPresence()
        {
            SessionState.SetBool(stopped_session_key, false);
            is_stopped = false;
            next_presence_update = 0;
        }

        [MenuItem(button_prefixe + nameof(UpdatePresence))]
        static void UpdatePresence() => next_presence_update = 0;

        [MenuItem(button_prefixe + nameof(StopPresence))]
        static void StopPresence()
        {
            SessionState.SetBool(stopped_session_key, true);
            is_stopped = true;
            ShutdownPresence();
        }

        [MenuItem(button_prefixe + nameof(RestartPresence))]
        static void RestartPresence()
        {
            ShutdownPresence();
            StartPresence();
        }

        //----------------------------------------------------------------------------------------------------------

        static void OnBeforeAssemblyReload()
        {
            is_stopped = true;
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            ShutdownPresence();
        }

        static void OnQuitEditor()
        {
            is_stopped = true;
            ShutdownPresence();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            playModeState = state;
            UpdatePresence();
        }

        static void OnEditorUpdate()
        {
            if (is_stopped || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            try
            {
                if (client != null)
                    NativeMethods.Discord_RunCallbacks();

                if (EditorApplication.timeSinceStartup < next_presence_update)
                    return;

                next_presence_update = EditorApplication.timeSinceStartup + retry_delay;
                if (!TryLoadHText())
                    return;

                if (!activate_presence)
                {
                    ShutdownPresence();
                    next_presence_update = EditorApplication.timeSinceStartup + refresh_delay;
                    return;
                }

                if (!TryLoadRText())
                    return;

                if (application_id == 0)
                {
                    ReportPresenceError("RICH_PRESENCE_UPDATE_FAILED application_id=0");
                    return;
                }

                if (client != null && client_application_id != application_id)
                    ShutdownPresence();

                if (client == null)
                {
                    Debug.Log($"{typeof(Shitcord)}.START_PRESENCE");
                    client = new Client();
                    client_application_id = application_id;
                    Client currentClient = client;
                    client.SetApplicationId(application_id);
                    client.SetStatusChangedCallback((status, error, detail) => OnClientStatusChanged(currentClient, status, error, detail));
                    client.GetDiscordClientConnectedUser(application_id, (result, user) => OnDiscordClientConnectedUser(currentClient, result, user));
                }

                next_presence_update = EditorApplication.timeSinceStartup + refresh_delay;
                PublishPresence();
            }
            catch (Exception exception) when (exception is DllNotFoundException || exception is EntryPointNotFoundException || exception is BadImageFormatException)
            {
                is_stopped = true;
                ShutdownPresence();
                Debug.LogException(exception);
            }
            catch (Exception exception)
            {
                next_presence_update = EditorApplication.timeSinceStartup + retry_delay;
                ShutdownPresence();
                ReportPresenceError($"RICH_PRESENCE_UPDATE_FAILED {exception.Message}");
            }
        }

        static void PublishPresence()
        {
            using Activity activity = new();
            using ActivityTimestamps timestamps = new();
            timestamps.SetStart(timestamp_editorStart);

            activity.SetName(application_name);
            activity.SetType(ActivityTypes.Playing);
            activity.SetStatusDisplayType(StatusDisplayTypes.Name);

            if (show_unityVersion)
                activity.SetDetails("Unity " + Application.unityVersion);

            if (show_unityState)
                activity.SetState($"{playModeState}");

            activity.SetTimestamps(timestamps);
            Client currentClient = client;
            currentClient.UpdateRichPresence(activity, result => OnRichPresenceUpdated(currentClient, result));
        }

        static void ShutdownPresence()
        {
            Client previousClient = client;
            client = null;
            client_application_id = 0;
            presence_confirmed = false;
            if (previousClient == null)
                return;

            Debug.Log($"{typeof(Shitcord)}.SHUTDOWN");
            try
            {
                try
                {
                    previousClient.ClearRichPresence();
                    NativeMethods.Discord_RunCallbacks();
                }
                finally
                {
                    previousClient.Dispose();
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        static void ReportPresenceError(string message)
        {
            if (last_presence_error == message)
                return;

            last_presence_error = message;
            Debug.LogWarning($"{typeof(Shitcord)}.{message}");
        }

        //----------------------------------------------------------------------------------------------------------

        static void OnClientStatusChanged(Client source, Client.Status status, Client.Error error, int errorDetail)
        {
            if (!ReferenceEquals(source, client))
                return;

            Debug.Log($"{typeof(Shitcord)}.STATUS status={status} error={error} detail={errorDetail}");
            if (status == Client.Status.Ready)
                UpdatePresence();
        }

        static void OnRichPresenceUpdated(Client source, ClientResult result)
        {
            try
            {
                if (!ReferenceEquals(source, client))
                    return;

                if (result != null && result.Successful())
                {
                    if (!presence_confirmed || last_presence_error != null)
                        Debug.Log($"{typeof(Shitcord)}.RICH_PRESENCE_UPDATED {result}");

                    presence_confirmed = true;
                    last_presence_error = null;
                    return;
                }

                presence_confirmed = false;
                next_presence_update = EditorApplication.timeSinceStartup + retry_delay;
                if (result == null)
                    ReportPresenceError("RICH_PRESENCE_UPDATE_FAILED result=null");
                else
                    ReportPresenceError($"RICH_PRESENCE_UPDATE_FAILED type={result.Type()} status={result.Status()} code={result.ErrorCode()} error={result.Error()} details={result}");
            }
            finally
            {
                result?.Dispose();
            }
        }

        static void OnDiscordClientConnectedUser(Client source, ClientResult result, UserHandle user)
        {
            try
            {
                if (!ReferenceEquals(source, client))
                    return;

                if (result == null || !result.Successful())
                    Debug.LogWarning($"{typeof(Shitcord)}.CLIENT_USER_UNAVAILABLE {result}");
                else if (user == null)
                    Debug.LogWarning($"{typeof(Shitcord)}.CLIENT_USER_NULL");
                else
                    Debug.Log($"{typeof(Shitcord)}.CLIENT_USER display_name={user.DisplayName()} id={user.Id()}");
            }
            finally
            {
                user?.Dispose();
                result?.Dispose();
            }
        }
    }
}
