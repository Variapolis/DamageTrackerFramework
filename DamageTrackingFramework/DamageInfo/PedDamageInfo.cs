using System;
using Rage;

namespace DamageTrackingFramework.DamageInfo
{
    [Serializable]
    public struct PedDamageInfo
    {
        public Ped Ped;
        public int Damage;
        public WeaponDamageInfo WeaponInfo;
        public BoneDamageInfo BoneInfo;
    }
}