using System;

namespace DamageTrackerLib.DamageInfo
{
    [Serializable]
    public struct PedDamageInfo
    {
        public uint PedHandle;
        public int Damage;
        public WeaponDamageInfo WeaponInfo;
        public BoneDamageInfo BoneInfo;
    }
}