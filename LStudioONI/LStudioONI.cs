//using System;
using System.IO;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Newtonsoft.Json;
using HarmonyLib;
//using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.Options;
//using TUNING;
using UnityEngine;
using KMod;
//using BUILDINGEFFECTS =STRINGS.UI.BUILDINGEFFECTS;

namespace LStudioONI
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Settings
    {
        [Option("倍率", "流量倍率", "类别")]
        [Limit(0, 10)]
        [JsonProperty]
        public int Scale { get; set; } = 10;

        //public Settings()
        //{
        //}
#if DEBUG
#endif
    }
    public class Main : UserMod2
    {
        //static int S = 0;
        public static Settings Settings;
        public static string LogPath;
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            new PLocalization().Register(null);
            //new POptions().RegisterOptions(this, typeof(Settings));
            Settings = POptions.ReadSettings<Settings>() ?? new Settings();

            LogPath=Path.Combine(path, "info.log");
            //oldVersion= Settings.Version;
            //Settings.Version=1;
            //LocString.CreateLocStringKeys(typeof(StringConst), "LStudioONI.");

        }
        [HarmonyPatch(typeof(LegacyModMain), "Load")]
        public class LegacyModMain_Load
        {
            public static void Postfix()
            {
                //S++;
                //Conditioner.TEST += S+"-Load-";
                var t = "";
                foreach (var def in Assets.BuildingDefs) if (def.BuildingComplete)
                    {
                        t +=def.PrefabID + "\t" + def.name + "\t" + def.BuildingComplete.name+ "\r\n";
                        switch (def.name)
                        {
                            case "LiquidConditioner": {
                                //var EnergyConsumer= def.BuildingComplete.gameObject.AddOrGet<EnergyConsumer>();
                                //EnergyConsumer.BaseWattageRating=
                                def.EnergyConsumptionWhenActive=240;
                                def.GeneratorWattageRating=1200;
                                break; 
                            }
                            //case "AirFilter": def.GeneratorWattageRating = 1; break;
                            //case "GasFilter": def.GeneratorWattageRating = 2; break;
                            //case "LiquidFilter": def.GeneratorWattageRating = 3; break;
                            //case "LogicGateFILTER": def.GeneratorWattageRating = 4; break;
                            //case "SolidFilter":     def.GeneratorWattageRating = 5; break;
                            //case "LAirFilter":     def.GeneratorWattageRating = 5; break;
                            case "SteamTurbine":
                            case "SteamTurbine2":
                                var o = def.BuildingComplete.AddOrGet<SteamTurbine>();
                                if (o)
                                {
                                    def.GeneratorWattageRating *= Settings.Scale;
                                    //def.GeneratorBaseCapacity = 1001;
                                    o.pumpKGRate *= Settings.Scale;
                                    //Conditioner.TEST += o.pumpKGRate;
                                }
                                break;
                        }
                        //switch (def.BuildingComplete.name)
                        //{
                        //    //case "AirConditionerComplete": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                        //    //case "LiquidConditionerComplete": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                        //    case "SteamTurbineComplete":
                        //    case "SteamTurbine2Complete":
                        //        //Conditioner.TEST += def.name+"-"+ def.BuildingComplete.name;
                        //        var o = def.BuildingComplete.AddOrGet<SteamTurbine>();
                        //        if (o)
                        //        {
                        //            def.GeneratorWattageRating *= Settings.Scale;
                        //            //def.GeneratorBaseCapacity = 1001;
                        //            o.pumpKGRate *= Settings.Scale;
                        //            //Conditioner.TEST += o.pumpKGRate;
                        //        }
                        //        break;
                        //}
                    }
                //new System.IO.TextWriter()
                File.WriteAllText(LogPath,t);
            }
        }
        [HarmonyPatch(typeof(AirConditioner),"UpdateState")]
        public class AirConditioner_UpdateState
        {
            public static void Postfix(AirConditioner __instance,float dt)//,ref float ___envTemp,ref int ___cellCount,ref float ___lowTempLag, ref float ___lastSampleTime,ref int ___cooledAirOutputCell)
            {
                var EnergyConsumer = __instance.gameObject.AddOrGet<EnergyConsumer>();
                EnergyConsumer.BaseWattageRating=240;
            }
        }


        //static T GetValue<T>(object O, string Name) => Traverse.Create(O).Field(Name).GetValue<T>();
        //static Traverse SetValue(object O, string Name, object o) => Traverse.Create(O).Field(Name).SetValue(o);
    }
}
