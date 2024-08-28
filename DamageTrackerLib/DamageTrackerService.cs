using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using DamageTrackerLib.DamageInfo;
using Rage;
using Rage.Native;

namespace DamageTrackerLib
{
    // ReSharper disable once UnusedType.Global
    public static class DamageTrackerService
    {
        // ReSharper disable once UnusedMember.Global
        public static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

        public const string Guid = "609a228f";

        /// <summary>
        /// Delegate to be used for TookDamage events.
        /// </summary>
        public delegate void PedTookDamageDelegate(Ped victimPed, Ped attackerPed, PedDamageInfo damageInfo);

        /// <summary>
        /// Event invoked when a Ped takes damage. This event excludes the Player.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public static event PedTookDamageDelegate OnPedTookDamage;

        /// <summary>
        /// Event invoked when the Player takes damage ONLY.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public static event PedTookDamageDelegate OnPlayerTookDamage;

        
        /// <summary>
        /// Delegate to be used for TookDamage events.
        /// </summary>
        public delegate void VehTookDamageDelegate(Vehicle vehicle, Ped attackerPed, VehDamageInfo damageInfo);

        /// <summary>
        /// Event invoked when a Vehicle takes damage.
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        public static event VehTookDamageDelegate OnVehicleTookDamage;
        
        
        private static readonly BinaryFormatter BinaryFormatter = new();
        private static GameFiber _gameFiber;

        // ReSharper disable once UnusedMember.Global
        public static bool IsRunning => _gameFiber != null;

        /// <summary>
        /// Starts a GameFiber that collects incoming damage data from the DamageTracker plugin and turns them into events.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static void Start() => Start(false);

        /// <summary>
        /// Starts a GameFiber that collects incoming damage data from the DamageTracker plugin and turns them into events. Takes a boolean parameter that will enable logging damage.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void Start(bool enableLogging)
        {
            if (_gameFiber != null)
            {
                Game.LogTrivial("Tried to start DamageTrackerService while already running!");
                return;
            }
            NativeFunction.Natives
                .x5BA652A0CD14DF2F(); // HACK: Fixes stuttering issue by warming up JIT with a useless native.
            Game.LogTrivial("DamageTrackerService Started");
            _gameFiber = GameFiber.StartNew(() => Run(enableLogging));
        }


        /// <summary>
        /// Stops DamageTrackerService GameFiber.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static void Stop()
        {
            if (_gameFiber == null)
            {
                Game.LogTrivial("Tried to stop DamageTrackerService while it was not running");
                return;
            }

            Game.LogTrivial("DamageTrackerService Stopped");
            _gameFiber.Abort();
        }

        private static void Run(bool enableLogging)
        {
            using var mmf = MemoryMappedFile.CreateOrOpen(Guid, 20000, MemoryMappedFileAccess.ReadWrite);
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            while (true)
            {
                GameFiber.Yield();
                stream.SetLength(0);
                var buffer = new byte[mmfAccessor.Capacity];
                mmfAccessor.ReadArray(0, buffer, 0, buffer.Length);
                if (IsByteArrayZero(buffer)) continue; // TODO: Add error message
                stream.Write(buffer, 0, buffer.Length);
                stream.Position = 0;
                if (stream.Length <= 0) continue; // TODO: Add error message
                var damageData = (DamageInfoData)BinaryFormatter.Deserialize(stream);
                foreach (var pedDamageInfo in damageData.PedDamageInfoList) InvokePedDamageEvent(pedDamageInfo, enableLogging);
                foreach (var vehDamageInfo in damageData.VehDamageInfoList) InvokeVehicleDamageEvent(vehDamageInfo, enableLogging);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void InvokePedDamageEvent(PedDamageInfo pedDamageInfo, bool enableLogging)
        {
            var ped = TryGetPedByHandle(pedDamageInfo.PedHandle);
            if (!ped) return;
            if (enableLogging)
                Game.LogTrivial(
                    $"DamageTrackerService: Ped {ped.Model.Name} damaged by {pedDamageInfo.WeaponInfo.Hash} ({pedDamageInfo.AttackerPedHandle}).");
            var attackerPed = pedDamageInfo.AttackerPedHandle == default
                ? null
                : TryGetPedByHandle(pedDamageInfo.AttackerPedHandle);
            switch (ped.IsPlayer)
            {
                case true when OnPlayerTookDamage != null:
                    OnPlayerTookDamage.Invoke(ped, attackerPed, pedDamageInfo);
                    break;
                case false when OnPedTookDamage != null:
                    OnPedTookDamage.Invoke(ped, attackerPed, pedDamageInfo);
                    break;
            }
        }
        
        private static void InvokeVehicleDamageEvent(VehDamageInfo vehDamageInfo, bool enableLogging)
        {
            var veh = TryGetVehByHandle(vehDamageInfo.VehHandle);
            if (!veh) return;
            if (enableLogging)
                Game.LogTrivial(
                    $"DamageTrackerService: Vehicle {veh.Model.Name} damaged by {vehDamageInfo.WeaponInfo.Hash} ({vehDamageInfo.AttackerPedHandle}).");
            
            var attackerPed = vehDamageInfo.AttackerPedHandle == default
                ? null
                : TryGetPedByHandle(vehDamageInfo.AttackerPedHandle);
            OnVehicleTookDamage?.Invoke(veh, attackerPed, vehDamageInfo);
        }

        
        // TODO: Make this a template instead.
        private static Ped TryGetPedByHandle(PoolHandle handle)
        {
            if (!NativeFunction.Natives.DOES_ENTITY_EXIST<bool>((uint)handle))
            {
                Game.LogTrivial($"DamageTrackerService Warning: Ped Handle {handle.ToString()} does not exist.");
                return null;
            }
            try
            {
                return World.GetEntityByHandle<Ped>(handle);
            }
            catch (ArgumentException)
            {
                Game.LogTrivial($"DamageTrackerService Exception Caught: Ped Handle ({handle.ToString()}) did not return an Entity.");
            }
            return null;
        }
        
        private static Vehicle TryGetVehByHandle(PoolHandle handle)
        {
            if (!NativeFunction.Natives.DOES_ENTITY_EXIST<bool>((uint)handle))
            {
                Game.LogTrivial($"DamageTrackerService Warning: Vehicle Handle {handle.ToString()} does not exist.");
                return null;
            }
            try
            {
                return World.GetEntityByHandle<Vehicle>(handle);
            }
            catch (ArgumentException)
            {
                Game.LogTrivial($"DamageTrackerService Exception Caught: Vehicle Handle ({handle.ToString()}) did not return an Entity.");
            }
            return null;
        }

        private static bool IsByteArrayZero(byte[] array)
        {
            foreach (var element in array)
                if (element != 0)
                    return false;
            return true;
        }
    }
}