using _ARK_;
using _UTIL_;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _DISCORD_
{
    partial class Shitcord
    {
        [AttributeUsage(AttributeTargets.Field)]
        sealed class RFieldAttribute : Attribute
        {
        }

        [RField] static ulong application_id;
        [RField] static string application_name;
        [RField] static bool show_unityVersion;
        [RField] static bool show_unityState;

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
            jobj.WriteFields<RFieldAttribute>(null, typeof(Shitcord));
            jobj.NJSave(spath);
            AssetDatabase.Refresh();
        }

        [MenuItem(button_prefixe + nameof(LoadRText))]
        static void LoadRText()
        {
            string lpath = LoadRTextPath();

            if (lpath.TryNJRead_resource(out JObject jobj))
            {
                jobj.ReadFields<RFieldAttribute>(null, typeof(Shitcord));
                SaveRText();
            }
            else
                OpenRText();
        }
    }
}