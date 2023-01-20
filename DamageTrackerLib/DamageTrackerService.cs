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
            using var mmf = MemoryMappedFile.OpenExisting(Guid);
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            while (true)
            {
                stream.SetLength(0);
                var buffer = new byte[mmfAccessor.Capacity];
                mmfAccessor.ReadArray(0, buffer, 0, buffer.Length);
                stream.Write(buffer, 0, buffer.Length);
                stream.Position = 0;
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
                GameFiber.Yield();
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}