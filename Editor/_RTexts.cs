using _ARK_;
using Newtonsoft.Json.Linq;
using System.IO;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord
    {
        [AutoStaticsCleanup, RField] static ulong application_id;
        [AutoStaticsCleanup, RField] static string application_name;
        [AutoStaticsCleanup, RField] static bool show_unityVersion;
        [AutoStaticsCleanup, RField] static bool show_unityState;

        static string SaveRTextPath() => Path.Combine(NUCLEOR.DFResources.FullName, typeof(Shitcord).GetJSonFileName());
        static string LoadRTextPath() => typeof(Shitcord).GetJSonFileName_noTXT();

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenRText))]
        static void OpenRText()
        {
            Application.OpenURL(SaveRTextPath());
        }

        [MenuItem(button_prefixe + nameof(SaveRText))]
        static void SaveRText()
        {
            string spath = SaveRTextPath();
            JObject jobj = new();
            jobj.WriteStaticFields<RFieldAttribute>(typeof(Shitcord));
            jobj.NJSave(spath);
            AssetDatabase.Refresh();
        }

        [MenuItem(button_prefixe + nameof(LoadRText))]
        static void LoadRText()
        {
            string lpath = LoadRTextPath();

            if (lpath.TryNJRead_resource(out JObject jobj))
            {
                jobj.ReadStaticFields<RFieldAttribute>(typeof(Shitcord));
                SaveRText();
            }
        }
    }
}