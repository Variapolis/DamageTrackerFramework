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
        private static readonly Dictionary<Ped, (int health, int armour)> PedDict = new();

        private static readonly List<PedDamageInfo> PedDamageList = new();

        private static readonly BinaryFormatter Formatter = new();

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
                CleanPedDictionary();
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void
            SendPedData(MemoryMappedViewAccessor accessor,
                MemoryStream stream) // TODO: Resize file if ped count is too small or send less.
        {
            // var requiredSize = Marshal.SizeOf(PedDamageList) + (PedDamageList.Count * Marshal.SizeOf<PedDamageInfo>()) + 2000;
            // if(requiredSize > accessor.Capacity)
            stream.SetLength(0);
            Formatter.Serialize(stream, PedDamageList.ToArray());
            var buffer = stream.ToArray();
            accessor.WriteArray(0, buffer, 0, buffer.Length);
            accessor.Flush();
        }

        private static void HandlePed(Ped ped)
        {
            if (!ped.Exists() || !ped.IsHuman) return;
            if (!PedDict.ContainsKey(ped)) PedDict.Add(ped, (ped.Health, ped.Armor));

            var previousHealth = PedDict[ped];
            if (!TryGetPedDamage(ped, out var damage)) return;
            PedDamageList.Add(GenerateDamageInfo(ped, previousHealth.health, previousHealth.armour, damage));
            ClearPedDamage(ped);
        }

        private static PedDamageInfo GenerateDamageInfo(Ped ped, int previousHealth, int previousArmour, WeaponDamageInfo damage)
        {
            var lastDamagedBone = (BoneId)ped.LastDamageBone;
            var boneTuple = DamageTrackerLookups.BoneLookup[lastDamagedBone];
            PoolHandle attackerPed = 0;
            if (ped.HasBeenDamagedByAnyPed)
                foreach (var otherPed in PedDict.Keys)
                {
                    if (!otherPed.IsValid() || !ped.HasBeenDamagedBy(otherPed)) continue;
                    attackerPed = otherPed.Handle;
                    break;
                }
            return new PedDamageInfo
            {
                PedHandle = ped.Handle,
                AttackerPedHandle = attackerPed,
                Damage = previousHealth - ped.Health,
                ArmourDamage = previousArmour - ped.Armor,
                WeaponInfo = damage,
                BoneInfo = new BoneDamageInfo
                {
                    BoneId = lastDamagedBone,
                    Limb = boneTuple.limb,
                    BodyRegion = boneTuple.bodyRegion
                }
            };
        }

        private static void ClearPedDamage(Ped ped)
        {
            ped.ClearLastDamageBone();
            NativeFunction.Natives.xAC678E40BE7C74D2(ped);
            PedDict[ped] = (ped.Health, ped.Armor);
        }

        private static bool TryGetPedDamage(Ped ped, out WeaponDamageInfo damage) // BUG: Fire doesn't damage per tick
        {
            var pedAddr = ped.MemoryAddress;
            damage = default;
            unsafe
            {
                var damageHandler = *(IntPtr*)(pedAddr + 648);
                if (damageHandler == IntPtr.Zero) return false;
                var damageArray = *(int*)(damageHandler + 72);
                if (damageArray == 0) return false;
                if (!WasDamaged(ped, PedDict[ped])) return false;
                var hashAddr = damageHandler + 8;
                if (hashAddr == IntPtr.Zero || *(WeaponHash*)hashAddr == 0 ||
                    !DamageTrackerLookups.WeaponLookup.ContainsKey(*(WeaponHash*)hashAddr))
                    return false; // May not be necessary. TODO: Default value for unknown hashes and 0
                var weaponHash = *(WeaponHash*)hashAddr;
                var damageTuple = DamageTrackerLookups.WeaponLookup[weaponHash];
                damage = new WeaponDamageInfo
                {
                    Hash = weaponHash,
                    Group = damageTuple.DamageGroup,
                    Type = damageTuple.DamageType
                };
                return true;
            }
        }

        private static bool WasDamaged(Ped ped, (int health, int armour) previousHealth)
        {
            var wasDamaged = ped.Health < previousHealth.health || ped.Armor < previousHealth.armour;
            if (ped.Health > previousHealth.health) PedDict[ped] = (ped.Health, PedDict[ped].armour);
            if (ped.Armor > previousHealth.armour) PedDict[ped] = (PedDict[ped].health, ped.Armor);
            return wasDamaged;
        }

        private static void CleanPedDictionary()
        {
            foreach (var ped in PedDict.Keys.ToList())
                if (!ped.Exists())
                    PedDict.Remove(ped);
        }
    }
}