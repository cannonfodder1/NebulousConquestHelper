using Game;
using Game.Units;
using HarmonyLib;
using Modding;
using Ships;
using Ships.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;
using Utility;

namespace SaveFleetState
{
    public class SaveFleetState : IModEntryPoint
    {
        public static bool saveInProgress = false;

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
            SerializedFleet output = PrepareSerializedFleet(fleet);
            FilePath filePath = new FilePath(fleet.Name + ".fleet", folder);

            Debug.Log("SAVEFLEETSTATE :: Saving fleet state to filepath: " + filePath.ToString());

            try
            {
                filePath.CreateDirectoryIfNeeded();
                using (FileStream stream = new FileStream(filePath.RelativePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializedFleet));
                    serializer.Serialize(stream, output);
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

        public static SerializedFleet PrepareSerializedFleet(SerializedFleet fleet)
        {
            List<SerializedShip> deadShips = new List<SerializedShip>();

            foreach (SerializedShip ship in fleet.Ships)
            {
                if (ship.SavedState.Eliminated == EliminationReason.Withdrew)
                {
                    ship.SavedState.Eliminated = EliminationReason.NotEliminated;
                    ship.SavedState.Vaporized = false;
                    ship.SavedState.LaunchedLifeboats = false;
                }

                if (ship.SavedState.Eliminated != EliminationReason.NotEliminated)
                {
                    deadShips.Add(ship);
                }
            }

            foreach (var deadShip in deadShips)
            {
                fleet.Ships.Remove(deadShip);
            }

            return fleet;
        }

        public static object GetPrivateValue(object instance, string fieldName, bool baseClass = false)
        {
            Type type = instance.GetType();
            if (baseClass) type = type.BaseType;

            FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(instance);
        }

        public static object GetPrivateProperty(object instance, string fieldName, bool baseClass = false)
        {
            Type type = instance.GetType();
            if (baseClass) type = type.BaseType;

            PropertyInfo property = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return property.GetValue(instance);
        }

        public static SerializedShipState GetShipLimitedState(ShipController instance)
        {
            ShipStatusSummary shipStatus = (ShipStatusSummary)GetPrivateValue(instance, "_shipStatus");
            bool launchedLifeboats = (bool)GetPrivateValue(instance, "_launchedLifeboats");

            SerializedShipState serializedShipState = new SerializedShipState();
            
            serializedShipState.Position = instance.transform.position;
            serializedShipState.Eliminated = shipStatus.Eliminated;
            serializedShipState.Vaporized = instance.IsEliminated && !instance.gameObject.activeSelf;
            serializedShipState.LaunchedLifeboats = launchedLifeboats;
            serializedShipState.Damage = instance.Ship.GetSerializableDamageState();
            serializedShipState.BulkComponents = instance.GetComponent<SaveFileObject>().SaveComponentStates();
            
            return serializedShipState;
        }
    }
    
    [HarmonyPatch(typeof(SkirmishGameManager), "TransitionFinished")]
    class Patch_SkirmishGameManager_TransitionFinished
    {
        static bool Prefix(ref SkirmishGameManager __instance)
        {
            Debug.Log("SAVEFLEETSTATE :: Match over, saving fleet states");
            DateTime timestamp = SystemClock.now;

            if (__instance == null)
            {
                Debug.Log("SAVEFLEETSTATE :: GameManager is null, aborting");
                return true;
            }

            SaveFleetState.saveInProgress = true;

            foreach (IPlayer player in __instance.Players)
            {
                if (player == null)
                {
                    Debug.Log("SAVEFLEETSTATE :: Player is null, skipping");
                    continue;
                }

                if (player.IsSpectator)
                {
                    continue;
                }

                if (player.IsOnLocalPlayerTeam || __instance.LocalPlayer.IsSpectator)
                {
                    SkirmishPlayer skirmishPlayer = (SkirmishPlayer)player;

                    if (skirmishPlayer == null)
                    {
                        Debug.Log("SAVEFLEETSTATE :: SkirmishPlayer is null, skipping");
                        continue;
                    }

                    if (skirmishPlayer.PlayerFleet == null)
                    {
                        Debug.Log("SAVEFLEETSTATE :: PlayerFleet is null, skipping");
                        continue;
                    }
                    
                    if (skirmishPlayer.PlayerFleet.GetSerializable(true) == null)
                    {
                        Debug.Log("SAVEFLEETSTATE :: GetSerializable is null, skipping");
                        continue;
                    }

                    SaveFleetState.SaveFleetToFile(
                        skirmishPlayer.PlayerFleet.GetSerializable(true),
                        "Saves/Fleets/_SavedStates"
                        );
                    Debug.Log("SAVEFLEETSTATE :: Done saving rolling state of fleet " + skirmishPlayer.PlayerFleet.GetSerializable(true).Name);
                    
                    if (timestamp == null)
                    {
                        Debug.Log("SAVEFLEETSTATE :: Timestamp is null, skipping");
                        continue;
                    }

                    SaveFleetState.SaveFleetToFile(
                        skirmishPlayer.PlayerFleet.GetSerializable(true),
                        "Saves/_BackupSavedStates/" + timestamp.ToString("yyyy-dd-M---HH-mm-ss")
                        );
                    Debug.Log("SAVEFLEETSTATE :: Done saving backup state of fleet " + skirmishPlayer.PlayerFleet.GetSerializable(true).Name);
                }
            }

            SaveFleetState.saveInProgress = false;

            return true;
        }
    }

    [HarmonyPatch(typeof(ShipController), "GetSavedState")]
    class Patch_ShipController_GetSavedState
    {
        static bool Prefix(ref ShipController __instance, ref SerializedShipState __result)
        {
            if (SaveFleetState.saveInProgress)
            {
                __result = SaveFleetState.GetShipLimitedState(__instance);
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
