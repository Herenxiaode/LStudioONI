using System;
using System.Collections.Generic;
using KSerialization;
using UnityEngine;
//using STRINGS;
using HarmonyLib;

namespace LStudioONI
{
	public static class StringConst
	{
		public static LocString LIQUID = "STRINGS.ELEMENTS.STATE.LIQUID";	//液体	STRINGS.ELEMENTS.STATE.LIQUID
		public static LocString GAS = "STRINGS.ELEMENTS.STATE.GAS";     //气体	STRINGS.ELEMENTS.STATE.GAS
		public static LocString TEMPERATURE = "STRINGS.UI.NEWBUILDCATEGORIES.TEMPERATURE.NAME";	//温度	STRINGS.UI.NEWBUILDCATEGORIES.TEMPERATURE.NAME

		public static LocString CONTROL_TITLE = "温度调节";
		public static LocString CONTROL_CHECKBOX = "绝对温度";
		public static LocString CONTROL_CHECKBOX_TOOLTIP = "勾选可调节至指定温度.\n不勾选则和原版一样用相对温度.";
		public static LocString CONTROL_TOOLTIP = "将经过的内容物温度调节至<style=\"KKeyword\">{0}{1}</style>.";

		public static LocString ITEMTTEMP = "{1}<link=\"HEAT\">温度</link>：{0}";
		public static LocString TARGETTEMP = "目标<link=\"HEAT\">温度</link>：{0}";
		public static LocString TARGETWATT = "消耗<link=\"POWER\">电力</link>：{0}";
        public static LocString ITEMTTEMP_TOOLTIPS = "最后一次经过的<style=\"KKeyword\">{1}温度</style>为{0}";
        public static LocString TARGETTEMP_TOOLTIPS = "将管内<style=\"KKeyword\">{1}温度</style>调节至{0}";
        public static LocString TARGETWATT_TOOLTIPS = "当前调节所需<link=\"POWER\">电力</link>{0}";


        public static LocString CALORIES = "<link=\"HEAT\">热量</link>：{0}（近似值）";
		public static LocString CONDITIONING = "调节<link=\"HEAT\">温度</link>：{0}";
		public static LocString TOOLTIPS_CONDITIONING = "调节管内<style=\"KKeyword\">{1}温度</style><b>{0}</b>";
		public static LocString TOOLTIPS_CONDITIONER =string.Concat("基于抽取<style=\"KKeyword\">{2}</style>的<style=\"KKeyword\">量</style>和",
														"<style=\"KKeyword\">比热容</style>进行<style=\"KKeyword\">热量</style>交换\n");
		public static LocString TOOLTIPS_WATER = "使10千克的<link=\"WATER\">水</link><b>{1}</b>会交换<style=\"consumed\">{0}</style>的热量";
		public static LocString TOOLTIPS_OXYGEN = "使1千克的<link=\"OXYGEN\">氧气</link><b>{1}</b>会交换<b>{0}</b>的热量";

    }
	[SerializationConfig(MemberSerialization.OptIn)]
	public class Conditioner : KMonoBehaviour, ISingleSliderControl, ICheckboxControl
	{
		public const float Zero = 273.15f;
		int ISliderControl.SliderDecimalPlaces(int index) => 1;
		float ISliderControl.GetSliderMin(int index) => Enable ? GameUtil.GetConvertedTemperature(0) : -100;
		float ISliderControl.GetSliderMax(int index) => Enable ? GameUtil.GetConvertedTemperature(4000 + Zero) : 100;
		float ISliderControl.GetSliderValue(int index) => Enable ? GameUtil.GetConvertedTemperature(TargetTemp) : RelativeTemp;
		string ISliderControl.GetSliderTooltip(int index) => string.Format(Strings.Get(GetSliderTooltipKey(0)), GameUtil.GetConvertedTemperature(TargetTemp, true), GameUtil.GetTemperatureUnitSuffix());//+ TEST;
		string ISliderControl.SliderTitleKey => "LStudioONI.StringConst.CONTROL_TITLE";
		string ISliderControl.SliderUnits => "  " + GameUtil.GetTemperatureUnitSuffix();
		public string GetSliderTooltipKey(int index) => "LStudioONI.StringConst.CONTROL_TOOLTIP";
		void ISliderControl.SetSliderValue(float value, int index) { if (Enable) UpdateTargetTemp(GameUtil.GetTemperatureConvertedToKelvin(value)); else UpdateRelativeTemp(value); }

		string ICheckboxControl.CheckboxTitleKey => "Unknown";
		string ICheckboxControl.CheckboxLabel => LStudioONI.StringConst.CONTROL_CHECKBOX;
		string ICheckboxControl.CheckboxTooltip => LStudioONI.StringConst.CONTROL_CHECKBOX_TOOLTIP;
		bool ICheckboxControl.GetCheckboxValue() => Enable;
		public void SetCheckboxValue(bool value) => UpdateSlider(value);

		[Serialize]
		public float TargetTemp { get; set; } = Zero + 28;//目标温度
		[Serialize]
		public float ItemTemperature { get; set; } = Zero;
		[Serialize]
		public bool Enable { get; set; }//突然想到,如果是第一次使用,在尚不了解的情况下擅自把原本的相对温度改成绝对温度是不太理智的.例如原本214℃的蒸汽会-14℃调节成200℃,结果直接调节到了mod默认设定的绝对28℃会导致管道损坏.于是又努力了下增加了相对温度和绝对温度的切换选项,并默认回了原版的相对-14℃.
										//[Serialize]
										//public int Version;
		[Serialize]
		public float RelativeTemp { get; set; } = -14;//相对温度
        [MyCmpReq]
        Building Building;
        [MyCmpAdd]
		public CopyBuildingSettings CopyBuildingSettings;

		public AirConditioner AirConditioner => gameObject.AddOrGet<AirConditioner>();
		//public Building Building => gameObject.AddOrGet<Building>();
		public EnergyConsumer EnergyConsumer => gameObject.AddOrGet<EnergyConsumer>();

		int simHandleCopy;
		bool preUpdate;
#if DEBUG
		//int TESTINT = 0;
		public static string TEST = "";
#endif
		//const int NowVersion = 1;

		void UpdateSlider(bool enable)
		{
			if (Enable == enable) return;
			Enable = enable;
			if (preUpdate) return;
			preUpdate = true;
            //Game.Instance.Trigger((int)GameHashes.BuildingStateChanged, AirConditioner.gameObject);
            Game.Instance.Trigger((int)GameHashes.BuildingStateChanged, gameObject);
            //Game.Instance.Trigger((int)GameHashes.UIRefresh, gameObject);
            preUpdate = false;
		}

		void UpdateTargetTemp(float Temp) { SetCheckboxValue(true); TargetTemp = Temp; UpdateTemp(); }
		void UpdateRelativeTemp(float Temp) { RelativeTemp = Temp; UpdateTemp(); }
		public void UpdateItemTemp(float Temp) { ItemTemperature = Temp; UpdateTemp(); }
		void UpdateTemp()
		{
			//float Temp;
			if (Enable) RelativeTemp = AirConditioner.temperatureDelta  = TargetTemp - ItemTemperature;
			else
			{
				AirConditioner.temperatureDelta = RelativeTemp;
				TargetTemp = RelativeTemp + ItemTemperature;
			}

			var StructureTemperature = GetValue<HandleVector<int>.Handle>(AirConditioner, "structureTemperature");
			var Payload = GameComps.StructureTemperatures.GetPayload(StructureTemperature);

			if (simHandleCopy != -1)
			{
				if (Payload.simHandleCopy != -1) simHandleCopy = Payload.simHandleCopy;
				if (RelativeTemp > 0) Payload.simHandleCopy = -1;
				else Payload.simHandleCopy = simHandleCopy;
			}

            EnergyConsumer.BaseWattageRating=Building.Def.EnergyConsumptionWhenActive*(Mathf.Abs(RelativeTemp)/Mathf.Abs(-14f));
   //         if (AirConditioner.isLiquidConditioner)
			//	EnergyConsumer.BaseWattageRating = 1200 * (Mathf.Abs(RelativeTemp) / Mathf.Abs(-14f));
			//else EnergyConsumer.BaseWattageRating = 240f * (Mathf.Abs(RelativeTemp) / Mathf.Abs(-14f));
		}
		public void GetDescriptors(ref List<Descriptor> List)
		{
			//if (!Enable) return;
			List = new List<Descriptor>();
			var isLiquid = AirConditioner.isLiquidConditioner;
			var temp = AirConditioner.temperatureDelta;

			//var element = ElementLoader.FindElementByName(isLiquid ? "Water" : "Oxygen");
			var calories = GameUtil.GetFormattedHeatEnergy(-temp * ElementLoader.FindElementByName(isLiquid ? "Water" : "Oxygen").specificHeatCapacity * (isLiquid ? 10000f : 1000f));
			var formattedTemperature = GameUtil.GetFormattedTemperature(temp, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Relative);
			var ELEMENTS = isLiquid ? STRINGS.ELEMENTS.STATE.LIQUID : STRINGS.ELEMENTS.STATE.GAS;

			if (temp < 0) calories = string.Concat("+", calories);
			else formattedTemperature = string.Concat("+", formattedTemperature);

			//void AddDescriptor(List<Descriptor> o, string t, string tip = null) => o.Add(new Descriptor(t, tip, Descriptor.DescriptorType.Effect, tip == null));
			void AddDesP(List<Descriptor> o, string t, string tip, params object[] p) => o.Add(new Descriptor(string.Format(t ?? "", p), string.Format(tip ?? "", p)));
			//var AddDescriptor2 =o=> o.Add(new Descriptor(t, null, Descriptor.DescriptorType.Effect, true));

			//AddDescriptor(List, string.Format(StringConst.CALORIES, calories, formattedTemperature),
			//					string.Concat(string.Format(StringConst.TOOLTIPS_CONDITIONER, ELEMENTS),
			//								string.Format(isLiquid ? StringConst.TOOLTIPS_WATER : StringConst.TOOLTIPS_OXYGEN, calories, formattedTemperature)));//热量
			AddDesP(List, StringConst.CALORIES, string.Concat(StringConst.TOOLTIPS_CONDITIONER, isLiquid ? StringConst.TOOLTIPS_WATER : StringConst.TOOLTIPS_OXYGEN), calories, formattedTemperature, ELEMENTS);//热量AddDesP(List, StringConst.ITEMTTEMP, StringConst.ITEMTTEMP_TOOLTIPS, GameUtil.GetFormattedTemperature(ItemTemperature), ELEMENTS);//内容温度
			AddDesP(List, StringConst.ITEMTTEMP, StringConst.ITEMTTEMP_TOOLTIPS, GameUtil.GetFormattedTemperature(ItemTemperature), ELEMENTS);//内容温度
			if (Enable) AddDesP(List, StringConst.TARGETTEMP, StringConst.TARGETTEMP_TOOLTIPS, GameUtil.GetFormattedTemperature(TargetTemp), ELEMENTS);//目标温度
			else AddDesP(List, StringConst.CONDITIONING, StringConst.TOOLTIPS_CONDITIONING, formattedTemperature, ELEMENTS);//调节温度
			AddDesP(List, StringConst.TARGETWATT, StringConst.TARGETWATT_TOOLTIPS, GameUtil.GetFormattedWattage(EnergyConsumer.BaseWattageRating));//消耗电力

#if DEBUG
			AddDesP(List, TEST,null);
			//TESTINT++; TEST = "" + TESTINT;
#endif
		}
		//protected override void OnPrefabInit()//绘制,会调用GetDescriptors
		//{
		//	base.OnPrefabInit();

		//}
        protected override void OnSpawn()
		{
			base.OnSpawn();
			Subscribe((int)GameHashes.CopySettings, o =>
			{
				var Control = (o as GameObject)?.GetComponent<Conditioner>();
				if (Control) UpdateTargetTemp(Control.TargetTemp);
			});
			if (ItemTemperature != Zero) UpdateSlider(true);
			UpdateTemp();

            //bool flag = oldModVersion == 0 && Math.Abs(TargetTemp - 293.15f) > 0.01f;
            //if (flag)TargetTemp = GameUtil.GetTemperatureConvertedToKelvin(TargetTemp);
            //oldModVersion = modVersion;
        }

        //[Serialize]
        //public int oldModVersion;
        //private readonly int modVersion = 1;

		static T GetValue<T>(object O, string Name) => Traverse.Create(O).Field(Name).GetValue<T>();

	}
}
