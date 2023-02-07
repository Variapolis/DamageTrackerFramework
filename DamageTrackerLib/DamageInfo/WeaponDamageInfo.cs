using System;

namespace DamageTrackerLib.DamageInfo
{
    /// <summary>
    /// Holds info on the Weapon/Thing that damaged the Ped.
    /// </summary>
    [Serializable]
    public struct WeaponDamageInfo
    {
        public WeaponHash Hash;
        public DamageType Type;
        public DamageGroup Group;
    }
}