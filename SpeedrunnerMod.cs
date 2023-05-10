using System.IO;
using MelonLoader;
using UnityEngine;
using static UnityEngine.Object;
using Il2Cpp;
using HarmonyLib;
using System.Reflection;

namespace SpeedrunnerMod
{
    /*
     * SpeedrunnerMod - Moves Lobby moth jelly to in front of ladder, along with lobby spawnpoints
     */
    public class SpeedrunnerMod : MelonMod
    {
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
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
                    MelonLogger.Msg(System.ConsoleColor.Cyan, mothJelly.name + " moved to ladder!");
                }

                foreach (GameObject spawnPoint in spawnPoints)
                {
                    spawnPoint.transform.position = new Vector3(20, 0, -2);
                    MelonLogger.Msg(System.ConsoleColor.Cyan, spawnPoint.name + " moved to ladder!");
                }
            }
        }
    }
}