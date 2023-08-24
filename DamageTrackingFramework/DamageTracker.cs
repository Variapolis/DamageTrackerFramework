using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using DamageTrackerLib;
using DamageTrackerLib.DamageInfo;
using Rage;
using Rage.Attributes;
using Rage.Native;
using WeaponHash = DamageTrackerLib.DamageInfo.WeaponHash;

namespace DamageTrackingFramework
{
    internal struct HookDamageData
    {
        internal readonly PoolHandle VictimHandle;
        internal readonly PoolHandle? AttackerHandle;
        internal readonly uint WeaponHash;

        public HookDamageData(PoolHandle victimHandle, PoolHandle? attackerHandle, uint weaponHash)
        {
            VictimHandle = victimHandle;
            AttackerHandle = attackerHandle;
            WeaponHash = weaponHash;
        }
    }

    internal static class DamageTracker
    {
        // ReSharper disable once HeapView.ObjectAllocation.Evident
        private static readonly Dictionary<Ped, (int health, int armour)> PedHealthDict = new();
        private static readonly List<PedDamageInfo> PedDamageList = new();
        private static readonly List<HookDamageData> HookData = new();
        public static EasyHook.LocalHook hook;
        private static readonly BinaryFormatter Formatter = new();

        public delegate void EntityLogDamageDelegate(IntPtr victim, IntPtr culprit, uint weapon, uint time, bool a5);

        public delegate uint GetScriptGuidForEntityDelegate(IntPtr fwEntity);

        public static GetScriptGuidForEntityDelegate GetScriptGuidForEntity;
        public static EntityLogDamageDelegate origEntityLogDamage;

        internal static void CheckPedsFiber()
        {
            IntPtr addr = Game.FindPattern("48 F7 F9 49 8B 48 08 48 63 D0 C1 E0 08 0F B6 1C 11 03 D8");
            if (addr == IntPtr.Zero) return;
            addr -= 0x68;
            GetScriptGuidForEntity = Marshal.GetDelegateForFunctionPointer<GetScriptGuidForEntityDelegate>(addr);
            Hook();

            using var mmf = MemoryMappedFile.CreateOrOpen(DamageTrackerService.Guid, 20000,
                MemoryMappedFileAccess.ReadWrite); // TODO: Replace with GUID from Lib
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            while (true)
            {
                PedDamageList.Clear();
                CreateDamageInfo();
                CachePedHealth();
                SendPedData(mmfAccessor, stream);
                CleanPedDictionaries();
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static void Hook()
        {
            IntPtr addr = Game.FindPattern("21 4D D8 21 4D DC 41 8B D8");

            if (addr == IntPtr.Zero) throw new EntryPointNotFoundException("Pattern could not be found");

            addr -= 0x1F;
            origEntityLogDamage = Marshal.GetDelegateForFunctionPointer<EntityLogDamageDelegate>(addr);
            EntityLogDamageDelegate detour = EntityLogDamageDetour;
            hook = EasyHook.LocalHook.Create(addr, detour, null);
            hook.ThreadACL.SetExclusiveACL(new[] { 0 });
            Game.DisplaySubtitle("Hooked");
        }

        private static void CreateDamageInfo()
        {
            foreach (var data in HookData)
            {
                var victim = HandleUtility.TryGetPedByHandle(data.VictimHandle);
                // if (victim == null) continue;
                // Game.DisplaySubtitle(victim?.Model.Name ?? "Nothing");
                // PedDamageList.Add(GenerateDamageInfo(data));
            }
        }

        private static PedDamageInfo GenerateDamageInfo(Ped ped, int previousHealth, int previousArmour,
            WeaponHash damageHash)
        {
            var lastDamagedBone = (BoneId)ped.LastDamageBone;
            var boneTuple = DamageTrackerLookups.BoneLookup[lastDamagedBone];
            var weaponTuple = DamageTrackerLookups.WeaponLookup[damageHash];

            return new PedDamageInfo
            {
                PedHandle = ped.Handle,
                AttackerPedHandle = default,
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

        private static void CachePedHealth()
        {
            foreach (var ped in World.EnumeratePeds()) PedHealthDict[ped] = (ped.Health, ped.Armor);
        }

        private static void EntityLogDamageDetour(IntPtr victim, IntPtr culprit, uint weapon, uint time, bool a5)
        {
            if (victim != IntPtr.Zero) // Check if victim is null
            {
                var victimHandle = new PoolHandle(GetScriptGuidForEntity(victim));
                PoolHandle? attackerHandle =
                    culprit == IntPtr.Zero ? null : new PoolHandle(GetScriptGuidForEntity(culprit));
                HookData.Add(new HookDamageData(victimHandle, attackerHandle, weapon));
                var ped = HandleUtility.TryGetPedByHandle(victimHandle);
                if (ped != null) Game.LogTrivial(ped.Health.ToString());
            }

            origEntityLogDamage(victim, culprit, weapon, time, a5);
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

        private static void CleanPedDictionaries()
        {
            foreach (var ped in PedHealthDict.Keys.ToList())
                if (!ped.Exists())
                    PedHealthDict.Remove(ped);
            HookData.Clear();
        }

        public static void Dispose() => hook.Dispose();

        [ConsoleCommand]
        public static void DeformAdvancedFromVehicle(Vector3 relativeVehicleOffset)
        {
            var vehicle = Game.LocalPlayer.Character.CurrentVehicle;
            var offset =  vehicle.GetOffsetPosition(relativeVehicleOffset);
            NativeFunction.Natives.SET_VEHICLE_DAMAGE(vehicle, offset.X, offset.Y, offset.Z, 100f, 100f, false);
        }
    }
}