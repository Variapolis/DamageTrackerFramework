using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using DamageTrackingFramework.DamageInfo;
using Rage;

namespace DamageTrackingFramework
{
    public static class PipeServer
    {
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
        private static Thread _thread;

        public static void Start()
        {
            _thread = new Thread(Run);
            _thread.Start();
        }

        public static void Stop()
        {
            _thread.Abort();
            _thread = null;
        }

        private static void Run()
        {
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut);


            // Wait for a client to connect
            Game.LogTrivial("Waiting for client to connect.");

            pipeServer.WaitForConnection(); // TODO: Convert to GameFiber by checking Connected instead.
            Game.LogTrivial("Client connected.");
            try
            {
                Queue<int> q = new Queue<int>();
                for (int i = 0; i < q.Count; i++) BinaryFormatter.Serialize(pipeServer, q.Dequeue());
                pipeServer.Flush();
                Game.LogTrivial("Packet sent!");
            }
            // Catch the IOException that is raised if the pipe is broken or disconnected.
            catch (Exception e)
            {
                Game.LogTrivial($"ERROR: {e.Message}");
            }
            finally
            {
                pipeServer.Close();
                Game.LogTrivial("Server Closed.");
            }
        }

        // public PedDamageInfo ReadString()
        // {
        //     var shit = (PedDamageInfo)BinaryFormatter.Deserialize(ioStream);
        //     return shit;
        // }
    }
}