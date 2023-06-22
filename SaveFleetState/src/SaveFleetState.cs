using Game;
using Game.Orders.Tasks;
using HarmonyLib;
using Modding;
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

        public static bool SaveFleetToFile(SerializedFleet fleet, string folder, bool clean)
        {
            SerializedFleet output = clean ? PrepareSerializedFleet(fleet) : fleet;
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

        // Thanks to Abrams on the discord for this function
        public static SerializedFleet PrepareSerializedFleet(SerializedFleet fleet)
        {
            List<SerializedShip> deadShips = new List<SerializedShip>();

            foreach (SerializedShip ship in fleet.Ships)
            {
                // we need to keep ship.SavedState.Position, otherwise the Testing Range bugs out, and it doesn't matter on other maps
                ship.SavedState.AngularVel = Vector3.zero;
                ship.SavedState.LinearVel = Vector3.zero;
                ship.SavedState.Throttle = MovementSpeed.Full;
                ship.SavedState.NavOrder = null;
                ship.SavedState.WeaponsControl = WeaponsControlStatus.Free;
                ship.SavedState.Rotation = Quaternion.identity;
                ship.SavedState.Orders = null;
                ship.SavedState.MoveStyle = MovementStyle.Direct;
                ship.SavedState.DCState = null;

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
    }
    
    [HarmonyPatch(typeof(SkirmishGameManager), "CoroutineCountdownToEnd")]
    class Patch_SkirmishGameManager_CoroutineCountdownToEnd
    {
        static bool Prefix(ref SkirmishGameManager __instance)
        {
            Debug.Log("SAVEFLEETSTATE :: Match over, saving fleet states");
            DateTime timestamp = SystemClock.now;

            foreach (IPlayer player in __instance.Players)
            {
                if (player.IsOnLocalPlayerTeam)
                {
                    SkirmishPlayer skirmishPlayer = (SkirmishPlayer)player;

                    SaveFleetState.SaveFleetToFile(
                        skirmishPlayer.PlayerFleet.GetSerializable(true),
                        "Saves/Fleets/FleetStates",
                        true
                        );
                    Debug.Log("SAVEFLEETSTATE :: Done saving rolling state of fleet " + skirmishPlayer.PlayerFleet.GetSerializable(true).Name);
                    
                    SaveFleetState.SaveFleetToFile(
                        skirmishPlayer.PlayerFleet.GetSerializable(true),
                        "Saves/BackupStates/" + timestamp.ToString("yyyy-dd-M---HH-mm-ss"),
                        false
                        );
                    Debug.Log("SAVEFLEETSTATE :: Done saving backup state of fleet " + skirmishPlayer.PlayerFleet.GetSerializable(true).Name);
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(OrderTask), "SaveOrderInternal")]
    class Patch_OrderTask_SaveOrderInternal
    {
        static bool Prefix(ref OrderTask __instance, ref OrderTask.SavedOrderTask saved)
        {
            if (saved == null)
            {
                Debug.Log("SAVEFLEETSTATE :: Order of type " + __instance.GetType().Name + " is null and will not be saved");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
