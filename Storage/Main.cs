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
    public static class StringConst
    {

        //static T Invoke<T>(this object O,string Name) => Traverse.Create(O).Method(Name).GetValue<T>();

        public static LocString CheckboxLabel = "STRINGS.ELEMENTS.STATE.LIQUID";   //液体	STRINGS.ELEMENTS.STATE.LIQUID
        public static LocString CheckboxTooltip = "STRINGS.ELEMENTS.STATE.GAS";     //气体	STRINGS.ELEMENTS.STATE.GAS
        public static LocString TEMPERATURE = "STRINGS.UI.NEWBUILDCATEGORIES.TEMPERATURE.NAME"; //温度	STRINGS.UI.NEWBUILDCATEGORIES.TEMPERATURE.NAME

        static void AddString(string ID,string Name,string Value) => Strings.Add(new string[] { string.Join(".","STRINGS.BUILDINGS.PREFABS",ID,Name),Value });
        public static void AddString(string ID,string Name,string Effect,string Desc)
        {
            ID=ID.ToUpper();
            AddString(ID,"NAME",Name);
            AddString(ID,"EFFECT",Effect);
            AddString(ID,"DESC",Desc);
        }
    }
    public class Main:UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            PrimaryElement.MAX_MASS=float.MaxValue;
        }
    }
}
