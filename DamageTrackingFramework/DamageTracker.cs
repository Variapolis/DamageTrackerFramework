using System;
using System.Collections.Generic;
using System.Linq;
using DamageTrackingFramework.DamageInfo;
using Rage;
using Rage.Native;
using WeaponHash = DamageTrackingFramework.DamageInfo.WeaponHash;

namespace DamageTrackingFramework
{
    public static class DamageTracker
    {
        public delegate void PedDamagedDelegate(Ped ped, PedDamageInfo pedDamageInfo);

        public static event PedDamagedDelegate OnPedTookDamage;
        public static event PedDamagedDelegate OnPlayerTookDamage;

        // ReSharper disable once HeapView.ObjectAllocation.Evident
        private static readonly Dictionary<Ped, int> PedDict = new Dictionary<Ped, int>();

        internal static void CheckPedsFiber()
        {
            // var beforeCount = DamageSubCount;
            // OnPedTookDamage += Test;
            // Game.LogTrivial($"{beforeCount} to {DamageSubCount}");
            PipeServer.Start();
            while (true)
            {
                var peds = World.GetAllPeds();
                foreach (var ped in peds) HandlePed(ped); // TODO: Add peds to pipe queue here
                // TODO: Flush ped queue here
                CleanPedDictionary();
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void HandlePed(Ped ped)
        {
            if (!ped.Exists()) return;
            if (!PedDict.ContainsKey(ped)) PedDict.Add(ped, ped.Health);

            var previousHealth = PedDict[ped];
            if (!TryGetPedDamage(ped, out var damage)) return;
            InvokeDamageEvent(ped, GenerateDamageInfo(ped, previousHealth, damage));
            ClearPedDamage(ped);
        }

        private static void InvokeDamageEvent(Ped ped, PedDamageInfo damageInfo)
        {
            switch (ped.IsPlayer)
            {
                case true when OnPlayerTookDamage != null:
                    OnPlayerTookDamage(ped, damageInfo);
                    break;
                case false when OnPedTookDamage != null:
                    OnPedTookDamage(ped, damageInfo);
                    break;
            }
        }

        private static PedDamageInfo GenerateDamageInfo(Ped ped, int previousHealth, WeaponDamageInfo damage)
        {
            var lastDamagedBone = (BoneId)ped.LastDamageBone;
            var boneTuple = Lookups.BoneLookup[lastDamagedBone];
            return new PedDamageInfo()
            {
                Damage = previousHealth - ped.Health,
                WeaponInfo = damage,
                BoneInfo = new BoneDamageInfo()
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
        }

        private static bool TryGetPedDamage(Ped ped, out WeaponDamageInfo damage)
        {
            var pedAddr = ped.MemoryAddress;
            damage = default;
            unsafe
            {
                var damageHandler = *(IntPtr*)(pedAddr + 648);
                if (damageHandler == IntPtr.Zero) return false;
                var damageArray = *(int*)(damageHandler + 72);
                if (ped.Health >= PedDict[ped] || damageArray <= 0)
                {
                    PedDict[ped] = ped.Health;
                    return false;
                }

                var hashAddr = damageHandler + 8;
                if (hashAddr == IntPtr.Zero || *(WeaponHash*)hashAddr == 0 ||
                    !Lookups.WeaponLookup.ContainsKey(*(WeaponHash*)hashAddr)) return false; // May not be necessary.
                var weaponHash = *(WeaponHash*)hashAddr;
                var damageTuple = Lookups.WeaponLookup[weaponHash];
                damage = new WeaponDamageInfo()
                {
                    Hash = weaponHash,
                    Group = damageTuple.DamageGroup,
                    Type = damageTuple.DamageType
                };
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