using System;

namespace DamageTrackerLib.DamageInfo
{
    /// <summary>
    /// Holds info on the Bone that was damaged on the Ped.
    /// </summary>
    [Serializable]
    public struct BoneDamageInfo
    {
        public BoneId BoneId;
        public Limb Limb;
        public BodyRegion BodyRegion;
    }
}