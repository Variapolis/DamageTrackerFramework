using System.Net;
using System.Reflection;
using DamageTrackerLib;
using Rage;

namespace DamageTrackingFramework;

internal static class VersionChecker
{
    internal static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    internal static string LibraryVersion =>
        Assembly.GetAssembly(typeof(DamageTrackerService)).GetName().Version.ToString(3);

    internal static void CheckForUpdates()
    {
        var webClient = new WebClient();
        var frameworkUpToDate = false;
        var libraryVersionMatch = false;
        try
        {
            var receivedVersion = webClient
                .DownloadString(
                    "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=42767&textOnly=1")
                .Trim();
            
            Game.LogTrivial(
                $"DamageTrackerFramework loaded successfully. Online Version: {receivedVersion} | Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            frameworkUpToDate = receivedVersion == CurrentVersion;
            libraryVersionMatch = LibraryVersion == CurrentVersion;
        }
        catch (WebException)
        {
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Failed.",
                "Please ensure you are ~o~online~w~.");
        }
        finally
        {
            Game.DisplayNotification("commonmenu", "card_suit_hearts", $"DamageTrackerFramework {CurrentVersion}",
                "~g~Successfully Loaded",
                $"By Variapolis \nVersion is {(frameworkUpToDate ? "~g~Up To Date" : "~r~Out Of Date")}");
            if (!frameworkUpToDate)
                Game.LogTrivial(
                    "[VERSION OUTDATED] Please update to latest version here: https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/");
            if (!libraryVersionMatch)
            {
                Game.LogTrivial(
                    "[VERSION MISMATCH] DamageTrackerLib version does not match DamageTrackerFramework. Ensure both DLLs are up to date.");
                Game.DisplayNotification(
                    "~r~WARNING: ~w~Version Mismatch for DamageTrackerLib.dll! ~o~Ensure both DLL files are up to date.");
            }
        }
    }
}