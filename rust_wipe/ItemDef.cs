using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;

namespace rust_wipe
{

    public class Phrase
    {
        public string token { get; set; }
        public string english { get; set; }
    }

    public class IconSprite
    {
        public int instanceID { get; set; }
    }

    public class SoundDefinition
    {
        public int instanceID { get; set; }
    }

    public class FoundCondition
    {
        public double fractionMin { get; set; }
        public double fractionMax { get; set; }
    }

    public class Condition
    {
        public bool enabled { get; set; }
        public double max { get; set; }
        public bool repairable { get; set; }
        public bool maintainMaxCondition { get; set; }
        public FoundCondition foundCondition { get; set; }
    }

    public class SteamItem
    {
        public int instanceID { get; set; }
    }

    public class Parent
    {
        public int instanceID { get; set; }
    }

    public class Panel
    {
        public int instanceID { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemCategory
    {
        Weapon,
        Construction,
        Items,
        Resources,
        Attire,
        Tool,
        Medical,
        Food,
        Ammunition,
        Traps,
        Misc,
        All,
        Common,
        Component,
        Search
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemSelectionPanel
    {
        None,
        Vessel,
        Modifications,
        GunInformation
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ItemSlot
    {
        None = 1,
        Barrel = 2,
        Silencer = 4,
        Scope = 8,
        UnderBarrel = 0x10
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ContentsType
    {
        Generic = 1,
        Liquid = 2
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum AmountType
    {
        Count,
        Millilitre,
        Feet,
        Genetics,
        OxygenSeconds
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Rarity
    {
        None,
        Common,
        Uncommon,
        Rare,
        VeryRare
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Flag
    {
        NoDropping = 1,
        NotStraightToBelt = 2
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TraitFlag
    {
        None = 0,
        Alive = 1,
        Animal = 2,
        Human = 4,
        Interesting = 8,
        Food = 0x10,
        Meat = 0x20,
        Water = 0x20
    }

    public class ItemDefinition
    {
        public int itemid { get; set; }
        public string shortname { get; set; }
        public Phrase displayName { get; set; }
        public Phrase displayDescription { get; set; }
        public IconSprite iconSprite { get; set; }
        public ItemCategory category { get; set; }
        public ItemSelectionPanel selectionPanel { get; set; }
        public int maxDraggable { get; set; }
        public ContentsType itemType { get; set; }
        public AmountType amountType { get; set; }
        public int occupySlots { get; set; }
        public int stackable { get; set; }
        public bool quickDespawn { get; set; }
        public Rarity rarity { get; set; }
        public bool spawnAsBlueprint { get; set; }
        public SoundDefinition inventorySelectSound { get; set; }
        public SoundDefinition inventoryGrabSound { get; set; }
        public SoundDefinition inventoryDropSound { get; set; }
        public SoundDefinition physImpactSoundDef { get; set; }
        public Condition condition { get; set; }
        public bool hidden { get; set; }
        public Flag flags { get; set; }
        public SteamItem steamItem { get; set; }
        public Parent Parent { get; set; }
        public ObjectRef worldModelPrefab { get; set; }
        public TraitFlag Traits { get; set; }
        public Panel panel { get; set; }

        public List<ItemMod> Mods { get; set; } = new List<ItemMod>();
    }

    public class ItemAmount
    {
        public ItemDefinition itemDef { get; set; }

        public float amount { get; set; }

        public int itemid { get; set; }
    }

    public class ItemBlueprint
    {
        public List<ItemAmount> ingredients { get; set; } = new List<ItemAmount>();
        public bool defaultBlueprint { get; set; }
        public bool userCraftable { get; set; }
        public bool isResearchable { get; set; }
        public Rarity rarity { get; set; }
        public int workbenchLevelRequired { get; set; }
        public int scrapRequired { get; set; }
        public int scrapFromRecycle { get; set; }
        public bool NeedsSteamItem { get; set; }
        public int blueprintStackSize { get; set; }
        public float time { get; set; }
        public int amountToCreate { get; set; }
        public string UnlockAchievment { get; set; }
    }

    public class ObjectRef
    {
        public string guid { get; set; }
    }

    public class MCurve
    {
        public string serializedVersion { get; set; }
        public double time { get; set; }
        public double value { get; set; }
        public double inSlope { get; set; }
        public double outSlope { get; set; }
        public int tangentMode { get; set; }
        public int weightedMode { get; set; }
        public double inWeight { get; set; }
        public double outWeight { get; set; }
    }

    public class SpreadScalar
    {
        public string serializedVersion { get; set; }
        public List<MCurve> m_Curve { get; set; }
        public int m_PreInfinity { get; set; }
        public int m_PostInfinity { get; set; }
        public int m_RotationOrder { get; set; }
    }

    [Flags]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AmmoTypes
    {
        PISTOL_9MM = 1,
        RIFLE_556MM = 2,
        SHOTGUN_12GUAGE = 4,
        BOW_ARROW = 8,
        HANDMADE_SHELL = 0x10,
        ROCKET = 0x20,
        NAILS = 0x40
    }

    public class ItemMod
    {

    }

    public class ItemModProjectileMod
    {

    }

    public class ItemModProjectile : ItemMod
    {
        public ObjectRef projectileObject { get; set; }
        public List<ItemModProjectileMod> mods { get; set; }
        public AmmoTypes ammoType { get; set; }
        public int numProjectiles { get; set; }
        public double projectileSpread { get; set; }
        public double projectileVelocity { get; set; }
        public double projectileVelocitySpread { get; set; }
        public bool useCurve { get; set; }
        public SpreadScalar spreadScalar { get; set; }
        public string category { get; set; }
    }

    public class ItemModBurnable : ItemMod
    {
        public float fuelAmount { get; set; }
        public ItemDefinition byproductItem { get; set; }
        public int byproductAmount { get; set; }
        public float byproductChance { get; set; }
    }

    public class ItemModEntity : ItemMod
    {
        public ObjectRef EntityPrefab { get; set; }
        public string DefaultBone { get; set; }
    }
}
