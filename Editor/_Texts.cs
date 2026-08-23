using _ARK_;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord
    {
        static ulong application_id;
        static string application_name;
        static bool show_unityVersion;
        static bool show_unityState;

        static string GetTextPath() => Path.Combine(ArkMachine.DFResources.FullName, typeof(Shitcord).GetJSonFileName());

        //----------------------------------------------------------------------------------------------------------

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(OpenText))]
        static void OpenText() => Application.OpenURL(GetTextPath());

        static void SaveText()
        {
            string spath = GetTextPath();
            JObject jobj = new()
            {
                [nameof(application_id)] = application_id,
                [nameof(application_name)] = application_name,
                [nameof(show_unityVersion)] = show_unityVersion,
                [nameof(show_unityState)] = show_unityState,
            };
            jobj.NJSave(spath);
        }

        static void LoadText(in bool log)
        {
            string lpath = GetTextPath();

            if (!Util.TryNJRead(lpath, out JObject jobj, force: true, log_success: log))
            {
                SaveText();
                Application.OpenURL(lpath);
            }
            else
            {
                jobj.TryRead(nameof(application_id), ref application_id);
                jobj.TryRead(nameof(application_name), ref application_name);
                jobj.TryRead(nameof(show_unityVersion), ref show_unityVersion);
                jobj.TryRead(nameof(show_unityState), ref show_unityState);
            }
        }
    }
}