using System;
using System.Collections.Generic;
using TUNING;
//using STRINGS;
using UnityEngine;
//using KSerialization;
//using System.Linq;
//using System.Runtime.Serialization;
//using static LStorage.FilterStorageConfig;
//using static StateMachine;
//using static Grid.Restriction;
//using HarmonyLib;
//using static ModUtil;
//using static UnityEngine.GraphicsBuffer;
//using PeterHan.PLib.Actions;
//using static Operational;
//using static STRINGS.BUILDINGS.PREFABS;
//using static STRINGS.UI.SANDBOXTOOLS.SETTINGS;
using static OverlayModes;

namespace LStorage
{
    public class FilterStorage:KMonoBehaviour//, ICheckboxControl
    {
        class ListIndex
        {
            int Index;
            List<GameObject> List;
            public GameObject Next { get { if(List.Count<1) return null; Index++; return List[Index%=List.Count]; } }
            public static implicit operator ListIndex(List<GameObject> o) => new ListIndex() { List=o };
        }
        abstract class IConduitManager
        {
            public abstract class Item
            {
                class ItemContents:Item
                {
                    internal ConduitFlow.ConduitContents Value;
                    public override float Mass => Value.mass;
                    public override Tag Tag => ElementLoader.FindElementByHash(Value.element).tag;

                }
                class ItemPickupable:Item
                {
                    internal Pickupable Value;
                    public override float Mass => Value.PrimaryElement.Mass;
                    public override Tag Tag => Value.KPrefabID.PrefabTag;

                }
                public abstract Tag Tag { get; }
                public abstract float Mass { get; }

                public static implicit operator Item(ConduitFlow.ConduitContents o) => new ItemContents() { Value=o };
                public static implicit operator Item(Pickupable o) => o ? new ItemPickupable() { Value=o } : null;
            }

            abstract class ConduitManager:IConduitManager
            {
                public override ConduitType ConduitType { get; }
                internal virtual ConduitFlow Target { get; }
                public override bool HasElement(int Cell) => Target.IsConduitEmpty(Cell);
                public override Item Element(int Cell) => Contents(Cell);
                //public override Item Element(int Cell) => ElementLoader.FindElementByHash(Contents(Cell).element);

                ConduitFlow.ConduitContents Contents(int Cell) => Target.GetContents(Cell);

                public abstract void Storage(Storage Storage,ConduitFlow.ConduitContents Conduit,float mass);
                public override void Storage(Storage Storage,int Cell)
                {
                    var Input = Contents(Cell);
                    var mass = PrimaryElement.MAX_MASS;

                    var o = Storage.items.Find(o => o.GetComponent<PrimaryElement>().Element.tag==Element(Cell).Tag);
                    if(o) mass-=o.GetComponent<PrimaryElement>().Mass;

                    this.Storage(Storage,Input,Target.RemoveElement(Cell,mass).mass);
                    Storage.Trigger((int)GameHashes.OnStorageChange,Input.element);
                }
                public override bool Send(Storage Storage,bool OUT,int Cell,GameObject Object)
                {
                    var o = Object?.GetComponent<PrimaryElement>();
                    if(o&&!(OUT^Filter.ContainsTag(o.Element.tag))&&o.Mass>0)
                    {
                        var mass = Target.AddElement(Cell,o.ElementID,o.Mass,o.Temperature,o.DiseaseIdx,o.DiseaseCount);
                        o.ModifyDiseaseCount(-(int)(mass/o.Mass*o.DiseaseCount),"ConduitDispenser.ConduitUpdate");
                        o.Mass-=mass;
                        Storage.Trigger((int)GameHashes.OnStorageChange,o.gameObject);
                        return true;
                    }
                    return false;
                }

            }
            class SolidConduitManager:IConduitManager
            {
                public override ConduitType ConduitType => ConduitType.Solid;
                internal SolidConduitFlow Target => Game.Instance.solidConduitFlow;
                public override bool HasElement(int Cell) => Target.IsConduitEmpty(Cell);
                public override Item Element(int Cell) => Target.GetPickupable(Contents(Cell).pickupableHandle);
                SolidConduitFlow.ConduitContents Contents(int Cell) => Target.GetContents(Cell);

                public override void Storage(Storage Storage,int Cell)
                {
                    Storage.Store(Target.RemovePickupable(Cell)?.gameObject,true);
                }
                static Pickupable T(Pickupable o,float s) { if(o&&o.PrimaryElement.Mass>s) return o.Take(s); return o; }
                public override bool Send(Storage Storage,bool OUT,int Cell,GameObject Object)
                {
                    var p = Object?.GetComponent<Pickupable>();
                    if(p&&(!OUT^Filter.ContainsTag(p.KPrefabID.PrefabTag)))
                    {
                        Target.AddPickupable(Cell,T(p,20));
                        return true;
                    }
                    return false;
                }
            }
            class LiquidConduitManager:ConduitManager
            {
                public override ConduitType ConduitType => ConduitType.Liquid;
                internal override ConduitFlow Target => Game.Instance.liquidConduitFlow;
                public override void Storage(Storage Storage,ConduitFlow.ConduitContents Conduit,float mass)
                {
                    Storage.AddLiquid(Conduit.element,mass,Conduit.temperature,Conduit.diseaseIdx,(int)(Conduit.diseaseCount*(mass/Conduit.mass)),true,false);
                }
            }
            class GasConduitManager:ConduitManager
            {
                public override ConduitType ConduitType => ConduitType.Gas;
                internal override ConduitFlow Target => Game.Instance.gasConduitFlow;
                public override void Storage(Storage Storage,ConduitFlow.ConduitContents Conduit,float mass)
                {
                    Storage.AddGasChunk(Conduit.element,mass,Conduit.temperature,Conduit.diseaseIdx,(int)(Conduit.diseaseCount*(mass/Conduit.mass)),true,false);
                }
            }
            public TreeFilterable Filter { get; set; }
            public abstract ConduitType ConduitType { get; }
            public abstract bool HasElement(int Cell);
            public abstract Item Element(int Cell);
            public abstract void Storage(Storage Storage,int Cell);
            public abstract bool Send(Storage Storage,bool OUT,int Cell,GameObject Object);

            public static IConduitManager Get(ConduitType o) => o==ConduitType.Solid ? new SolidConduitManager() : o==ConduitType.Liquid ? new LiquidConduitManager() : new GasConduitManager();
        }

        [MyCmpReq]
        Building Building;
        [MyCmpReq]
        public TreeFilterable Filter;
        [MyCmpReq]
        public Storage Storage;

        int InputCell;
        int OutputCell;
        ListIndex StorageInput;
        ListIndex StorageOutput;
        public ConduitType conduitType;

        IConduitFlow ConduitManager => conduitType switch
        {
            ConduitType.Solid => Game.Instance.solidConduitFlow,
            ConduitType.Liquid => Game.Instance.liquidConduitFlow,
            ConduitType.Gas => Game.Instance.gasConduitFlow,
            _ => null
        };
        protected override void OnSpawn()
        {
            InputCell=Building.GetUtilityInputCell();
            OutputCell=Building.GetUtilityOutputCell();
            StorageInput=Storage.items;
            StorageOutput=Storage.items;

            ConduitManager.AddConduitUpdater(ConduitUpdate);
            //ConduitManager.AddConduitUpdater(conduitType==ConduitType.Solid?SolidConduitUpdate: ConduitUpdate);


            //            ButtonControls =new ButtonControl[]{
            //	gameObject.AddComponent<ButtonControl>().SetValue("固体","切换至固体筛选",UpdateType),
            //	gameObject.AddComponent<ButtonControl>().SetValue("液体","切换至液体筛选",UpdateType),
            //	gameObject.AddComponent<ButtonControl>().SetValue("气体","切换至气体筛选",UpdateType)
            //};
        }
        protected override void OnCleanUp()
        {
            ConduitManager.RemoveConduitUpdater(ConduitUpdate);
        }
        void ConduitUpdate(float dt)
        {
            var Manager = IConduitManager.Get(conduitType);
            Manager.Filter=Filter;
            var Item = Manager.Element(InputCell);
            if(Item!=null&&Item.Mass>0&&Filter.ContainsTag(Item.Tag)) Manager.Storage(Storage,InputCell);

            var input = Manager.HasElement(InputCell);
            var ouput = Manager.HasElement(OutputCell);
            for(var i = 0;i<Storage.items.Count;i++)
            {
                if(input&&Manager.Send(Storage,false,InputCell,StorageInput.Next)) input=false;
                if(ouput&&Manager.Send(Storage,true,OutputCell,StorageOutput.Next)) ouput=false;
            }
        }
    }
    public class FilterStorageConfig :IBuildingConfig {

        class SolidFilterConfig:FilterStorageConfig
        {
            protected override string ID => "SolidFilterStorage";
            protected override ConduitType ConduitType => ConduitType.Solid;// GameTags.AllCategories
            protected override List<Tag> StorageFilters => new List<Tag>(GameTags.AllCategories);// new List<Tag>(STORAGEFILTERS.SOLID_TRANSFER_ARM_CONVEYABLE);
            public override BuildingDef CreateBuildingDef()
            {
                var def = base.CreateBuildingDef();
                def.InputConduitType=
                def.OutputConduitType=ConduitType;
                StringConst.AddString(ID,"过滤储存","筛选指定物质送入轨道出口或储存,不符合的物质则返还至入口.","...");
                ModUtil.AddBuildingToPlanScreen("Conveyance",ID,"uncategorized","SolidFilter");//"Base"
                return def;
            }
        }
        class LiquidFilterConfig:FilterStorageConfig
        {
            protected override string ID => "LiquidFilterStorage";
            protected override ConduitType ConduitType => ConduitType.Liquid;
            protected override List<Tag> StorageFilters => STORAGEFILTERS.LIQUIDS;
            public override BuildingDef CreateBuildingDef()
            {
                var def = base.CreateBuildingDef();
                def.InputConduitType=
                def.OutputConduitType=ConduitType;
                StringConst.AddString(ID,"液体过滤储存","筛选指定液体送入管道出口或储存,不符合的液体则返还至入口.","...");
                ModUtil.AddBuildingToPlanScreen("Plumbing",ID,"uncategorized","LiquidFilter");//"Base"
                return def;
            }
        }
        class GasFilterConfig:FilterStorageConfig
        {
            protected override string ID => "GasFilterStorage";
            protected override ConduitType ConduitType => ConduitType.Gas;
            protected override List<Tag> StorageFilters => STORAGEFILTERS.GASES;
            public override BuildingDef CreateBuildingDef()
            {
                var def = base.CreateBuildingDef();
                def.InputConduitType=
                def.OutputConduitType=ConduitType;
                StringConst.AddString(ID,"气体过滤储存","筛选指定气体送入管道出口或储存,不符合的气体则返还至入口.","...");
                ModUtil.AddBuildingToPlanScreen("HVAC",ID,"uncategorized","GasFilter");
                return def;
            }
        }

        //public class Checkbox: ICheckboxControl
        //{
        //    string ICheckboxControl.CheckboxTitleKey => "Unknown";
        //    string ICheckboxControl.CheckboxLabel => StringConst.CheckboxLabel;
        //    string ICheckboxControl.CheckboxTooltip => StringConst.CheckboxTooltip;
        //    bool ICheckboxControl.GetCheckboxValue() => Enable;
        //    public void SetCheckboxValue(bool value) => Enable = value;
        //    public void SetInfo()
        //    {

        //    }
        //}
        internal class ButtonControl:KMonoBehaviour, ISidescreenButtonControl
        {
            string ISidescreenButtonControl.SidescreenButtonText => Text;
            string ISidescreenButtonControl.SidescreenButtonTooltip => Tooltip;
            void ISidescreenButtonControl.SetButtonTextOverride(ButtonMenuTextOverride textOverride) { }
            bool ISidescreenButtonControl.SidescreenEnabled() => true;
            bool ISidescreenButtonControl.SidescreenButtonInteractable() => Enabled;
			void ISidescreenButtonControl.OnSidescreenButtonPressed() { if(Action!=null)Action(this); }
            int ISidescreenButtonControl.HorizontalGroupID() => 1;
            int ISidescreenButtonControl.ButtonSideScreenSortOrder() => 1;
			internal bool Enabled =true;
            internal string Text;
			string Tooltip;
            Action<ButtonControl> Action;
            internal ButtonControl SetValue(string t,string tip,Action<ButtonControl> T)
            {
                Text=t;
                Tooltip=tip;
				Action=T;
                return this;
            }
        }

		public bool Enable = false;
		protected virtual string ID => "";
        protected virtual ConduitType ConduitType { get; }
        protected virtual List<Tag> StorageFilters{ get; }

        public override BuildingDef CreateBuildingDef()
        {
            float 建造时间 = 10f;
			float[] 建材质量 = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
			string[] 建材 = MATERIALS.RAW_METALS;
			//string[] 建材 = { "Metal", "RefinedMetal" };
			var EffectorValues = default(EffectorValues);//装饰-噪音
			var Def = BuildingTemplates.CreateBuildingDef(ID, 1, 2,"filterstorage_kanim", 30, 建造时间, 建材质量, 建材, 3200, BuildLocationRule.Anywhere, EffectorValues, EffectorValues, 0.2f);
            Def.ThermalConductivity=0;//导热率
            Def.OverheatTemperature=3273.15f;//过热温度
            //Def.OverheatTemperature=Def.FatalHot=3273.15f;=Def.BaseMeltingPoint

            Def.InputConduitType =
			Def.OutputConduitType = ConduitType;
			//Def.ViewMode =ConduitType==ConduitType.Solid ? SolidConveyor.ID : ConduitType==ConduitType.Liquid ? LiquidConduits.ID : GasConduits.ID; 
            Def.AudioCategory = "HollowMetal";
			Def.UtilityInputOffset = new CellOffset(0, 0);
			Def.UtilityOutputOffset = new CellOffset(0, 1);
            Def.PermittedRotations=PermittedRotations.R360;
            GeneratedBuildings.RegisterWithOverlay(OverlayScreen.GasVentIDs, ID);

            return Def;
		}
		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            //go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery,false);
            //go.AddOrGet<ElementFilter>().portInfo=new ConduitPortInfo(ConduitType.Gas,new CellOffset(0,1));
            //go.AddOrGet<Filterable>().filterElementState=Filterable.ElementState.Gas;

            //go.AddOrGet<SingleCheckboxSideScreen>();
            //go.AddOrGet<TreeFilterableSideScreen>();

            //go.GetComponent<KSelectable>().SetStatusItem(Db.Get().StatusItemCategories.Main,new StatusItem("Filter","BUILDING","",StatusItem.IconType.Info,NotificationType.Neutral,false,OverlayModes.LiquidConduits.ID,true,129022,null),this);
            //go.AddOrGet<Filterable>().filterElementState=Filterable.ElementState.Gas;

            var Filter = go.AddOrGet<TreeFilterable>();
            Filter.dropIncorrectOnFilterChange=false;

            var storage = BuildingTemplates.CreateDefaultStorage(go, false);
			storage.storageFilters =StorageFilters;
			storage.capacityKg = 0;
			storage.showCapacityStatusItem = true;
			storage.useGunForDelivery=false;

            var FilterStorage = go.AddOrGet<FilterStorage>();
            FilterStorage.conduitType = ConduitType;

        }
		public override void DoPostConfigureComplete(GameObject go)
		{
			go.AddOrGetDef<StorageController.Def>();
			go.GetComponent<KPrefabID>().AddTag(GameTags.OverlayBehindConduits, false);
		}

    }
}