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

        static string SaveTextPath() => Path.Combine(ArkMachine.DFResources.FullName, typeof(Shitcord).GetJSonFileName());
        static string LoadTextPath() => typeof(Shitcord).GetJSonFileName_noTXT();

        //----------------------------------------------------------------------------------------------------------

        [MenuItem("Assets/" + nameof(_DISCORD_) + "/" + nameof(OpenText))]
        static void OpenText()
        {
            SaveText();
            Application.OpenURL(SaveTextPath());
        }

        static void SaveText()
        {
            string spath = SaveTextPath();
            JObject jobj = new()
            {
                [nameof(application_id)] = application_id,
                [nameof(application_name)] = application_name,
                [nameof(show_unityVersion)] = show_unityVersion,
                [nameof(show_unityState)] = show_unityState,
            };
            jobj.NJSave(spath);
            AssetDatabase.Refresh();
        }

        static void LoadText(in bool log)
        {
            string lpath = LoadTextPath();

            if (lpath.TryNJRead_resource(out JObject jobj, log_success: log))
            {
                jobj.TryRead_out(nameof(application_id), out application_id);
                jobj.TryRead_out(nameof(application_name), out application_name);
                jobj.TryRead_out(nameof(show_unityVersion), out show_unityVersion);
                jobj.TryRead_out(nameof(show_unityState), out show_unityState);
                SaveText();
            }
            else
                OpenText();
        }
    }
}