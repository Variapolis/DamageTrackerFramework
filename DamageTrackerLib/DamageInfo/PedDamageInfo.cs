using System;

namespace DamageTrackerLib.DamageInfo
{
    /// <summary>
    /// Holds all info for a Ped's damage.
    /// </summary>
    [Serializable]
    public struct PedDamageInfo
    {
        public uint PedHandle;
        public uint AttackerPedHandle;
        public int Damage;
        public WeaponDamageInfo WeaponInfo;
        public BoneDamageInfo BoneInfo;
    }
}