using System;
using System.Collections.Generic;
using System.Linq;
using Rage;
using Rage.Native;

namespace DamageTrackingFramework
{
    internal static class DamageTracker
    {
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        private static readonly Dictionary<Ped, int> PedDict = new Dictionary<Ped, int>();

        internal static void CheckPedsFiber()
        {
            while (true)
            {
                var peds = World.GetAllPeds();
                foreach (var ped in peds) HandlePed(ped);
                CleanPedDictionary();
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void HandlePed(Ped ped)
        {
            if (!ped.Exists()) return;
            if (!PedDict.ContainsKey(ped)) PedDict.Add(ped, ped.Health);
            TryGetPedDamage(ped, out var damage);
        }

        private static bool TryGetPedDamage(Ped ped, out WeaponHash damage)
        {
            var pedAddr = ped.MemoryAddress;
            damage = 0;
            unsafe
            {
                // PedDict[ped] = ped.Health;
                var damageHandler = *(IntPtr*)(pedAddr + 648);
                if (damageHandler == IntPtr.Zero) return false;
                var damageArray = *(int*)(damageHandler + 72);
                if (ped.Health >= PedDict[ped] || damageArray <= 0)
                {
                    PedDict[ped] = ped.Health;
                    return false;
                }
                var hashAddr = damageHandler + 8;
                if (hashAddr == IntPtr.Zero || *(uint*)hashAddr == 0)
                    return false;

                damage = *(WeaponHash*)hashAddr;
                var lastDamagedBone = (BoneId)ped.LastDamageBone;
                try
                {
                    Game.DisplayHelp(
                        $"Ped ~g~{ped.Model.Name} {ped.MemoryAddress.ToString("X8")} ~y~{(ped.IsDead ? "Dead" : "Alive")}\n" +
                        $"~r~{Lookups.WeaponLookup[damage].Name} {Lookups.WeaponLookup[damage].DamageType.ToString()} {Lookups.WeaponLookup[damage].WeaponGroup.ToString()}\n" +
                        $"{lastDamagedBone.ToString()} {Lookups.BoneLookup[lastDamagedBone].limb.ToString()} {Lookups.BoneLookup[lastDamagedBone].bodyRegion.ToString()}");
                }
                catch (KeyNotFoundException e)
                {
                    Game.LogTrivial(e.Data.Keys.GetType().ToString());
                    Game.LogTrivial($"Damage = {damage} + Bone = {lastDamagedBone}");
                    Game.DisplaySubtitle("~r~Error");
                }
                finally
                {
                    Game.LogTrivial($"Damage = {damage}");
                }

                ped.ClearLastDamageBone();
                NativeFunction.Natives.xAC678E40BE7C74D2(ped);
                return true;
            }
        }

        private static void CleanPedDictionary()
        {
            foreach (var ped in PedDict.Keys.ToList())
                if (!ped.Exists())
                    PedDict.Remove(ped);
        }
    }
}