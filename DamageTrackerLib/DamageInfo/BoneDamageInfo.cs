using System;

namespace DamageTrackerLib.DamageInfo
{
    [Serializable]
    public struct BoneDamageInfo
    {
        public BoneId BoneId;
        public Limb Limb;
        public BodyRegion BodyRegion;
    }
}