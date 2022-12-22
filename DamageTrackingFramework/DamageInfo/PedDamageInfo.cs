namespace DamageTrackingFramework.DamageInfo
{
    public struct PedDamageInfo
    {
        public int Damage;
        public WeaponDamageInfo WeaponInfo;
        public BoneDamageInfo BoneInfo;
    }

    public struct WeaponDamageInfo
    {
        public WeaponHash Hash;
        public string Name;
        public DamageType Type;
        public DamageGroup Group;
    }

    public struct BoneDamageInfo
    {
        public BoneId BoneId;
        public Limb Limb;
        public BodyRegion BodyRegion;
    }
}