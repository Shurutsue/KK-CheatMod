using BepInEx;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


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
        private byte SubTabReagents = 0;
        private byte SubTabKoboldEditor = 0;

        private Texture2D GrabOneIcon;
        private Texture2D GrabNormalIcon;

        // Kobold Editor Data
        private static Texture2D LineTex;
        private Kobold[] Kobolds;
        private string[] KoboldIDs;
        private Kobold SelectedKobold;
        private StringKoboldGenes CurrentGenes;
        private int SelectedKoboldID;
        private bool SelectingKobold = false;
        //   --  Stomach Editor Data
        private List<ReagentValues> Reagents = new();
        private bool SelectingReagent = false;
        private short ?SelectedReagentID;
        private string SelectedReagentName = "None";
        private string SelectedReagentVolume = "";

        // Spawn Menu Data
        private float ReagentAmount = 20f;
        private string ReagentContainer = "Bucket";
        private string ReagentSearchText = "";
        private string PrefabSearchText = "";

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

        private Texture2D LoadImageFromResources(System.Drawing.Bitmap bitmap, System.Drawing.Imaging.ImageFormat imageFormat)
        {
            Texture2D texture = new(1,1);
            byte[] imgBytes;
            using (MemoryStream ms = new())
            {
                bitmap.Save(ms, imageFormat);
                imgBytes = ms.ToArray();
            }
            texture.LoadImage(imgBytes);
            return texture;
        }

        private void Start()
        {
            MainWindow = new Rect(Screen.width - Width - 50f, 50f, Width, Height);
            // Load Images
            var format = System.Drawing.Imaging.ImageFormat.Png;
            GrabOneIcon = LoadImageFromResources(Properties.Resources.ico_hand_grab_one, format);
            GrabNormalIcon = LoadImageFromResources(Properties.Resources.ico_hand_grab_normal, format);
        }

        private void Update()
        {
            /* -------------------------------------
             *  Requires work. Need to remove the ability to switch cameras for example.
             * -------------------------------------
             * 
            if (BepInExLoader.CursorWhenVisible.Value && MainWindowVisible)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
            else if (BepInExLoader.CursorWhenVisible.Value && !GameManager.instance.isPaused && Cursor.lockState == CursorLockMode.None && SceneManager.GetActiveScene().name != "MainMenu")
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            */
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
            // Grab One Indicator when activated
            CameraSwitcher CS = GameObject.FindObjectOfType<CameraSwitcher>();
            if (CS != null && GameManager.instance.isPaused == false)
            {
                if (CameraSwitcher.CameraMode.FreeCam != CS.mode)
                {
                    Rect textureRectangle = new(Screen.width - GrabOneIcon.width - 3, Screen.height / 2 - GrabOneIcon.height / 2, GrabOneIcon.width, GrabOneIcon.height);
                    if(CurrentSettings.GrabOne)
                        GUI.DrawTexture(textureRectangle, GrabOneIcon);
                    else
                        GUI.DrawTexture(textureRectangle, GrabNormalIcon);
                    GUIStyle HotkeyIndicatorStyle = new(); HotkeyIndicatorStyle.alignment = TextAnchor.LowerCenter; HotkeyIndicatorStyle.normal.textColor = Color.white;
                    GUI.Label(textureRectangle, BepInExLoader.GrabOneHotkey.Value.ToString(), HotkeyIndicatorStyle);
                }
            }

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
                            Reagents = new List<ReagentValues>();
                            foreach (Reagent r in SelectedKobold.bellyContainer.GetContents())
                            {
                                if (r.volume > 0f) Reagents.Add(new ReagentValues(r));
                            }
                            SelectingKobold = false;
                        }
                    }
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Gene Editor", (SubTabKoboldEditor == 0) ? EnabledButton : DisabledButton)) SubTabKoboldEditor = 0;
                    if (GUILayout.Button("Stomach Editor", (SubTabKoboldEditor == 1) ? EnabledButton : DisabledButton)) SubTabKoboldEditor = 1;
                    GUILayout.EndHorizontal();

                    switch (SubTabKoboldEditor)
                    {
                        case 0:
                            #region[Kobold Editor - Gene Editor]
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
                            KoboldEditorGeneOption("Saturation", ref CurrentGenes.saturation);
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
                            #endregion[Kobold Editor - Gene Editor]
                            break;
                        case 1:
                            #region[Kobold Editor - Stomach Editor]
                            if (!SelectingReagent)
                            {
                                string selectedReagent = (SelectedReagentID == null) ? "None" : $"[{SelectedReagentID}] {SelectedReagentName}:";
                                GUILayout.BeginHorizontal();
                                GUILayout.Label($"Selected:\n{selectedReagent}");
                                if (GUILayout.Button("Select")) SelectingReagent = true;
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Volume:", GUILayout.Width(60f));
                                SelectedReagentVolume = GUILayout.TextField(SelectedReagentVolume);
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button("Add Reagent") && SelectedReagentID != null)
                                {
                                    try
                                    {   // In Try catch block due to conversion
                                        short nid = (short)SelectedReagentID;
                                        float vol = Convert.ToSingle(SelectedReagentVolume);
                                        Reagents.Add(new ReagentValues(vol, SelectedReagentName, nid));
                                    }
                                    catch { SelectedReagentVolume = $"Invalid Input [{SelectedReagentVolume}]"; }
                                }
                                if (GUILayout.Button("Clear Reagents")) Reagents = new List<ReagentValues>();
                                GUILayout.EndHorizontal();
                            }
                            else
                            {
                                GUILayout.Label("Selecting Reagent");
                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Search:");
                                ReagentSearchText = GUILayout.TextField(ReagentSearchText);
                                GUILayout.EndHorizontal();
                                List<ScriptableReagent> AvailableReagents = ReagentDatabase.GetReagents();
                                ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(100f));
                                foreach(ScriptableReagent r in AvailableReagents)
                                {
                                    if (r.name.ToLower().Contains(ReagentSearchText.ToLower()) && GUILayout.Button(r.name)) { SelectingReagent = false; SelectedReagentID = ReagentDatabase.GetID(r); SelectedReagentName = r.name; }
                                }
                                GUILayout.EndScrollView();
                            }
                            
                            GUILayout.Space(5f);
                            List<ReagentValues> reagentsToRemove = new();
                            foreach (ReagentValues r in Reagents)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label($"[{r.ID}] {r.Name}: {r.Volume}");
                                if (GUILayout.Button("Remove", GUILayout.Width(75f))) reagentsToRemove.Add(r);
                                GUILayout.EndHorizontal();
                            }
                            //Enumeration fix
                            foreach (ReagentValues r in reagentsToRemove) Reagents.Remove(r);
                            GUILayout.Space(5f);
                            GUILayout.BeginHorizontal();
                            canUse = SelectedKobold != null && MasterClientOrCheatsAllowed;
                            style = (canUse) ? GreenButton : RedButton;
                            if (GUILayout.Button("Set Stomach", style) && canUse)
                            {
                                SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, SelectedKobold.bellyContainer.volume);
                                ReagentContents reagentContents = new();
                                foreach(ReagentValues r in Reagents)
                                {
                                    Reagent newReagent = new() { id = r.ID, volume = r.Volume };
                                    reagentContents.AddMix(newReagent);
                                }
                                SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.AddMixRPC), RpcTarget.All, reagentContents, -1);
                            }
                            if (GUILayout.Button("Force Set Stomach", style) && canUse)
                            {
                                SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, SelectedKobold.bellyContainer.volume);
                                ReagentContents reagentContents = new();
                                foreach (ReagentValues r in Reagents)
                                {
                                    Reagent newReagent = new() { id = r.ID, volume = r.Volume };
                                    reagentContents.AddMix(newReagent);
                                }
                                SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.ForceMixRPC), RpcTarget.All, reagentContents, -1);
                            }
                            GUILayout.EndHorizontal();
                            if (GUILayout.Button("Clear Stomach", style) && canUse)
                                SelectedKobold.photonView.RPC(nameof(GenericReagentContainer.Spill), RpcTarget.All, SelectedKobold.bellyContainer.volume);
                            #endregion[Kobold Editor - Stomach Editor]
                            break;
                        default:
                            SubTabKoboldEditor = 0;
                            break;
                    }

                    
                    #endregion[Kobold Editor]
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
                    if (GUILayout.Button("Prefabs", (SubTabReagents == 0) ? EnabledButton : DisabledButton) && true) SubTabReagents = 0;
                    if (GUILayout.Button("Reagents", (SubTabReagents == 1) ? EnabledButton : DisabledButton) && true) SubTabReagents = 1;
                    GUILayout.EndHorizontal();
                    DefaultPool pool = PhotonNetwork.PrefabPool as DefaultPool;
                    
                    switch (SubTabReagents)
                    {
                        case 0:
                            if (pool == null) break;
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Search:");
                            PrefabSearchText = GUILayout.TextField(PrefabSearchText);
                            GUILayout.EndHorizontal();
                            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(100));
                            foreach (var kVPair in pool.ResourceCache)
                            {
                                if (kVPair.Key.ToLower().Contains(PrefabSearchText.ToLower()) && GUILayout.Button(kVPair.Key, (PhotonNetwork.IsMasterClient) ? EnabledButton : DisabledButton) && PhotonNetwork.IsMasterClient)
                                {
                                    Kobold curKobold = (Kobold)PhotonNetwork.LocalPlayer.TagObject;
                                    if (curKobold != null)
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
                            GUILayout.Label("Reagent Container to use:");
                            GUILayout.BeginHorizontal();
                            if (pool.ResourceCache.ContainsKey("Bucket") && GUILayout.Button("Bucket", (ReagentContainer == "Bucket") ? EnabledButton : DisabledButton)) ReagentContainer = "Bucket";
                            if (pool.ResourceCache.ContainsKey("Trough") && GUILayout.Button("Trough", (ReagentContainer == "Trough") ? EnabledButton : DisabledButton)) ReagentContainer = "Trough";
                            if (pool.ResourceCache.ContainsKey("WateringCan") && GUILayout.Button("Watering Can", (ReagentContainer == "WateringCan") ? EnabledButton : DisabledButton)) ReagentContainer = "WateringCan";
                            GUILayout.EndHorizontal();
                            GUILayout.Space(5f);
                            GUILayout.BeginHorizontal();
                            GUILayout.Label("Search:");
                            ReagentSearchText = GUILayout.TextField(ReagentSearchText);
                            GUILayout.EndHorizontal();
                            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Height(100));
                            foreach (var reagent in Reagents)
                            {
                                if (reagent.name.ToLower().Contains(ReagentSearchText.ToLower()) && GUILayout.Button(reagent.name, (PhotonNetwork.IsMasterClient) ? EnabledButton : DisabledButton) && PhotonNetwork.IsMasterClient)
                                {
                                    Kobold curKobold = (Kobold)PhotonNetwork.LocalPlayer.TagObject;
                                    if (curKobold != null && pool.ResourceCache.ContainsKey(ReagentContainer))
                                    {
                                        var kT = curKobold.hip.transform;
                                        GameObject obj = PhotonNetwork.InstantiateRoomObject(ReagentContainer, kT.position + kT.forward, Quaternion.identity);
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
                            SubTabReagents = 0;
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
            GUILayout.Label(Label, GUILayout.Width(90f));
            value = GUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }
    }
}
