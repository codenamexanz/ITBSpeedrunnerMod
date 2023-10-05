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
     * SpeedrunnerMod - Moves lobby moth jelly and spawnpoints to ladder, detects bad VHS/Hands spawns, gets rid of deafening exit noise
     */
    public class SpeedrunnerModNoBtns : MelonMod
    {
        private string listOfMods = "Active Mods \n";
        private string version = "";
        private string scene = "";

        #region MelonPreferences
        private MelonPreferences_Category category;
        private MelonPreferences_Entry<bool> detectorBool;
        #endregion

        public override void OnInitializeMelon()
        {
            foreach (MelonMod mod in RegisteredMelons)
            {
                listOfMods = listOfMods + mod.Info.Name + " by " + mod.Info.Author + "\n";
            }
            
            category = MelonPreferences.CreateCategory("Speedrunner Mod");
            detectorBool = category.CreateEntry<bool>("detectorBool", true);

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


            #region detectorButton
            if (detectorBool.Value)
            {
                detectorButton = whiteButtonStyle;
            }
            else
            {
                detectorButton = normalButtonStyle;
            }


            if (GUI.Button(new Rect(Screen.width - 380f, 5f, 165f, 20f), "VHS/Hands Detector", detectorButton))
            {
                detectorBool.Value = !detectorBool.Value;
                MelonLogger.Msg(System.ConsoleColor.Green, "Detector bool changed to " + detectorBool.Value);
            }
            #endregion


        }


        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName == "MainMenu")
            {
                MelonEvents.OnGUI.Subscribe(DrawRegisteredMods, 100);
                MelonEvents.OnGUI.Unsubscribe(vhsHandsRuleDetected);
                MelonEvents.OnGUI.Subscribe(CreateButtons, 100);
                MelonEvents.OnGUI.Subscribe(DrawVersion, 100);
            }

            if (sceneName == "MainLevel" || sceneName == "HOTEL_SCENE" || sceneName == "GRASS_ROOMS_SCENE")
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