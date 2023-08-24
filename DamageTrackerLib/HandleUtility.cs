using System;
using Rage;
using Rage.Native;

namespace DamageTrackerLib;

public static class HandleUtility
{
    public static Ped TryGetPedByHandle(PoolHandle handle)
    {
        if (!NativeFunction.Natives.DOES_ENTITY_EXIST<bool>((uint)handle))
        {
            Game.LogTrivial($"DamageTrackerService Warning: Ped Handle {handle.ToString()} does not exist.");
            return null;
        }
        try
        {
            return NativeFunction.Natives.IS_ENTITY_A_PED<bool>((uint)handle) ? World.GetEntityByHandle<Ped>(handle) : null;
        }
        catch (ArgumentException)
        {
            Game.LogTrivial($"Exception Caught: Ped Handle ({handle.ToString()}) did not return an Entity.");
        }
        return null;
    }
}