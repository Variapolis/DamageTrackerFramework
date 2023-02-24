using System.Net;
using System.Reflection;
using Rage;

namespace DamageTrackingFramework;

internal static class VersionChecker
{
    internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

    internal static void CheckForUpdates()
    {
        var webClient = new WebClient();
        var upToDate = false;
        try
        {
            var receivedVersion = webClient
                .DownloadString(
                    "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=42767&textOnly=1")
                .Trim();
            Game.LogTrivial(
                $"DamageTrackerFramework loaded successfully. Online Version: {receivedVersion} | Local Version: {CurrentVersion}");
            upToDate = receivedVersion == CurrentVersion;
        }
        catch (WebException)
        {
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Failed.",
                "Please ensure you are ~o~online~w~.");
        }
        finally
        {
            if (!upToDate)
                Game.LogTrivial(
                    "[VERSION OUTDATED] Please update to latest version here: https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/");
            Game.DisplayNotification("commonmenu", "card_suit_hearts", $"DamageTrackerFramework {CurrentVersion}",
                "~g~Successfully Loaded",
                $"By Variapolis \nVersion is {(upToDate ? "~g~Up To Date" : "~r~Out Of Date")}");
        }
    }
}