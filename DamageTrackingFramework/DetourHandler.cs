using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using DamageTrackerLib;
using Rage;

namespace DamageTrackingFramework;

internal class DetourHandler
{
    internal static EasyHook.LocalHook DamageHook;
    internal readonly ConcurrentDictionary<PoolHandle, HookDamageData> PedHookData = new();

    internal delegate void EntityLogDamageDelegate(IntPtr victim, IntPtr culprit, uint weapon, uint time, bool a5);

    internal delegate uint GetScriptGuidForEntityDelegate(IntPtr fwEntity);

    internal static GetScriptGuidForEntityDelegate GetScriptGuidForEntity;
    internal static EntityLogDamageDelegate OrigEntityLogDamage;
        
    internal struct HookDamageData
    {
        internal readonly PoolHandle VictimHandle;
        internal readonly PoolHandle? AttackerHandle;
        internal readonly uint WeaponHash;
        internal static DetourHandler DetourHandler;
        
        
        public HookDamageData(PoolHandle victimHandle, PoolHandle? attackerHandle, uint weaponHash)
        {
            VictimHandle = victimHandle;
            AttackerHandle = attackerHandle;
            WeaponHash = weaponHash;
        }
    }
        
    internal void StartHook()
    {
        IntPtr addr = Game.FindPattern("48 F7 F9 49 8B 48 08 48 63 D0 C1 E0 08 0F B6 1C 11 03 D8");
        if (addr == IntPtr.Zero) return;
        addr -= 0x68;
        GetScriptGuidForEntity = Marshal.GetDelegateForFunctionPointer<GetScriptGuidForEntityDelegate>(addr);
        Hook();
    }
        
    private void Hook()
    {
        IntPtr addr = Game.FindPattern("21 4D D8 21 4D DC 41 8B D8");

        if (addr == IntPtr.Zero) throw new EntryPointNotFoundException("Pattern could not be found");

        addr -= 0x1F;
        OrigEntityLogDamage = Marshal.GetDelegateForFunctionPointer<EntityLogDamageDelegate>(addr);
        EntityLogDamageDelegate detour = EntityLogDamageDetour;
        DamageHook = EasyHook.LocalHook.Create(addr, detour, null);
        DamageHook.ThreadACL.SetExclusiveACL(new[] { 0 });
        Game.LogTrivial("Hooked Successfully");
    }
        
    private void EntityLogDamageDetour(IntPtr victim, IntPtr culprit, uint weapon, uint time, bool melee)
    {
        if (victim != IntPtr.Zero && weapon != 0) // Check if victim is null or weapon is 0
        {
            var victimHandle = new PoolHandle(GetScriptGuidForEntity(victim));
            PoolHandle? attackerHandle =
                culprit == IntPtr.Zero ? null : new PoolHandle(GetScriptGuidForEntity(culprit));
        
            if (HandleUtility.TryGetPedByHandle(victimHandle))
            {
                PedHookData.TryAdd(victimHandle, new HookDamageData(victimHandle, attackerHandle, weapon));   
            } 
        }

        OrigEntityLogDamage(victim, culprit, weapon, time, melee);
    }

    internal static void Dispose() => DamageHook.Dispose();
}