namespace DamageTrackerLib
{
    internal static class Wrapper
    {
        // static Wrapper() => AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        //
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // public static void Start()
        // {
        //     Game.LogTrivial("Started!");
        //     var beforeCount = DamageTrackingFramework.DamageTracker.DamageSubCount;
        //     DamageTrackingFramework.DamageTracker.OnPedTookDamage += OnPlayerTookDamage;
        //     Game.LogTrivial($"[Start] {beforeCount} to {DamageTrackingFramework.DamageTracker.DamageSubCount}");
        // }
        //
        // [MethodImpl(MethodImplOptions.NoInlining)]
        // public static void Call()
        // {
        //     Game.LogTrivial($"[Call] {DamageTrackingFramework.DamageTracker.DamageSubCount}");
        //     // DamageTrackingFramework.EntryPoint.GetPlayer();
        // }
        //
        // private static void PrintPed(Ped ped, PedDamageInfo info)
        // {
        //     Game.DisplayHelp($"{ped.Model.Name} {info.Damage} {info.WeaponInfo.Hash.ToString()} {info.BoneInfo.BoneId.ToString()}");
        // }
        //
        //
        // private static void OnPlayerTookDamage(Ped ped, PedDamageInfo info)
        // {
        //     Game.LogTrivial("Damaged!");
        //     // Game.DisplayHelp($"~r~Player ~w~took damage from ~g~{info.WeaponInfo.Name} ~w~in ~y~{info.BoneInfo.Limb.ToString()}");
        // }
        //
        // private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        // {
        //     if (!args.Name.StartsWith("DamageTrackingFramework")) return null;
        //     Game.LogTrivial("Resolved!");
        //     return Assembly.Load(File.ReadAllBytes(@"DamageTrackingFramework.dll"));
        // }
    }
}