using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using DamageTrackerLib.DamageInfo;
using Rage;

namespace DamageTrackerLib
{
    // ReSharper disable once UnusedType.Global
    public static class DamageTrackerService
    {
        public const string Guid = "609a228f";

        public delegate void PedTookDamageDelegate(Ped ped, PedDamageInfo damageInfo);

        public static event PedTookDamageDelegate OnPedTookDamage;
        public static event PedTookDamageDelegate OnPlayerTookDamage;

        private static readonly BinaryFormatter binaryFormatter = new();
        private static GameFiber _gameFiber;

        public static void Start()
        {
            if (_gameFiber != null)
            {
                Game.LogTrivial("Tried to start DamageTrackerService while already running!");
                return;
            }

            _gameFiber = GameFiber.StartNew(Run);
        }

        public static void Stop() => _gameFiber.Abort();

        private static void Run()
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
                var damagedPeds = (PedDamageInfo[])binaryFormatter.Deserialize(stream);
                foreach (var pedDamageInfo in damagedPeds)
                {
                    var ped = World.GetEntityByHandle<Ped>(pedDamageInfo.PedHandle);
                    switch (ped.IsPlayer)
                    {
                        case true when OnPlayerTookDamage != null:
                            OnPlayerTookDamage.Invoke(ped, pedDamageInfo);
                            break;
                        case false when OnPedTookDamage != null:
                            OnPedTookDamage.Invoke(ped, pedDamageInfo);
                            break;
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
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