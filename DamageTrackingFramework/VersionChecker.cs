namespace DamageTrackingFramework;

using System;
using System.Reflection;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DamageTrackerLib;
using Rage;
using Task = System.Threading.Tasks.Task;
using TaskStatus = System.Threading.Tasks.TaskStatus;

internal static class VersionChecker
{
    internal static readonly string CurrentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    private static readonly string LibraryVersion = Assembly.GetAssembly(typeof(DamageTrackerService)).GetName().Version.ToString(3);

    private static CancellationTokenSource _cancellationTokenSource;
    private static Task _processTask;
    private static string _receivedVersion;
    private static bool _frameworkUpToDate, _webSuccess;
    private static Exception _fault;

    public static void CheckForUpdates()
    {
        ResetState();
        
        _cancellationTokenSource = new CancellationTokenSource();
        AppDomain.CurrentDomain.DomainUnload += (_, _) => _cancellationTokenSource?.Cancel();
        
        GameFiber.StartNew(WaitProc);
        
        _processTask = Task.Run(CheckForUpdatesAsync, _cancellationTokenSource.Token);
    }

    private static void ResetState()
    {
        _processTask = null;
        _receivedVersion = default;
        _frameworkUpToDate = _webSuccess = false;
        _fault = null;
    }

    private static async Task CheckForUpdatesAsync()
    {
        using HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        try
        {
            // Perform the asynchronous HTTP request
            var response = await httpClient.GetStringAsync("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=42767&textOnly=1");

            _receivedVersion = response.Trim();
            _frameworkUpToDate = _receivedVersion == CurrentVersion;
            _webSuccess = true;
        }
        catch (HttpRequestException ex)
        {
            _fault = ex;  // Handle network errors
        }
        catch (TaskCanceledException ex)
        {
            _fault = ex;  // Handle request timeouts
        }
    }

    private static void WaitProc()
    {
        // Wait for the async task to complete
        GameFiber.Sleep(1000);
        GameFiber.WaitUntil(() => _processTask.Status != TaskStatus.Running);

        // Handle the result or fault
        if (_fault != null)
        {
            ProcessError();
        }
        else
        {
            ProcessSuccess();
        }

        // Log the final state of the version check
        Game.LogTrivial($"DamageTrackerFramework loaded. Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
        Game.DisplayNotification("commonmenu", "card_suit_hearts", $"DamageTrackerFramework {CurrentVersion}",
            "~g~Successfully Loaded",
            $"By Variapolis \nVersion is {(_webSuccess ? (_frameworkUpToDate ? "~g~Up To Date" : "~r~Out Of Date") : "~o~Version Check Failed")}");
    }

    private static void ProcessError()
    {
        if (_fault is HttpRequestException)
        {
            Game.LogTrivial($"[VERSION CHECK FAILED] Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("Version check failed, your internet may be disabled, or LCPDFR may be down.");
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Failed.", "Please ensure you are ~o~online~w~.");
        }
        else if (_fault is TaskCanceledException)
        {
            Game.LogTrivial($"[VERSION CHECK TIMED OUT] Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Timed Out.", "The server may be down, or your internet is slow.");
        }
    }

    private static void ProcessSuccess()
    {
        if (!_frameworkUpToDate)
        {
            Game.LogTrivial($"[VERSION OUTDATED] Online Version: {_receivedVersion} | Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("Please update to the latest version here: https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/");
        }

        if (LibraryVersion != CurrentVersion)
        {
            Game.LogTrivial($"[VERSION MISMATCH] Online Version: {_receivedVersion} | Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("DamageTrackerLib version does not match DamageTrackerFramework. Ensure both DLLs are up to date.");
            Game.DisplayNotification("~r~WARNING: ~w~Version Mismatch for DamageTrackerLib.dll! ~o~Ensure both DLL files are up to date.\n" +
                $"~w~Framework Version: ~o~{CurrentVersion}   ~w~Library Version: ~o~{LibraryVersion}");
        }
    }
}
