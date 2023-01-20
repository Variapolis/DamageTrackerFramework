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
        public const string Guid = "609a228f-ac5d-4308-849b-34ebafcc9778";
        public delegate void PedTookDamageDelegate(Ped ped, PedDamageInfo damageInfo);
        public static event PedTookDamageDelegate OnPedTookDamage;
        public static event PedTookDamageDelegate OnPlayerTookDamage;
        
        private static readonly BinaryFormatter binaryFormatter = new();
        private static readonly GameFiber _gameFiber = new(Run);
        public static void Start() => _gameFiber.Start();

        public static void Stop() => _gameFiber.Abort();

        private static void Run()
        {
            using var mmf = MemoryMappedFile.OpenExisting(Guid);
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            while (true)
            {
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
                            OnPlayerTookDamage(ped, pedDamageInfo);
                            break;
                        case false when OnPedTookDamage != null:
                            OnPedTookDamage(ped, pedDamageInfo);
                            break;
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}