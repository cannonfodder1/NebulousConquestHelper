using Game;
using HarmonyLib;
using Modding;
using Munitions;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Utility;

namespace SaveFleetState
{
    public class SaveFleetState : IModEntryPoint
    {
        public void PostLoad()
        {
            Harmony harmony = new Harmony("nebulous.save-fleet-state");
            harmony.PatchAll();
        }

        public void PreLoad()
        {

        }

        public static bool SaveFleetToFile(SerializedFleet fleet, string folder)
        {
            FilePath filePath = new FilePath(fleet.Name + ".conquest", "Saves/Conquest/" + folder);
            Debug.Log("SAVEFLEETSTATE :: Saving fleet state to file: " + filePath.ToString());
            try
            {
                filePath.CreateDirectoryIfNeeded();
                using (FileStream stream = new FileStream(filePath.RelativePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializedFleet));
                    serializer.Serialize(stream, fleet);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("SAVEFLEETSTATE :: Failed to save fleet state to file");
                Debug.LogError(e);
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(SkirmishGameManager), "CoroutineCountdownToEnd")]
    class Patch_SkirmishGameManager_CoroutineCountdownToEnd
    {
        static bool Prefix(ref SkirmishGameManager __instance)
        {
            Debug.Log("SAVEFLEETSTATE :: Match over, saving fleet states");

            foreach (IPlayer player in __instance.Players)
            {
                if (player.IsOnLocalPlayerTeam)
                {
                    SkirmishPlayer skirmishPlayer = (SkirmishPlayer)player;
                    string mapName = __instance.LoadedMap.DisplayName;
                    DateTime timestamp = SystemClock.now;
                    SaveFleetState.SaveFleetToFile(
                        skirmishPlayer.PlayerFleet.GetSerializable(true),
                        /*mapName + " " + */timestamp.ToString("yyyy-dd-M---HH-mm-ss")
                        );
                }
            }

            return true;
        }
    }
}
