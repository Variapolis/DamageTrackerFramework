using System;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using Rage;
using Rage.Attributes;

[assembly: Plugin("DamageTrackerLib", Description = "A plugin for testing.",
    Author = "Variapolis",
    PrefersSingleInstance = true)]

namespace DamageTrackerLib
{
    // ReSharper disable once UnusedType.Global
    public static class Process
    {
    }

    public static class PipeClient
    {
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        internal static void Run()
        {
            var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.In);


            Game.LogTrivial("Connecting to server.");
            pipeClient.Connect();
            Game.LogTrivial("Connected to server.");


            // Validate the server's signature string.
            try
            {
                Game.LogTrivial("Attempting to deserialize.");
                var shit = (int)BinaryFormatter.Deserialize(pipeClient); // HACK: NEEDS TO BE CAST TO A PED DAMAGE INFO
                Game.LogTrivial($"Success! {shit}");
            }
            // Catch the IOException that is raised if the pipe is broken or disconnected.
            catch (Exception e)
            {
                Game.LogTrivial($"ERROR: {e.Message}");
            }
            finally
            {
                pipeClient.Close();
                Game.LogTrivial("Client Closed.");
            }
        }
    }
}