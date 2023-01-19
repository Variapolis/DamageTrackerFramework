using System;
using Rage;

namespace DamageTrackerLib.DamageInfo
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