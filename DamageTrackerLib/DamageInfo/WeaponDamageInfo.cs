using System;

namespace DamageTrackerLib.DamageInfo
{
    [Serializable]
    public struct WeaponDamageInfo
    {
        public WeaponHash Hash;
        public DamageType Type;
        public DamageGroup Group;
    }
}