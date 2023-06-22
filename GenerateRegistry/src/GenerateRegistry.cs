using HarmonyLib;
using Modding;
using Ships;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using Utility;
using NebulousConquestHelper;
using Bundles;
using System.Reflection;

namespace GenerateRegistry
{
    public class GenerateRegistry : IModEntryPoint
    {
        public void PostLoad()
        {
            Harmony harmony = new Harmony("nebulous.generate-registry");
            harmony.PatchAll();

            ComponentRegistry cRegistry = new ComponentRegistry();
            cRegistry.Components = new List<NebulousConquestHelper.Component>();
            foreach (HullComponent comp in BundleManager.Instance.AllComponents)
            {
                NebulousConquestHelper.Component info = new NebulousConquestHelper.Component();

                FieldInfo field1 = typeof(HullPart).GetField("_maxHealth", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo field2 = typeof(HullPart).GetField("_functioningThreshold", BindingFlags.NonPublic | BindingFlags.Instance);

                info.Name = comp.SaveKey;
                info.MaxHP = (float)field1.GetValue(comp);
                info.MinHP = (float)field2.GetValue(comp);

                if (comp.GetType() == typeof(CrewOperatedComponent) || comp.GetType().IsSubclassOf(typeof(CrewOperatedComponent)))
                {
                    FieldInfo field3 = typeof(CrewOperatedComponent).GetField("_crewRequired", BindingFlags.NonPublic | BindingFlags.Instance);
                    info.Crew = (int)field3.GetValue(comp);
                }

                if (comp.GetType() == typeof(DCLockerComponent) || comp.GetType().IsSubclassOf(typeof(DCLockerComponent)))
                {
                    FieldInfo field4 = typeof(DCLockerComponent).GetField("_restoreCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    info.Restores = (int)field4.GetValue(comp);
                }

                FieldInfo field5 = typeof(HullPart).GetField("_dcPriority", BindingFlags.NonPublic | BindingFlags.Instance);

                info.Priority = (Ships.Priority)field5.GetValue(comp);

                cRegistry.Components.Add(info);
            }

            FilePath filePath = new FilePath("ComponentRegistry.xml", "Saves/Generated");
            try
            {
                filePath.CreateDirectoryIfNeeded();
                using (FileStream stream = new FileStream(filePath.RelativePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ComponentRegistry));
                    serializer.Serialize(stream, cRegistry);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            MunitionRegistry mRegistry = new MunitionRegistry();
            mRegistry.Munitions = new List<Munition>();
            foreach (Munitions.IMunition ammo in BundleManager.Instance.AllMunitions)
            {
                Munition info = new Munition();

                info.Name = ammo.SaveKey;
                info.PointCost = ammo.PointCost;
                info.PointDivision = ammo.PointDivision;

                mRegistry.Munitions.Add(info);
            }
            
            filePath = new FilePath("MunitionRegistry.xml", "Saves/Generated");
            try
            {
                filePath.CreateDirectoryIfNeeded();
                using (FileStream stream = new FileStream(filePath.RelativePath, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MunitionRegistry));
                    serializer.Serialize(stream, mRegistry);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public void PreLoad()
        {

        }
    }
}
