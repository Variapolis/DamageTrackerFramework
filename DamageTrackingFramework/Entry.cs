using Rage;
using Rage.Attributes;
using Rage.Native;

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
            NativeFunction.Natives
                .x5BA652A0CD14DF2F(); // HACK: Fixes stuttering issue by warming up JIT with a useless native.
            _gameFiber = GameFiber.StartNew(DamageTracker.CheckPedsFiber);
            Game.LogTrivial("GameFiber started!");
            VersionChecker.CheckForUpdates();
            GameFiber.Hibernate();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedParameter.Global
        public static void OnUnload(bool Exit)
        {
            Game.DisplayNotification("commonmenu", "card_suit_hearts",
                $"DamageTrackerFramework {VersionChecker.CurrentVersion}",
                "~o~Unloaded", $"By Variapolis");
            _gameFiber.Abort();
            DamageTracker.Dispose();
        }
    }
}