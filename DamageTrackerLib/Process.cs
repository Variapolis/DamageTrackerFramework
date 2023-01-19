using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.Serialization.Formatters.Binary;
using DamageTrackerLib.DamageInfo;
using Rage;
using WeaponHash = DamageTrackerLib.DamageInfo.WeaponHash;

namespace DamageTrackerLib
{
    // ReSharper disable once UnusedType.Global
    public static class Process
    {
        public const string Guid = "609a228f-ac5d-4308-849b-34ebafcc9778";
        private static readonly BinaryFormatter binaryFormatter = new();

        public static void Run()
        {
            using var mmf = MemoryMappedFile.OpenExisting(Guid);
            using var mmfAccessor = mmf.CreateViewAccessor();
            using var stream = new MemoryStream();
            var buffer = new byte[mmfAccessor.Capacity];
            mmfAccessor.ReadArray(0, buffer, 0, buffer.Length);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;
            var damagedPeds = (PedDamageInfo[])binaryFormatter.Deserialize(stream);
            foreach (var pedDamageInfo in damagedPeds)
            {
                var ped = World.GetEntityByHandle<Ped>(pedDamageInfo.PedHandle);
            }
        }
    }
}