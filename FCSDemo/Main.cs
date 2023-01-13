using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using FCSCommon.Utilities;
using FCSDemo.Buildables;
using FCSDemo.Configuration;
using FCSDemo.Model;
using FMOD.Studio;
using FMODUnity;
using HarmonyLib;
using RootMotion.FinalIK;
using SMLHelper.Handlers;
using static SMLHelper.Utility.AudioUtils;

namespace FCSDemo
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class Main : BaseUnityPlugin
    {

        #region [Declarations]

        public const string
            MODNAME = "FCSDemo",
            AUTHOR = "FieldCreatorsStudios",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";

        internal static Config Configuration { get; private set; }
        internal static ConfigFile BepInExConfigFile { get; private set; }
        #endregion




        private void Awake()
        {
            QuickLogger.Info($"Started patching. Version: {QuickLogger.GetAssemblyVersion(Assembly.GetExecutingAssembly())}");

            try
            {

                BepInExConfigFile = Config;
                QuickLogger.Info($"{Config is null}");

                TomlTypeConverter.AddConverter(typeof(List<ModEntry>), new ListOfModTypeConverter());

                Configuration = new();

                var harmony = new Harmony("com.fcsdemo.fcstudios");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                if (Configuration.Prefabs != null)
                {
                    foreach (ModEntry modEntry in Configuration.Prefabs.Value)
                    {
                        QuickLogger.Info($"Added Prefab {modEntry.ClassID}");
                        modEntry.Prefab = FCSDemoModel.GetPrefabs(modEntry.PrefabName);
                        var prefab = new FCSDemoBuidable(modEntry);
                        prefab.Patch();
                    }
                }
                else
                {
                    QuickLogger.Error($"Failed to load Configuration.");
                }


                RuntimeManager.StudioSystem.getBankList(out Bank[] banks);
                foreach (Bank bank in banks)
                {
                    bank.getPath(out string bankPath);
                    QuickLogger.Info($"bankPath: {bankPath}");
                    bank.getBusList(out Bus[] busArray);
                    foreach (Bus bus in busArray)
                    {
                        bus.getPath(out string busPath);
                        QuickLogger.Info($"busPath: {busPath}");
                    }
                }

                QuickLogger.Info("Finished patching");
            }
            catch (Exception ex)
            {
                QuickLogger.Error(ex);
            }
        }
    }

    internal class ListOfModTypeConverter : TypeConverter
    {
        public ListOfModTypeConverter()
        {
            this.ConvertToString = ConvertToStringFunc;
            this.ConvertToObject = ConvertToObjectFunc;
        }

        private static string ConvertToStringFunc(object o, Type t)
        {
            var g = new List<string>();
            var list = o as List<ModEntry>;
            foreach (var item in list)
            {
                g.Add($"{item.PrefabName}|{item.FriendlyName}|{item.ClassID}|{item.IconName}");
            }
            return string.Join(", ", g.ToArray());
        }

        private static object ConvertToObjectFunc(string s, Type t)
        {
            var g = new List<ModEntry>();
            var list = s.Split(',');
            foreach (var item in list)
            {
                var values = item.Split('|');
                g.Add(new ModEntry
                {
                    PrefabName = values[0],
                    FriendlyName = values[1],
                    ClassID = values[2],
                    IconName = values[3],
                });
            }
            return g;
        }

    }

}