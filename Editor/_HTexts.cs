using _ARK_;
using _UTIL_;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord : IHomeTexts
    {
        [NJEdit] static bool activate_presence = true;

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenHText))]
        static void OpenHText()
        {
            string hpath = NUCLEOR.GetHomeJSonPath<Shitcord>();
            Application.OpenURL(hpath);
        }
    }
}