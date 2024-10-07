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
    
    private class VersionResult
    {
        public string ReceivedVersion = null;
        public bool FrameworkUpToDate = false, WebSuccess = false;
        public Exception Fault = null;

        public VersionResult()
        {
        }
    }
    
    public static void CheckForUpdates()
    {
        AppDomain.CurrentDomain.DomainUnload += (_, _) => _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        Task<VersionResult> processTask = Task.Run(CheckForUpdatesAsync, _cancellationTokenSource.Token);
        
        GameFiber.StartNew(() =>
        {
            // Wait for the async task to complete
            // ReSharper disable once AccessToDisposedClosure
            GameFiber.WaitUntil(() => processTask.Status > TaskStatus.WaitingForChildrenToComplete);
            if (processTask.IsCanceled) { return; }
            if (processTask.Exception != null) { Game.LogTrivial(processTask.Exception.Message); }
            HandleResult(processTask.Result);
            
            processTask.Dispose();
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        });
    }

    private static async Task<VersionResult> CheckForUpdatesAsync()
    {
        using HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var result = new VersionResult();

        try
        {
            // Perform the asynchronous HTTP request
            var response = await httpClient.GetStringAsync(
                "https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=42767&textOnly=1");

            result.ReceivedVersion = response.Trim();
            result.FrameworkUpToDate = result.ReceivedVersion == CurrentVersion;
            result.WebSuccess = true;
        }
        catch (HttpRequestException ex)
        {
            result.Fault = ex; // Handle network errors
        }
        catch (TaskCanceledException ex)
        {
            result.Fault = ex; // Handle request timeouts
        }
        return result;
    }

    private static void HandleResult(VersionResult result)
    {
        

        // Handle the result or fault
        if (result.Fault != null)
        {
            ProcessError(result);
        }
        else
        {
            ProcessSuccess(result);
        }

        // Log the final state of the version check
        Game.LogTrivial($"DamageTrackerFramework loaded. Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
        Game.DisplayNotification("commonmenu", "card_suit_hearts", $"DamageTrackerFramework {CurrentVersion}",
            "~g~Successfully Loaded",
            $"By Variapolis \nVersion is {(result.WebSuccess ? (result.FrameworkUpToDate ? "~g~Up To Date" : "~r~Out Of Date") : "~o~Version Check Failed")}");
        
        if (LibraryVersion != CurrentVersion)
        {
            Game.LogTrivial($"[VERSION MISMATCH] Online Version: {result.ReceivedVersion ?? "Unable To Retrieve"} | Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("DamageTrackerLib version does not match DamageTrackerFramework. Ensure both DLLs are up to date.");
            Game.DisplayNotification("~r~WARNING: ~w~Version Mismatch for DamageTrackerLib.dll! ~o~Ensure both DLL files are up to date.\n" +
                                     $"~w~Framework Version: ~o~{CurrentVersion}   ~w~Library Version: ~o~{LibraryVersion}");
        }
    }

    private static void ProcessError(VersionResult result)
    {
        if (result.Fault is HttpRequestException)
        {
            Game.LogTrivial($"[VERSION CHECK FAILED] Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("Version check failed, your internet may be disabled, or LCPDFR may be down.");
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Failed.", "Please ensure you are ~o~online~w~.");
        }
        else if (result.Fault is TaskCanceledException)
        {
            Game.LogTrivial($"[VERSION CHECK TIMED OUT] Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~y~DamageTrackerFramework Version Check",
                "Version check ~r~Timed Out.", "The server may be down, or your internet is slow.");
        }
    }

    private static void ProcessSuccess(VersionResult result)
    {
        if (!result.FrameworkUpToDate)
        {
            Game.LogTrivial($"[VERSION OUTDATED] Online Version: {result.ReceivedVersion} | Local DamageTrackerFramework Version: {CurrentVersion} | Local DamageTrackerLib Version: {LibraryVersion}");
            Game.LogTrivial("Please update to the latest version here: https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/");
        }
    }
}
