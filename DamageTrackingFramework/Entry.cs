using Rage;
using Rage.Attributes;

[assembly: Plugin("DamageTrackingFramework", Description = "A damage tracking utility for GTA V mods.",
    Author = "Variapolis", PrefersSingleInstance = true)]

namespace DamageTrackingFramework
{
    // ReSharper disable once UnusedType.Global
    public class EntryPoint
    {
        private static GameFiber _gameFiber;

        // ReSharper disable once UnusedMember.Global
        public static void Main()
        {
            _gameFiber = GameFiber.StartNew(DamageTracker.CheckPedsFiber);
            Game.LogTrivial("GameFiber started!");
            Game.DisplayNotification("DamageTrackerFramework by Variapolis ~g~Successfully Loaded");
            GameFiber.Hibernate();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static void OnUnload(bool Exit)
        {
            Game.DisplayNotification("DamageTrackingFramework by Variapolis ~r~ Unloaded");
            _gameFiber.Abort();
        }
    }
}