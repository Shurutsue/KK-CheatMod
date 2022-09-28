using BepInEx;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;

namespace Cheat_Mod
{

    internal class CheatMenu : MonoBehaviour
    {
        public SessionSettings CurrentSettings = new();
        private Rect MainWindow;
        private bool MainWindowVisible = true;
        private Vector2 ScrollPosition;
        private float Width = 250f;
        private float Height = 50f;
        private byte CurrentTab = 0;
        private byte SubTab = 0;

        // Kobold Editor Data
        private static Texture2D LineTex;
        private Kobold[] Kobolds;
        private string[] KoboldIDs;
        private Kobold SelectedKobold;
        private StringKoboldGenes CurrentGenes;
        private int SelectedKoboldID;
        private bool SelectingKobold = false;

        // Spawn Menu Data
        private float ReagentAmount = 20f;

        #region[GUI Styles]
        // Button Styles
        private GUIStyle EnabledButton;
        private GUIStyle DisabledButton;
        private GUIStyle GreenButton;
        private GUIStyle RedButton;

        //Label Styles
        private GUIStyle LabelStyle;
        private GUIStyle CenterLabelStyle;

        // Window Title Style
        private GUIStyle TitleStyle;
        #endregion[GUI Styles]

        private void Start()
        {
            MainWindow = new Rect(Screen.width - Width - 50f, 50f, Width, Height);
        }

        private void Update()
        {
            if (UnityInput.Current.GetKeyDown("F1")) MainWindowVisible = !MainWindowVisible;
            if (UnityInput.Current.GetKeyDown(BepInExLoader.GrabOneHotkey.Value)) CurrentSettings.GrabOne = !CurrentSettings.GrabOne;
        }
        // Line function for indication in KoboldEditor
        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            Matrix4x4 matrix = GUI.matrix;
            if (LineTex == null)
            {
                LineTex = new Texture2D(1, 1);
            }

            Color color2 = GUI.color;
            GUI.color = color;
            float num = Vector3.Angle(pointB - pointA, Vector2.right);

            if (pointA.y > pointB.y)
                num = -num;

            GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));
            GUIUtility.RotateAroundPivot(num, pointA);
            GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1f, 1f), LineTex);
            GUI.matrix = matrix;
            GUI.color = color2;
        }
        private void OnGUI()
        {
            if (!MainWindowVisible) return;
            
            // Indicating Line for Kobold Selection
            if (CurrentTab == 1 && SelectedKobold != null && BepInExLoader.IndicatorLine.Value && SelectedKobold != (Kobold)PhotonNetwork.LocalPlayer.TagObject)
            {
                Color lineColor = new(BepInExLoader.ColorR.Value, BepInExLoader.ColorG.Value, BepInExLoader.ColorB.Value, BepInExLoader.ColorA.Value);
                Vector3 ScreenPos = Camera.main.WorldToScreenPoint(SelectedKobold.transform.position);
                if(ScreenPos.z > 0f)
                    DrawLine(new Vector2(MainWindow.x+2, MainWindow.y+2), new Vector2(ScreenPos.x, (float)Screen.height - ScreenPos.y), lineColor, 2f);
            }

            #region[GUI Styles creation]
            // Button Styles
            EnabledButton = new GUIStyle(GUI.skin.button);
            EnabledButton.normal.textColor = Color.white;
            EnabledButton.hover.textColor = Color.yellow;
            EnabledButton.active.textColor = Color.green;
            EnabledButton.alignment = TextAnchor.MiddleCenter;

            DisabledButton = new GUIStyle(GUI.skin.button);
            DisabledButton.normal.textColor = Color.gray;
            DisabledButton.hover.textColor = Color.gray;
            DisabledButton.active.textColor = Color.red;
            DisabledButton.alignment = TextAnchor.MiddleCenter;

            GreenButton = new GUIStyle(GUI.skin.button);
            GreenButton.normal.textColor = Color.green;
            GreenButton.alignment = TextAnchor.MiddleCenter;

            RedButton = new GUIStyle(GUI.skin.button);
            RedButton.normal.textColor = Color.red;
            RedButton.alignment = TextAnchor.MiddleCenter;

            // Label Styles
            LabelStyle = new GUIStyle();
            LabelStyle.normal.textColor = Color.white;
            LabelStyle.alignment = TextAnchor.MiddleLeft;

            CenterLabelStyle = new GUIStyle();
            CenterLabelStyle.normal.textColor = Color.white;
            CenterLabelStyle.alignment = TextAnchor.MiddleCenter;

            // Window Title Styles

            TitleStyle = new GUIStyle(GUI.skin.window);
            TitleStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f, 1f);

            #endregion[GUI Styles creation]

            // TODO: A display for the GrabOne setting

            if (Event.current.type == EventType.Layout)
            {
                GUI.backgroundColor = Color.black;

                MainWindow = new Rect(MainWindow.x, MainWindow.y, Width, Height);
                MainWindow = GUILayout.Window(0, MainWindow, new GUI.WindowFunction(RenderUI), $"CheatMod v{PluginInfo.PLUGIN_VERSION}", TitleStyle);
                
            }
        }

        private void RenderUI(int id)
        {
            // Cheats refresher
            bool MasterClientOrCheatsAllowed = CurrentSettings.CheatsAllowed || PhotonNetwork.CurrentRoom != null && PhotonNetwork.LocalPlayer.IsMasterClient;
            if (PhotonNetwork.CurrentRoom == null)
            {
                bool GrabOne = CurrentSettings.GrabOne;
                CurrentSettings = new();
                CurrentSettings.GrabOne = GrabOne;
            }

            // Return Button in case not the main tab
            if (CurrentTab != 0 && GUILayout.Button("Return")) CurrentTab = 0;
            GUILayout.Space(10f);

            switch (CurrentTab)
            {
                case 0:
                    #region [Main Tab]
                    if (GUILayout.Button("Kobold Editor")) { CurrentTab = 1; CurrentGenes = new StringKoboldGenes(); }
                    if (GUILayout.Button("Spawn Menu")) CurrentTab = 3;
                    if (GUILayout.Button("About")) CurrentTab = 2;
                    #endregion
                    break;
                case 1:
                    #region [Kobold Editor]
                    GUILayout.Label("Kobold Editor", CenterLabelStyle);
                    GUILayout.Space(10f);
                    GUIStyle style;
                    if (!SelectingKobold)
                    {
                        GUILayout.BeginHorizontal();
                        string sel = (SelectedKobold == null) ? "None" : KoboldIDs[SelectedKoboldID];
                        GUILayout.Label($"Kobold:\n{sel}");
                        bool canSelect = PhotonNetwork.CurrentRoom != null;
                        style = (canSelect) ? GreenButton : RedButton;
                        if(GUILayout.Button("Select", style) && canSelect )
                        {
                            Kobolds = FindObjectsOfType<Kobold>();
                            Player[] players = PhotonNetwork.PlayerList;
                            KoboldIDs = new string[Kobolds.Length];
                            for (int i = 0; i < Kobolds.Length; i++)
                            {
                                Kobold k = Kobolds[i];
                                string kID = Kobolds[i].GetInstanceID().ToString();
                                foreach (Player p in players)
                                {
                                    if (k == (Kobold)p.TagObject) kID += $" ({p.NickName})";
                                }
                                kID += (k.photonView.IsMine && MasterClientOrCheatsAllowed) ? "[Editable]" : "[Not Editable]";
                                KoboldIDs[i] = kID;
                            }
                            if (Kobolds.Length > 0)
                            {
                                SelectedKobold = null;
                                SelectedKoboldID = -1;
                                SelectingKobold = true;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    else {
                        GUILayout.Label("Kobold:");
                        ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(50f));

                        int selResult = -1;
                        for (int i = 0; i < KoboldIDs.Length; i++)
                        {
                            if (GUILayout.Button(KoboldIDs[i])) SelectionBtnPressed(i, ref selResult);
                        }
                        GUILayout.EndScrollView();
                        if (selResult != SelectedKoboldID || Kobolds.Length == 1)
                        {
                            SelectedKoboldID = (selResult >= 0) ? selResult : 0;
                            SelectedKobold = Kobolds[SelectedKoboldID];
                            CurrentGenes = new StringKoboldGenes(SelectedKobold.GetGenes());
                            SelectingKobold = false;
                        }
                    }
                    GUILayout.Space(5f);
                    KoboldEditorGeneOption("Max Energy", ref CurrentGenes.maxEnergy);
                    KoboldEditorGeneOption("Base Size", ref CurrentGenes.baseSize);
                    KoboldEditorGeneOption("Fat Size", ref CurrentGenes.fatSize);
                    KoboldEditorGeneOption("Ball Size", ref CurrentGenes.ballSize);
                    KoboldEditorGeneOption("Dick Size", ref CurrentGenes.dickSize);
                    KoboldEditorGeneOption("Breast Size", ref CurrentGenes.breastSize);
                    KoboldEditorGeneOption("Belly Size", ref CurrentGenes.bellySize);
                    KoboldEditorGeneOption("Metab Cap", ref CurrentGenes.metabCap);
                    KoboldEditorGeneOption("Hue", ref CurrentGenes.hue);
                    KoboldEditorGeneOption("Brightness", ref CurrentGenes.brightness);
                    KoboldEditorGeneOption("Dick Equip", ref CurrentGenes.dickEquip);
                    KoboldEditorGeneOption("Dick Thickness", ref CurrentGenes.dickThickness);
                    KoboldEditorGeneOption("Grab Count", ref CurrentGenes.grabCount);
                    GUILayout.Space(5f);

                    bool canUse = SelectedKobold != null && MasterClientOrCheatsAllowed;
                    bool canUsePV = canUse && SelectedKobold.photonView.IsMine;
                    style = (canUse) ? GreenButton : RedButton;
                    GUIStyle stylePV = (canUsePV) ? GreenButton : RedButton;
                    if (GUILayout.Button("Apply Genes", stylePV) && canUsePV)
                        CurrentGenes.SetGenes(SelectedKobold);

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Cum", style) && canUse)
                        SelectedKobold.photonView.RPC(nameof(Kobold.Cum), RpcTarget.All);
                    if (GUILayout.Button("Lactate", style) && canUse)
                        SelectedKobold.photonView.RPC(nameof(Kobold.MilkRoutine), RpcTarget.All);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Clear Stomach", style) && canUse)
                        SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, SelectedKobold.bellyContainer.volume);
                    if (GUILayout.Button("Clear Metabolism", stylePV) && canUsePV)
                        SelectedKobold.metabolizedContents.Spill(SelectedKobold.metabolizedContents.volume);
                    GUILayout.EndHorizontal();
                    #endregion
                    break;
                case 2:
                    #region [About]
                    // TODO
                    #endregion
                    break;
                case 3:
                    #region[SpawnMenu]
                    GUILayout.Label("Spawn Menu", CenterLabelStyle);
                    GUILayout.Space(10f);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Prefabs", (SubTab == 0) ? EnabledButton : DisabledButton) && true) SubTab = 0;
                    if (GUILayout.Button("Reagents", (SubTab == 1) ? EnabledButton : DisabledButton) && true) SubTab = 1;
                    GUILayout.EndHorizontal();
                    switch (SubTab)
                    {
                        case 0:
                            DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
                            if (pool == null) break;
                            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(100));
                            foreach (var kVPair in pool.ResourceCache)
                            {
                                if (GUILayout.Button(kVPair.Key, (MasterClientOrCheatsAllowed) ? EnabledButton : DisabledButton) && MasterClientOrCheatsAllowed) {
                                    Kobold curKobold = (Kobold)PhotonNetwork.LocalPlayer.TagObject;
                                    if(curKobold != null)
                                    {
                                        var kT = curKobold.hip.transform;
                                        PhotonNetwork.InstantiateRoomObject(kVPair.Key, kT.position + kT.forward, Quaternion.identity);
                                        Debug.Log($"Spawned {kVPair.Key}");
                                    }
                                }
                            }
                            GUILayout.EndScrollView();
                            break;
                        case 1:
                            List<ScriptableReagent> Reagents = ReagentDatabase.GetReagents();
                            if (Reagents.Count <= 0) break;
                            GUILayout.Label("Reagent Amount:");
                            GUILayout.BeginHorizontal();
                            GUILayout.Label(Mathf.Floor(ReagentAmount).ToString(), GUILayout.Width(30f));
                            ReagentAmount = GUILayout.HorizontalSlider(ReagentAmount, 0, 1000);
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5f);
                            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(100));
                            foreach(var reagent in Reagents)
                            {
                                if (GUILayout.Button(reagent.name, (MasterClientOrCheatsAllowed) ? EnabledButton : DisabledButton) && MasterClientOrCheatsAllowed)
                                {
                                    Kobold curKobold = (Kobold)PhotonNetwork.LocalPlayer.TagObject;
                                    if (curKobold != null)
                                    {
                                        var kT = curKobold.hip.transform;
                                        GameObject obj = PhotonNetwork.InstantiateRoomObject("Bucket", kT.position + kT.forward, Quaternion.identity);
                                        ReagentContents contents = new();
                                        contents.AddMix(ReagentDatabase.GetReagent(reagent.name).GetReagent(ReagentAmount));
                                        obj.GetPhotonView().RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, contents, curKobold.photonView.ViewID);
                                        Debug.Log($"Spawned {ReagentAmount} {reagent.name}");
                                    }
                                    
                                }
                            }
                            GUILayout.EndScrollView();
                            break;
                        default:
                            SubTab = 0;
                            break;
                    }
                    #endregion
                    break;
                default:
                    Debug.Log($"[{PluginInfo.PLUGIN_GUID}] Unexpected tab index. Returning to main!");
                    CurrentTab = 0;
                    break;
            }
            GUI.DragWindow();
        }

        private void SelectionBtnPressed(int i, ref int Result)
        {
            Result = i;
        }
        private void KoboldEditorGeneOption(string Label, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Label);
            value = GUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }
    }
}
