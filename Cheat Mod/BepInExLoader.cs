using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using Photon.Pun;
using System;

namespace Cheat_Mod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("KoboldKare.exe")]
    public class BepInExLoader : BaseUnityPlugin
    {
        public static ConfigEntry<bool> IndicatorLine { get; private set; }
        public static ConfigEntry<float> ColorR { get; private set; }
        public static ConfigEntry<float> ColorG { get; private set; }
        public static ConfigEntry<float> ColorB { get; private set; }
        public static ConfigEntry<float> ColorA { get; private set; }
        public static ConfigEntry<KeyCode> GrabOneHotkey { get; private set; }
        public static ConfigEntry<KeyCode> ShowHideHotkey { get; private set; }

        public static GameObject Load { get; private set; }
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        public static bool Initialized { get; private set; } = false;

        private void Awake()
        {
            ShowHideHotkey = Config.Bind(
                "General",
                "ShowHide_HotKey",
                KeyCode.F1,
                "Key Code for the hotkey to use."
                );
            GrabOneHotkey = Config.Bind(
                "General",
                "GrabOne_HotKey",
                KeyCode.H,
                "Key Code for the hotkey to use."
                );
            IndicatorLine = Config.Bind(
                "KoboldEditor.LineIndicator",
                "Line_Enabled",
                true,
                "Turn on/off the indicator line"
                );
            ColorR = Config.Bind(
                "KoboldEditor.LineIndicator",
                "LineColor_R",
                0.75f,
                "Amount of Red to use in the indicator Line for the Kobold Editor (0-1)"
                );
            ColorG = Config.Bind(
                "KoboldEditor.LineIndicator",
                "LineColor_G",
                0.75f,
                "Amount of Green to use in the indicator Line for the Kobold Editor (0-1)"
                );
            ColorB = Config.Bind(
                "KoboldEditor.LineIndicator",
                "LineColor_B",
                0.2f,
                "Amount of Blue to use in the indicator Line for the Kobold Editor (0-1)"
                );
            ColorA = Config.Bind(
                "KoboldEditor.LineIndicator",
                "LineColor_A",
                1f,
                "Amount of visibility to use in the indicator Line for the Kobold Editor (0-1)"
                );


            // Plugin startup logic
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            
        }

        private void Start()
        {
            Load = new GameObject("CheatMod");
            Load.AddComponent<CheatMenu>();
            DontDestroyOnLoad(Load);
            Initialized = true;

        }

    }

    [HarmonyPatch(typeof(Grabber), nameof(Grabber.TryGrab))]
    public class GrabbingPatcher
    {
        [HarmonyPrefix]
        public static bool Prefix(Grabber __instance)
        {
            Kobold playerKobold = (Kobold)PhotonNetwork.LocalPlayer.TagObject;
            if (playerKobold == null) return true;
            int grabCount = Traverse.Create(__instance).Field("maxGrabCount").GetValue<int>();
            if (playerKobold == __instance.player)
            {
                if (BepInExLoader.Load.GetComponent<CheatMenu>().CurrentSettings.GrabOne)
                {
                    __instance.SetMaxGrabCount(1);
                }
                else
                {
                    __instance.SetMaxGrabCount(playerKobold.GetGenes().grabCount);
                }
                return true;
            }
            if (__instance.player.GetGenes().grabCount != grabCount) __instance.SetMaxGrabCount(__instance.player.GetGenes().grabCount);
            return true;
        }
    }

    [HarmonyPatch(typeof(CheatsProcessor), nameof(CheatsProcessor.ProcessCommand), new Type[] {typeof(Kobold), typeof(string)})]
    public class CommandPatcher
    {
        [HarmonyPrefix]
        public static void Prefix(Kobold kobold, string command)
        {
            string[] args = command.Split(' ');
            if (args.Length == 2 && (Kobold)PhotonNetwork.MasterClient.TagObject == kobold)
            {
                if (args[0].ToLower() == "!togglemods" && args[1] == Convert.ToString(PhotonNetwork.LocalPlayer.ActorNumber))
                {
                    SessionSettings currentSettings = BepInExLoader.Load.GetComponent<CheatMenu>().CurrentSettings;
                    currentSettings.CheatsAllowed = !currentSettings.CheatsAllowed;
                    Debug.Log("Toggling Mods for self!");
                }
            }
        }
    }


}
