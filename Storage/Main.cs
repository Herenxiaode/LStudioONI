using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Database;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace LStorage
{
    public class Main:UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            //PrimaryElement.MAX_MASS=1000000000000;
            PrimaryElement.MAX_MASS=float.MaxValue;
            //Strings.Add(new string[] {
            //    "STRINGS.BUILDINGS.PREFABS.FILTERSTORAGE1.NAME",
            //    "过滤储存"
            //});//建筑名
            //Strings.Add(new string[] {
            //    "STRINGS.BUILDINGS.PREFABS.FILTERSTORAGE1.EFFECT",
            //    "过滤储存"
            //});//顶行信息
            //Strings.Add(new string[] {
            //    "STRINGS.BUILDINGS.PREFABS.FILTERSTORAGE1.DESC",
            //    "过滤储存"
            //});//底行信息

        }
        //[HarmonyPatch(typeof(MiscStatusItems),"CreateStatusItems")]
        //public class CreateStatusItems
        //{
        //    public static void Postfix(MiscStatusItems __instance)
        //    {
        //        var Item = new StatusItem("TreeFilterableTags1","MISC","",StatusItem.IconType.Info,NotificationType.Neutral,false,OverlayModes.None.ID,true,129022);
        //        Item.resolveStringCallback=delegate (string str,object data)
        //        {
        //            var treeFilterable = (TreeFilterable1)data;
        //            return str.Replace("{Tags}",treeFilterable.GetTagsAsStatus(6));
        //        };
        //        __instance.Add(Item);
        //    }
        //}
        //[ComVisible(false)]
        //public struct DisplayedMod
        //{
        //    public RectTransform rect_transform;
        //    public int mod_index;
        //}
        //public static List<DisplayedMod> GetVisuals(IList displayedMods = null)
        //{
        //    //if(displayedMods==null)
        //    //{
        //    //    displayedMods=Tools.GetDisplayMods();
        //    //}
        //    if(displayedMods==null)return new List<DisplayedMod>();
        //    var nestedType = typeof(ModsScreen).GetNestedType("DisplayedMod",BindingFlags.Public|BindingFlags.NonPublic);
        //    var field = nestedType.GetField("mod_index",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        //    var field2 = nestedType.GetField("rect_transform",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        //    var list = new List<DisplayedMod>();
        //    foreach(object obj in displayedMods)
        //    {
        //        list.Add(new DisplayedMod
        //        {
        //            mod_index=(int)field.GetValue(obj),
        //            rect_transform=(RectTransform)((field2!=null) ? field2.GetValue(obj) : null)
        //        });
        //    }
        //    return list;
        //}
        //[HarmonyPatch(typeof(ModsScreen),"BuildDisplay")]
        //public class ModsScreen_BuildDisplay
        //{
        //    // Token: 0x06000151 RID: 337 RVA: 0x000090E4 File Offset: 0x000072E4
        //    public static void Postfix(Transform ___entryParent,ModsScreen __instance,IList ___displayedMods)
        //    {

        //        //__instance.gameObject.AddText("dddfffad是ddd",TMPro.TextAlignmentOptions.Top,null);
        //        //___entryParent.gameObject.AddText("dggggdd是ddd",TMPro.TextAlignmentOptions.Top,null);
        //        var visuals = GetVisuals(___displayedMods);
        //        foreach(var visual in visuals)
        //        { 
        //            var o = visual.rect_transform.gameObject;
        //            Ony.OxygenNotIncluded.Tools.AddLogo(o,Ony.OxygenNotIncluded.Tools.PatreonLogo,"123","https://www.patreon.com/ony_mods","asd",true,24,24);
        //            //visual.rect_transform.gameObject.AddText("ttttdd是ddd",TMPro.TextAlignmentOptions.Top,null);
        //            //var o=visual.rect_transform.gameObject.AddComponent<KButton>();
        //            //o.
        //            //var p=Ony.OxygenNotIncluded.Tools.CreatePanel("Panela");
                    
        //            Ony.OxygenNotIncluded.Tools.CreateLabel("a","b","c",o,Color.blue);
        //            //Ony.OxygenNotIncluded.Tools.ShowDialog("Dialogaaa","Dialogbbb");
        //        }
        //        Ony.OxygenNotIncluded.Tools.CreateLabel("a","b","c",Game.Instance.gameObject,Color.blue);
        //        Game.Instance.gameObject.AddText("ttttdd是ddd",TMPro.TextAlignmentOptions.Top,null);
        //    }
        //}
        [HarmonyPatch(typeof(GeneratedBuildings),"LoadGeneratedBuildings")]
        public class LoadGeneratedBuildings
        {
            static void Prefix()
            {
                //Strings.Add(new string[] { "STRINGS.BUILDINGS.PREFABS.LAIRFILTER.NAME" });//LAirFilter

                //ModUtil.AddBuildingToPlanScreen("Base", "LGasFilter");
                //ModUtil.AddBuildingToPlanScreen("Food", "LAirFilter");
            }
        }
    }
}
