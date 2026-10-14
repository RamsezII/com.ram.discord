using _ARK_;
using Unity.Scripting.LifecycleManagement;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord
    {
        [NoAutoStaticsCleanup, HField] static bool activate_presence = true;

        static string HTextPath() => NUCLEOR.GetHomeJSonPath<Shitcord>();

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenHText))]
        static void OpenHText()
        {
            Application.OpenURL(HTextPath());
        }

        [MenuItem(button_prefixe + nameof(LoadHText))]
        static void LoadHText() => TryLoadHText();

        static bool TryLoadHText()
        {
            string path = HTextPath();
            if (!File.Exists(path))
            {
                activate_presence = true;
                return true;
            }

            if (!path.TryNJRead(out JObject jobj, log_success: false))
                return false;

            jobj.ReadStaticFields<HFieldAttribute>(typeof(Shitcord));
            return true;
        }
    }
}
