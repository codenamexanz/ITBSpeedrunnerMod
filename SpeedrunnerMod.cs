using System.IO;
using MelonLoader;
using UnityEngine;
using Il2Cpp;
using HarmonyLib;
using UnityEngine.UI;
using UnityEngine.InputSystem;


namespace SpeedrunnerMod
{
    /*
     * SpeedrunnerMod - Moves lobby moth jelly and spawnpoints to ladder, detects bad VHS/Hands spawns, gets rid of deafening exit noise, adds keybinds to start runs
     */
    public class SpeedrunnerMod : MelonMod
    {
        private string listOfMods = "Active Mods \n";
        private string version = "";
        private string scene = "";
        private static bool tryJoin = false;
        private static GameObject multiStart = null;

        #region changingKeyBools
        private static bool changingExitKey = false;
        private static bool changingSingleKey = false;
        private static bool changingMultiKey = false;
        private static bool changingMultiJoinKey = false;
        private static bool keyChanged = false;
        #endregion

        #region MelonPreferences
        private MelonPreferences_Category category;
        private MelonPreferences_Entry<bool> detectorBool;
        private MelonPreferences_Entry<Key> exitKey;
        private MelonPreferences_Entry<Key> singlePlayerStartKey;
        private MelonPreferences_Entry<Key> multiPlayerStartKey;
        private MelonPreferences_Entry<int> levelStarting;
        private MelonPreferences_Entry<int> difficultyStart;
        private MelonPreferences_Entry<int> multiPeople;
        private MelonPreferences_Entry<Key> multiJoinKey;
        private MelonPreferences_Entry<string> joinName;
        #endregion

        public override void OnInitializeMelon()
        {
            foreach (MelonMod mod in RegisteredMelons)
            {
                listOfMods = listOfMods + mod.Info.Name + " by " + mod.Info.Author + "\n";
            }
            
            #region initCategory
            category = MelonPreferences.CreateCategory("Speedrunner Mod");
            detectorBool = category.CreateEntry<bool>("detectorBool", true);
            exitKey = category.CreateEntry<Key>("exitKey", Key.Delete);
            singlePlayerStartKey = category.CreateEntry<Key>("singlePlayerStartKey", Key.Insert);
            multiPlayerStartKey = category.CreateEntry<Key>("multiPlayerStartKey", Key.Home);
            levelStarting = category.CreateEntry<int>("startingLevel", 0);
            difficultyStart = category.CreateEntry<int>("difficultyStart", 0);
            multiPeople = category.CreateEntry<int>("multiplayerPlayers", 0);
            multiJoinKey = category.CreateEntry<Key>("multiJoinKey", Key.End);
            joinName = category.CreateEntry<string>("multiJoinName", "noobnoob423");
            #endregion

            MelonLogger.Msg(System.ConsoleColor.Green, "detectorBool value is " + detectorBool.Value);
        }

        private void DrawRegisteredMods()
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperRight;
            style.normal.textColor = Color.white;

            GUI.Label(new Rect(Screen.width - 500 - 10, 100, 500, 100), listOfMods, style);
        }

        private void DrawVersion()
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperRight;
            style.normal.textColor = Color.white;

            if (version == "")
            {
                version = GameObject.Find("VersionText").GetComponent<Text>().m_Text;
            }

            GUI.Label(new Rect(Screen.width - 500 - 10, 85, 500, 15), version, style);
        }

        private static void vhsHandsRuleDetected()
        {
            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.UpperCenter;
            style.normal.textColor = Color.red;
            style.fontSize = 40;

            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "BAD VHS/HANDS DETECTED", style);
        }

        public static async Task DetectClockCassette()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            GameObject[] cassettes = GameObject.FindGameObjectsWithTag("Item")
                .Where(obj => obj.name.StartsWith("Cassete") && obj.transform.parent.name == "CASSETS" && obj.transform.rotation == Quaternion.Euler(0, 0, 0))
                .ToArray();

            if (cassettes.Length > 0)
            {
                MelonEvents.OnGUI.Subscribe(vhsHandsRuleDetected, 100);
                MelonLogger.Msg(System.ConsoleColor.Red, "Bad vhs detected!");
            }
        }

        public static async Task DetectPliersHands()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            GameObject[] clockHands = GameObject.FindGameObjectsWithTag("Item")
                .Where(obj => obj.name.StartsWith("ClockHandles") && obj.transform.localPosition == new Vector3(-64.24694f, 0.4681168f, -6.436426f))
                .ToArray();

            if (clockHands.Length > 0)
            {
                MelonEvents.OnGUI.Subscribe(vhsHandsRuleDetected, 100);
                MelonLogger.Msg(System.ConsoleColor.Red, "Bad clock hands detected!");
            }
        }

        public void CreateButtons()
        {
            #region buttonStyles
            GUIStyle detectorButton;
            GUIStyle whiteButtonStyle = new GUIStyle(GUI.skin.button);
            GUIStyle normalButtonStyle = new GUIStyle(GUI.skin.button);

            whiteButtonStyle.normal.background = Texture2D.whiteTexture;
            whiteButtonStyle.normal.textColor = Color.black;

            normalButtonStyle.normal.background = GUI.skin.button.normal.background;
            normalButtonStyle.normal.textColor = GUI.skin.button.normal.textColor;
            #endregion

            #region keyButtonTexts
            String exitButtonText;
            if (changingExitKey)
            {
                exitButtonText = "Press new Key:";
            }
            else
            {
                exitButtonText = "Reset Key: " + exitKey.Value.ToString();
            }

            String singleButtonText;
            if (changingSingleKey)
            {
                singleButtonText = "Press new Key:";
            }
            else
            {
                singleButtonText = "Single Start Key: " + singlePlayerStartKey.Value.ToString();
            }

            String multiButtonText;
            if (changingMultiKey)
            {
                multiButtonText = "Press new Key:";
            }
            else
            {
                multiButtonText = "Multi Start Key: " + multiPlayerStartKey.Value.ToString();
            }

            String multiJoinButtonText;
            if (changingMultiJoinKey)
            {
                multiJoinButtonText = "Press new Key:";
            }
            else
            {
                multiJoinButtonText = "Multi Join Key: " + multiJoinKey.Value.ToString();
            }
            #endregion

            #region detectorButton
            if (detectorBool.Value)
            {
                detectorButton = whiteButtonStyle;
            }
            else
            {
                detectorButton = normalButtonStyle;
            }
            

            if (GUI.Button(new Rect(Screen.width - 450f, 5f, 165f, 20f), "VHS/Hands Detector", detectorButton))
            {
                detectorBool.Value = !detectorBool.Value;
                MelonLogger.Msg(System.ConsoleColor.Green, "Detector bool changed to " + detectorBool.Value);
            }
            #endregion

            #region keyButtons
            if (GUI.Button(new Rect(Screen.width - 450f, 30f, 165f, 20f), exitButtonText))
            {
                if (!changingExitKey)
                {
                    changingExitKey = true;
                }
            }

            if (GUI.Button(new Rect(Screen.width - 450f, 55f, 165f, 20f), singleButtonText))
            {
                if (!changingSingleKey)
                {
                    changingSingleKey = true;
                }
            }

            if (GUI.Button(new Rect(Screen.width - 450f, 80f, 165f, 20f), multiButtonText))
            {
                if (!changingMultiKey)
                {
                    changingMultiKey = true;
                }
            }

            if (GUI.Button(new Rect(Screen.width - 450f, 180f, 165f, 20f), multiJoinButtonText))
            {
                if (!changingMultiJoinKey)
                {
                    changingMultiJoinKey = true;
                }
            }
            #endregion

            #region levelStartButtons
            if (GUI.Button(new Rect(Screen.width - 450f, 105f, 52f, 20f), "Lobby", levelStarting.Value == 0 ? whiteButtonStyle : normalButtonStyle))
            {
                levelStarting.Value = 0;
            }

            if (GUI.Button(new Rect(Screen.width - 393f, 105f, 52f, 20f), "None", levelStarting.Value == 1 ? whiteButtonStyle : normalButtonStyle))
            {
                levelStarting.Value = 1;
            }

            if (GUI.Button(new Rect(Screen.width - 337f, 105f, 52f, 20f), "Hotel", levelStarting.Value == 2 ? whiteButtonStyle : normalButtonStyle))
            {
                levelStarting.Value = 2;
            }
            #endregion

            #region difficultyStartButtons
            if (GUI.Button(new Rect(Screen.width - 450f, 130f, 52f, 20f), "Easy", difficultyStart.Value == 0 ? whiteButtonStyle : normalButtonStyle))
            {
                difficultyStart.Value = 0;
            }

            if (GUI.Button(new Rect(Screen.width - 393f, 130f, 52f, 20f), "Norm", difficultyStart.Value == 1 ? whiteButtonStyle : normalButtonStyle))
            {
                difficultyStart.Value = 1;
            }

            if (GUI.Button(new Rect(Screen.width - 337f, 130f, 52f, 20f), "Hard", difficultyStart.Value == 2 ? whiteButtonStyle : normalButtonStyle))
            {
                difficultyStart.Value = 2;
            }
            #endregion

            #region multiPlayerPeopleButtons
            if (GUI.Button(new Rect(Screen.width - 450f, 155f, 52f, 20f), "2p", multiPeople.Value == 0 ? whiteButtonStyle : normalButtonStyle))
            {
                multiPeople.Value = 0;
            }

            if (GUI.Button(new Rect(Screen.width - 393f, 155f, 52f, 20f), "3p", multiPeople.Value == 1 ? whiteButtonStyle : normalButtonStyle))
            {
                multiPeople.Value = 1;
            }

            if (GUI.Button(new Rect(Screen.width - 337f, 155f, 52f, 20f), "4p", multiPeople.Value == 2 ? whiteButtonStyle : normalButtonStyle))
            {
                multiPeople.Value = 2;
            }
            #endregion

            #region multiJoinName
            joinName.Value = GUI.TextField(new Rect(Screen.width - 450f, 205f, 165f, 20f), joinName.Value);
            #endregion
        }

        public static async Task RefreshServerList(ServerListUI serverList)
        {
            serverList.RefreshServerList();
            await Task.Delay(TimeSpan.FromSeconds(1));
            tryJoin = true;
        }

        public override void OnUpdate()
        {
            #region exitKeyChange
            if (changingExitKey)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key.ToString() != "None" && key.ToString() != exitKey.Value.ToString())
                        {
                            if (Keyboard.current[key].wasPressedThisFrame)
                            {
                                changingExitKey = false;
                                exitKey.Value = key;
                                keyChanged = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region singleKeyChange
            if (changingSingleKey)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key.ToString() != "None" && key.ToString() != singlePlayerStartKey.Value.ToString())
                        {
                            if (Keyboard.current[key].wasPressedThisFrame)
                            {
                                changingSingleKey = false;
                                singlePlayerStartKey.Value = key;
                                keyChanged = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region multiKeyChange
            if (changingMultiKey)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key.ToString() != "None" && key.ToString() != multiPlayerStartKey.Value.ToString())
                        {
                            if (Keyboard.current[key].wasPressedThisFrame)
                            {
                                changingMultiKey = false;
                                multiPlayerStartKey.Value = key;
                                keyChanged = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region multiJoinKeyChange
            if (changingMultiJoinKey)
            {
                if (Keyboard.current.anyKey.wasPressedThisFrame)
                {
                    foreach (Key key in Enum.GetValues(typeof(Key)))
                    {
                        if (key.ToString() != "None" && key.ToString() != multiJoinKey.Value.ToString())
                        {
                            if (Keyboard.current[key].wasPressedThisFrame)
                            {
                                changingMultiJoinKey = false;
                                multiJoinKey.Value = key;
                                keyChanged = true;
                                break;
                            }
                        }
                    }
                }
            }
            #endregion

            #region exitKeyPress
            GameObject canvasExitSearch = null;
            if (scene != "MainMenu")
            {
                if (Keyboard.current[exitKey.Value].wasPressedThisFrame)
                {
                    if (!keyChanged)
                    {
                        GameObject canvas = GameObject.Find("ChatUI").transform.parent.gameObject;
                        if (canvas != null)
                        {
                            for (int i = 0; i < canvas.transform.childCount; i++)
                            {
                                canvasExitSearch = canvas.transform.GetChild(i).gameObject;
                                if (canvasExitSearch.name == "PauseMenu")
                                {
                                    canvasExitSearch.SetActive(true);
                                    break;
                                }
                            }
                            if (canvasExitSearch != null)
                            {
                                Button exitButton = GameObject.Find("ExitBtn").GetComponent<Button>();
                                exitButton.Press();
                            }
                        }
                    }
                    else
                    {
                        keyChanged = false;
                    }
                }
            }
            #endregion

            #region singleKeyPress
            GameObject canvasSingleSearch = null;
            if (scene == "MainMenu")
            {
                if (Keyboard.current[singlePlayerStartKey.Value].wasPressedThisFrame)
                {
                    if (!keyChanged)
                    {
                        GameObject canvas = GameObject.Find("DiscordButton").transform.parent.gameObject;
                        if (canvas != null)
                        {
                            for (int i = 0; i < canvas.transform.childCount; i++)
                            {
                                canvasSingleSearch = canvas.transform.GetChild(i).gameObject;
                                if (canvasSingleSearch.name == "SingleplayerTab")
                                {
                                    canvasSingleSearch.SetActive(true);
                                    break;
                                }
                            }
                            if (canvasSingleSearch != null)
                            {
                                Transform optionsContent = canvasSingleSearch.transform.FindChild("optionsContent");
                                GameObject difficultyToPress = optionsContent.FindChild("Option (3)").GetChild(difficultyStart.Value).gameObject;
                                Button difficultyButton = difficultyToPress.GetComponent<Button>();
                                difficultyButton.Press();
                                if (levelStarting.Value != 1)
                                {
                                    int childIndex = levelStarting.Value == 0 ? 0 : (levelStarting.Value == 2 ? 4 : 0);
                                    GameObject levelToPress = optionsContent.FindChild("LevelSelect (1)").GetChild(childIndex).gameObject;
                                    Button levelButton = levelToPress.GetComponent<Button>();
                                    levelButton.Press();
                                }
                                GameObject startToPress = canvasSingleSearch.transform.FindChild("Footer").FindChild("StartBtn").gameObject;
                                Button singleStartButton = startToPress.GetComponent<Button>();
                                singleStartButton.Press();
                            }
                        }
                    }
                    else
                    {
                        keyChanged = false;
                    }
                }
            }
            #endregion

            #region multiKeyPress
            GameObject canvasMultiSearch = null;
            if (scene == "MainMenu")
            {
                if (Keyboard.current[multiPlayerStartKey.Value].wasPressedThisFrame)
                {
                    if (!keyChanged)
                    {
                        GameObject canvas = GameObject.Find("DiscordButton").transform.parent.gameObject;
                        if (canvas != null)
                        {
                            for (int i = 0; i < canvas.transform.childCount; i++)
                            {
                                canvasMultiSearch = canvas.transform.GetChild(i).gameObject;
                                if (canvasMultiSearch.name == "HostTab")
                                {
                                    canvasMultiSearch.SetActive(true);
                                    break;
                                }
                            }
                            if (canvasMultiSearch != null)
                            {
                                Transform optionsContent = canvasMultiSearch.transform.FindChild("optionsContent");
                                GameObject playersToPress = optionsContent.FindChild("SlotOption_PLAYERS").FindChild("Content").GetChild(multiPeople.Value).gameObject;
                                Button playersButton = playersToPress.GetComponent<Button>();
                                playersButton.Press();
                                GameObject difficultyToPress = optionsContent.FindChild("SlotOption_DIFFICULTY").FindChild("Content (2)").GetChild(difficultyStart.Value).gameObject;
                                Button difficultyButton = difficultyToPress.GetComponent<Button>();
                                difficultyButton.Press();
                                int childIndex = levelStarting.Value == 0 ? 0 : (levelStarting.Value == 2 ? 4 : 0);
                                GameObject levelToPress = optionsContent.FindChild("LevelSelect").GetChild(childIndex).gameObject;
                                Button levelButton = levelToPress.GetComponent<Button>();
                                levelButton.Press();
                                GameObject nameToDo = optionsContent.FindChild("SlotOption_NAME").FindChild("Content (1)").FindChild("InputField").gameObject;
                                InputField inputField = nameToDo.GetComponent<InputField>();
                                String playerName = GameManager.GetPlayerName();
                                inputField.m_Text = "Game of " + playerName;
                                GameObject hostToPress = canvasMultiSearch.transform.FindChild("Footer").FindChild("HostBtn").gameObject;
                                Button hostButton = hostToPress.GetComponent<Button>();
                                hostButton.Press();
                            }
                        }
                    }
                    else
                    {
                        keyChanged = false;
                    }
                }
            }
            #endregion

            #region multiJoinKeyPress
            GameObject canvasMultiStartSearch = null;
            if (scene == "MainMenu")
            {
                if (Keyboard.current[multiJoinKey.Value].wasPressedThisFrame) // This line needs multi shit
                {
                    if (!keyChanged)
                    {
                        GameObject canvas = GameObject.Find("DiscordButton").transform.parent.gameObject;
                        if (canvas != null)
                        {
                            for (int i = 0; i < canvas.transform.childCount; i++)
                            {
                                canvasMultiStartSearch = canvas.transform.GetChild(i).gameObject;
                                multiStart = canvasMultiStartSearch;
                                if (canvasMultiStartSearch.name == "GameMatchmakingUI")
                                {
                                    canvasMultiStartSearch.SetActive(true);
                                    break;
                                }
                            }
                            if (canvasMultiStartSearch != null)
                            {
                                ServerListUI serverList = canvasMultiStartSearch.GetComponent<ServerListUI>();
                                if (serverList != null)
                                {
                                    RefreshServerList(serverList); 
                                }
                            }
                        }
                    }
                    else
                    {
                        keyChanged = false;
                    }
                }
            }
            if (tryJoin)
            {
                ServerListUI serverList = multiStart.GetComponent<ServerListUI>();
                foreach (ServerSlotUI lobby in serverList.m_CurrentGameSlots)
                {
                    Text lobbyName = lobby.nameText;
                    if (lobbyName.m_Text == "Game of " + joinName.Value)
                    {
                        lobby.joinBtn.Press();
                    }
                }
                tryJoin = false;
            }
            #endregion
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu")
            {
                MelonEvents.OnGUI.Subscribe(DrawRegisteredMods, 100);
                MelonEvents.OnGUI.Subscribe(CreateButtons, 100);
                MelonEvents.OnGUI.Unsubscribe(vhsHandsRuleDetected);
                MelonEvents.OnGUI.Subscribe(DrawVersion, 100);
            }

            if (sceneName == "MainLevel" || sceneName == "HOTEL_SCENE")
            {
                MelonEvents.OnGUI.Unsubscribe(DrawRegisteredMods);
                MelonEvents.OnGUI.Unsubscribe(CreateButtons);
                MelonEvents.OnGUI.Unsubscribe(DrawVersion);
                if (detectorBool.Value)
                {
                    DetectClockCassette();
                    DetectPliersHands();
                }
            }

            if (sceneName == "MainLevel")
            {
                GameObject[] mothJellys = GameObject.FindGameObjectsWithTag("Item")
                .Where(obj => obj.name.StartsWith("MothJelly (1)") && obj.transform.parent.parent.name == "__BACKROOMS (LEVEL1)")
                .ToArray();
                GameObject[] spawnPoints = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.StartsWith("PlayerSpawnpoint") && obj.transform.parent.name == "SPAWNPOINTS")
                .ToArray();

                foreach (GameObject mothJelly in mothJellys)
                {
                    mothJelly.transform.position = new Vector3(19, 0, 0);
                }

                foreach (GameObject spawnPoint in spawnPoints)
                {
                    spawnPoint.transform.position = new Vector3(20, 0, -2);
                }
            }
            scene = sceneName;
        }

        [HarmonyPatch(typeof(Button), "Press")]
        class ExitButtonPatch
        {
            [HarmonyPrefix]
            internal static void PressPrefix(Button __instance)
            {
                if (__instance.name == "ExitBtn" && __instance.transform.parent.name == "Options")
                {
                    GameObject levels = GameObject.Find("----------LEVELS---------------");
                    GameObject.Destroy(levels);
                    GameObject hotel = GameObject.Find("--------SCENE------------");
                    GameObject.Destroy(hotel);
                }
            }
        }
    }
} 