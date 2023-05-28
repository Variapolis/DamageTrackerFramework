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
                var damagedPeds = (PedDamageInfo[])BinaryFormatter.Deserialize(stream);
                foreach (var pedDamageInfo in damagedPeds) InvokeDamageEvent(pedDamageInfo, enableLogging);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void InvokeDamageEvent(PedDamageInfo pedDamageInfo, bool enableLogging)
        {
            var ped = TryGetPedByHandle(pedDamageInfo.PedHandle);
            if (!ped) return;
            if (enableLogging)
                Game.LogTrivial(
                    $"DamageTrackerService: Ped {ped.Model.Name} damaged by {pedDamageInfo.WeaponInfo.Hash}.");
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

        private static bool IsByteArrayZero(byte[] array)
        {
            foreach (var element in array)
                if (element != 0)
                    return false;
            return true;
        }
    }
}