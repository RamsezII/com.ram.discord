using _ARK_;
using Unity.Scripting.LifecycleManagement;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord
    {
        [AutoStaticsCleanup, HField] static bool activate_presence;

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenHText))]
        static void OpenHText()
        {
            string hpath = NUCLEOR.GetHomeJSonPath<Shitcord>();
            Application.OpenURL(hpath);
        }
    }
}