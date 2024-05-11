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
        [Option("倍率", "流量倍率", null)]
        [Limit(0, 10)]
        [JsonProperty]
        public int Scale { get; set; } = 10;

        //public Settings()
        //{
        //}
    }
#if DEBUG
#endif
    public class Main : UserMod2
    {
        //static int S = 0;
        public static Settings Settings;
        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            new PLocalization().Register(null);
            //new POptions().RegisterOptions(this, typeof(Settings));
            Settings = POptions.ReadSettings<Settings>() ?? new Settings();
            //oldVersion= Settings.Version;
            //Settings.Version=1;
            LocString.CreateLocStringKeys(typeof(StringConst), "LStudioONI.");

        }
        [HarmonyPatch(typeof(AirConditioner), "UpdateState")]
        public class AirConditioner_UpdateState
        {
            public static void Prefix(AirConditioner __instance, float dt)//,ref float ___envTemp,ref int ___cellCount,ref float ___lowTempLag, ref float ___lastSampleTime,ref int ___cooledAirOutputCell)
            {
                var Storage = __instance.GetComponent<Storage>();
                var Control = __instance.GetComponent<Conditioner>();
                foreach (var item in Storage.items)
                {
                    var Component = item.GetComponent<PrimaryElement>();
                    if (Component.Mass > 0f){Control.UpdateItemTemp(Component.Temperature); break;}
                }
            }
        }
        [HarmonyPatch(typeof(AirConditioner), "GetDescriptors")]
        public class AirConditioner_GetDescriptors
        {
            public static void Postfix(AirConditioner __instance, ref List<Descriptor> __result, GameObject go)
            {
                __instance.GetComponent<Conditioner>().GetDescriptors(ref __result);
            }
        }
        [HarmonyPatch(typeof(LegacyModMain), "Load")]
        public class LegacyModMain_Load
        {
            public static void Postfix()
            {
                //S++;
                //Conditioner.TEST += S+"-Load-";
                foreach (var def in Assets.BuildingDefs) if (def.BuildingComplete)
                    {
                        switch (def.BuildingComplete.name)
                        {
                            case "AirConditionerComplete": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                            case "LiquidConditionerComplete": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                            //case "SteamTurbineComplete":
                            //case "SteamTurbine2Complete":
                            //    Conditioner.TEST += def.name+"-"+ def.BuildingComplete.name;
                            //    var o = def.BuildingComplete.AddOrGet<SteamTurbine>();
                            //    if (o)
                            //    {
                            //        def.GeneratorWattageRating *= Settings.Scale;
                            //        //def.GeneratorBaseCapacity = 1001;
                            //        o.pumpKGRate *= Settings.Scale;
                            //        //Conditioner.TEST += o.pumpKGRate;
                            //    }
                            //    break;
                        }
                        //switch (def.name)
                        //{
                        //    //case "AirConditioner": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                        //    //case "LiquidConditioner": def.BuildingComplete.AddOrGet<Conditioner>(); break;
                        //    case "SteamTurbine2":
                        //        Conditioner.TEST += "-" + def.name;
                        //        Conditioner.TEST += "+" + def.BuildingComplete.name;
                        //        if (((object)def as SteamTurbine))
                        //        {
                        //            ((object)def as SteamTurbine).pumpKGRate = 100f;
                        //        }
                        //        break;
                        //}
                    }
            }
        }


        //static T GetValue<T>(object O, string Name) => Traverse.Create(O).Field(Name).GetValue<T>();
        //static Traverse SetValue(object O, string Name, object o) => Traverse.Create(O).Field(Name).SetValue(o);
    }
}
