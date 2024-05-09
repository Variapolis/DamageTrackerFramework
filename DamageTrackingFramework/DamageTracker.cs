using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DamageTrackerLib;
using DamageTrackerLib.DamageInfo;
using Rage;
using Rage.Native;
using WeaponHash = DamageTrackerLib.DamageInfo.WeaponHash;

namespace DamageTrackingFramework
{
    internal static class DamageTracker
    {
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        private static readonly Dictionary<Ped, (int health, int armour)> PedHealthDict = new();
        private static readonly List<PedDamageInfo> PedDamageList = new();

        private static readonly BinaryFormatter Formatter = new();

        private static readonly HashSet<WeaponHash> UnknownWeaponHashCache = new();
        
        internal static void CheckPedsFiber()
        {
            using var mmf = MemoryMappedFile.CreateOrOpen(DamageTrackerService.Guid, 20000,
                MemoryMappedFileAccess.ReadWrite); // TODO: Replace with GUID from Lib
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            while (true)
            {
                PedDamageList.Clear();
                var peds = World.GetAllPeds();
                foreach (var ped in peds) HandlePed(ped);
                SendPedData(mmfAccessor, stream);
                CleanPedDictionaries();
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void
            SendPedData(MemoryMappedViewAccessor accessor,
                MemoryStream stream) // TODO: Resize file if ped count is too small or send less.
        {
            stream.SetLength(0);
            Formatter.Serialize(stream, PedDamageList.ToArray());
            var buffer = stream.ToArray();
            accessor.WriteArray(0, buffer, 0, buffer.Length);
            accessor.Flush();
        }

        private static void HandlePed(Ped ped)
        {
            if (!ped.Exists() || !ped.IsHuman) return;
            if (!PedHealthDict.ContainsKey(ped)) PedHealthDict.Add(ped, (ped.Health, ped.Armor));

            var previousHealth = PedHealthDict[ped];
            if (!TryGetPedDamage(ped, out var damage)) return;
            PedDamageList.Add(GenerateDamageInfo(ped, previousHealth.health, previousHealth.armour, damage));
            ClearPedDamage(ped);
        }

        private static PedDamageInfo GenerateDamageInfo(Ped ped, int previousHealth, int previousArmour,
            WeaponHash damageHash)
        {
            var lastDamagedBone = (BoneId)ped.LastDamageBone;
            var boneTuple = DamageTrackerLookups.BoneLookup[lastDamagedBone];
            var weaponTuple = DamageTrackerLookups.WeaponLookup[damageHash];
            var attackerPed = GetAttackerPed(ped);

            return new PedDamageInfo
            {
                PedHandle = ped.Handle,
                AttackerPedHandle = attackerPed,
                Damage = previousHealth - ped.Health,
                ArmourDamage = previousArmour - ped.Armor,
                WeaponInfo =
                {
                    Hash = damageHash,
                    Type = weaponTuple.DamageType,
                    Group = weaponTuple.DamageGroup
                },
                BoneInfo = new BoneDamageInfo
                {
                    BoneId = lastDamagedBone,
                    Limb = boneTuple.limb,
                    BodyRegion = boneTuple.bodyRegion
                }
            };
        }

        private static PoolHandle GetAttackerPed(Ped ped)
        {
            PoolHandle attackerPed = default;
            if (!ped.HasBeenDamagedByAnyPed) return attackerPed;
            foreach (var otherPed in PedHealthDict.Keys)
            {
                if (!otherPed.IsValid() || !ped.HasBeenDamagedBy(otherPed)) continue;
                attackerPed = otherPed.Handle;
                break;
            }

            return attackerPed;
        }

        private static bool TryGetPedDamage(Ped ped, out WeaponHash damageHash)
        {
            var pedAddr = ped.MemoryAddress;
            damageHash = default;
            unsafe
            {
                var damageHandler = *(IntPtr*)(pedAddr + 648);
                if (damageHandler == IntPtr.Zero) // Always true if the Ped has never taken damage.
                {
                    if (!WasDamaged(ped)) return false;
                    damageHash = WeaponHash.Fall; // Triggers damage event if health went down due to falling.
                    return true;
                }

                var damageArray = *(int*)(damageHandler + 72);
                if (damageArray == 0) // true unless the ped took damage since LastDamage was cleared (Except Falling).
                {
                    if (!WasDamaged(ped)) return false;
                    damageHash = WeaponHash.Fall; // Triggers damage event if health went down due to falling.
                    return true;
                }

                var hashAddr = damageHandler + 8;
                damageHash = DamageTrackerLookups.WeaponLookup.ContainsKey(*(WeaponHash*)hashAddr)
                    ? *(WeaponHash*)hashAddr
                    : WeaponHash.Unknown;
                if (damageHash == WeaponHash.Unknown && !UnknownWeaponHashCache.Contains(*(WeaponHash*)hashAddr)){
                    Game.LogTrivial(
                        $"WARNING: {*(uint*)hashAddr} Hash is unknown. Please notify DamageTracker Developer at: https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/");
                    UnknownWeaponHashCache.Add(*(WeaponHash*)hashAddr);
                }
                return true;
            }
        }

        private static bool WasDamaged(Ped ped)
        {
            var previousHealth = PedHealthDict[ped];
            var wasDamaged = ped.Health < previousHealth.health || ped.Armor < previousHealth.armour;
            return wasDamaged;
        }

        private static void ClearPedDamage(Ped ped)
        {
            ped.ClearLastDamageBone();
            NativeFunction.Natives.xAC678E40BE7C74D2(ped);
            PedHealthDict[ped] = (ped.Health, ped.Armor);
        }

        private static void CleanPedDictionaries()
        {
            foreach (var ped in PedHealthDict.Keys.ToList())
                if (!ped.Exists())
                    PedHealthDict.Remove(ped);
        }
    }
}