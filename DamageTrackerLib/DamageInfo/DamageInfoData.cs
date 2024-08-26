using System;

namespace DamageTrackerLib.DamageInfo
{
    /// <summary>
    /// Holds all damage info for a given tick.
    /// </summary>
    [Serializable]
    public struct DamageInfoData
    {
        public PedDamageInfo[] PedDamageInfoList;
        public VehDamageInfo[] VehDamageInfoList;
    }
}