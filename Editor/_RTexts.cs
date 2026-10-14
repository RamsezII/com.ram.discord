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
        [NoAutoStaticsCleanup, RField] static ulong application_id;
        [NoAutoStaticsCleanup, RField] static string application_name;
        [NoAutoStaticsCleanup, RField] static bool show_unityVersion;
        [NoAutoStaticsCleanup, RField] static bool show_unityState;

        static string SaveRTextPath() => Path.Combine(Application.dataPath, NUCLEOR.dname_resources, typeof(Shitcord).GetJSonFileName());

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
        static void LoadRText() => TryLoadRText();

        static bool TryLoadRText()
        {
            string path = SaveRTextPath();

            if (!path.TryNJRead(out JObject jobj, log_success: false))
                return false;

            jobj.ReadStaticFields<RFieldAttribute>(typeof(Shitcord));
            return true;
        }
    }
}
