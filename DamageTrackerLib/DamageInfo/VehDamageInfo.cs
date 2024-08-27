using System;
using Rage;

namespace DamageTrackerLib.DamageInfo;

/// <summary>
/// Holds all info for a Vehicle's damage.
/// </summary>

[Serializable]
public struct VehDamageInfo
{
    public uint VehHandle;
    public uint AttackerPedHandle;
    public int Damage;
    public WeaponDamageInfo WeaponInfo;
    public Vector3 LastCollisionPosition;
    // TODO: Vector3 relativeVelocityChange;
    // TODO: Vector3/Quaternion angularVelocityChange;
}