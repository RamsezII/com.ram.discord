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
        sealed class HFieldAttribute : Attribute
        {
        }

        [HField] static bool activate_presence = true;
        static string GetHTextPath() => ArkMachine.GetHomeJSonPath(typeof(Shitcord));

        //----------------------------------------------------------------------------------------------------------

        [MenuItem(button_prefixe + nameof(OpenHText))]
        static void OpenHText()
        {
            if (!File.Exists(GetHTextPath()))
                SaveHText(log: true);
            Application.OpenURL(GetHTextPath());
        }

        [MenuItem(button_prefixe + nameof(SaveHText))]
        static void SaveHText() => SaveHText(log: true);
        static void SaveHText(in bool log)
        {
            string spath = GetHTextPath();
            JObject jobj = new();
            jobj.WriteFields<HFieldAttribute>(null, typeof(Shitcord));
            jobj.NJSave(spath, log: log);
            AssetDatabase.Refresh();
        }

        [MenuItem(button_prefixe + nameof(LoadHText))]
        static void LoadHText() => LoadHText(log: true);
        static void LoadHText(in bool log)
        {
            string lpath = GetHTextPath();

            if (Util.TryNJRead(lpath, out JObject jobj, force: true, log_success: log))
            {
                jobj.ReadFields<HFieldAttribute>(null, typeof(Shitcord));
                SaveHText(log: false);
            }
            else
                OpenHText();
        }
    }
}