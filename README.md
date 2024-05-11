# DamageTrackerFramework Documentation

[DamageTrackerFramework](https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/) is a GTA V Plugin that provides a framework and API for developers to get performant, reliable, and detailed events for when Peds are damaged.

## License Information
This project is licensed under the Eclipse Public License 2.0

While it is encouraged to read the full license, a summary of requirements will be listed below.
- You may redistribute and modify the source code, however, any modifications you make to the source code MUST be made open-source.
- The only exceptions to this are any files included alongside this project that do not contain any source code from this project.
- You must include the license within this project alongside any redistributed/modified versions of this project.

## Installation
The DamageTrackerFramework release can be downloaded from the [LSPDFR Website](https://www.lcpdfr.com/downloads/gta5mods/scripts/42767-damage-tracker-framework/)

For installation, the users MUST do the following:

1. Copy DamageTrackerLib.dll and the plugins folder to their GTA V directory.
2. DamageTrackerFramework ********MUST******** be started in RagePluginHook.

## Requirements
- [RagePluginHook](https://discord.gg/ragepluginhook)
## Issues
- Gas Grenades do not work.
## Usage

### Plugin Examples

Usage for the plugin is pretty simple.

1. Ensure that you have a reference to the DamageTrackerLib.dll. You ************DO NOT************ need a reference to the DamageTrackerFramework, but it **MUST** be installed and running in GTA V when your plugin is running.
2. When your plugin is loaded you must start the DamageTrackerService with `DamageTrackerService.Start()` (You must also have a `using DamageTrackerLib;` statement at the top of the file.)
3. Subscribe to the events with a custom function. I.E: `DamageTrackerService.OnPedTookDamage += MyCustomFunction;` and `DamageTrackerService.OnPlayerTookDamage += MyCustomFunction;` Custom function must be in the form of the [Event Delegate](#event-delegate)
4. Unsubscribe to the events when the plugin is unloaded. I.E: `DamageTrackerService.OnPedTookDamage -= MyCustomFunction;` and `DamageTrackerService.OnPlayerTookDamage -= MyCustomFunction;`
5. Stop the DamageTrackerService when the plugin is unloaded with `DamageTrackerService.Stop()`

Below are two example plugins, the full source code for them can be found here: [Variapolis/DamageTrackerExample](https://github.com/Variapolis/DamageTrackerExample)

#### RPH Plugin
```csharp
using DamageTrackerLib;
using DamageTrackerLib.DamageInfo;
using Rage;
using Rage.Attributes;

[assembly: Plugin("DamageTrackerExample", Description = "A plugin for testing.",
    Author = "Variapolis",
    PrefersSingleInstance = true)]

namespace TestCallouts
{
    // ReSharper disable once UnusedType.Global
    public class EntryPoint
    {
        private static GameFiber GameFiber;

        // ReSharper disable once UnusedMember.Global
        public static void Main()
        {
            Game.DisplayNotification("DamageTrackerExample by Variapolis ~g~Successfully Loaded");
            DamageTrackerService.Start();
            DamageTrackerService.OnPedTookDamage += HandleDamage; // C# Event from DamageTrackerService
            DamageTrackerService.OnPlayerTookDamage += HandleDamage; // C# Event from DamageTrackerService
            GameFiber.Hibernate();
        }

        // ReSharper disable once UnusedMember.Global
        public static void OnUnload(bool Exit)
        {
            DamageTrackerService.Stop();
            Game.DisplayNotification("DamageTrackerExample by Variapolis ~r~ Unloaded");
        }

        // This uses a delegate function from DamageTrackerLib - public delegate void PedTookDamageDelegate(Ped victimPed, Ped attackerPed, PedDamageInfo damageInfo)
        private static void HandleDamage(Ped victim, Ped attacker, PedDamageInfo damageInfo) =>
            Game.DisplayHelp($"~w~Ped: {victim.Model.Name} (~r~{damageInfo.Damage} ~b~{damageInfo.ArmourDamage} ~w~Dmg ({(victim.IsAlive ? "~g~Alive" : "~r~Dead")}~w~) " +
                             $"\n~w~Health: ~g~{victim.Health}/{victim.MaxHealth} Armor: ~b~{victim.Armor})" +
                             $"\n~w~Attacker: ~r~{attacker?.Model.Name ?? "None"}" +
                             $"\n~w~Weapon: ~y~{damageInfo.WeaponInfo.Hash.ToString()} {damageInfo.WeaponInfo.Type.ToString()} {damageInfo.WeaponInfo.Group.ToString()}" +
                             $"\n~w~Bone: ~r~{damageInfo.BoneInfo.BoneId.ToString()} {damageInfo.BoneInfo.Limb.ToString()} {damageInfo.BoneInfo.BodyRegion.ToString()}");
    }
}
```
#### LSPDFR Plugin

```csharp
using DamageTrackerLib;
using DamageTrackerLib.DamageInfo;
using LSPD_First_Response.Mod.API;
using Rage;

namespace DamageTrackerLSPDFRExample
{
    public class Main : Plugin
    {
        public override void Initialize()
        {
            Functions.OnOnDutyStateChanged += HandleDutyChanged;
            Game.DisplayNotification("DTF LSPDFR Example Loaded.");
        }

        private void HandleDutyChanged(bool onduty)
        {
            if (onduty)
            {
                DamageTrackerService.Start();
                DamageTrackerService.OnPedTookDamage += HandleDamage;
                DamageTrackerService.OnPlayerTookDamage += HandleDamage;
            }
            else
            {
                DamageTrackerService.Stop();
                DamageTrackerService.OnPedTookDamage -= HandleDamage;
                DamageTrackerService.OnPlayerTookDamage -= HandleDamage;
            }
        }

        private static void HandleDamage(Ped victim, Ped attacker, PedDamageInfo damageInfo) =>
            Game.DisplayHelp($"~w~Ped: {victim.Model.Name} (~r~{damageInfo.Damage} ~b~{damageInfo.ArmourDamage} ~w~Dmg ({(victim.IsAlive ? "~g~Alive" : "~r~Dead")}~w~) " +
                             $"\n~w~Health: ~g~{victim.Health}/{victim.MaxHealth} Armor: ~b~{victim.Armor})" +
                             $"\n~w~Attacker: ~r~{attacker?.Model.Name ?? "None"}" +
                             $"\n~w~Weapon: ~y~{damageInfo.WeaponInfo.Hash.ToString()} {damageInfo.WeaponInfo.Type.ToString()} {damageInfo.WeaponInfo.Group.ToString()}" +
                             $"\n~w~Bone: ~r~{damageInfo.BoneInfo.BoneId.ToString()} {damageInfo.BoneInfo.Limb.ToString()} {damageInfo.BoneInfo.BodyRegion.ToString()}");

        public override void Finally() => Game.Displtification("DTF LSPDFR Example Unloaded.");
    }
}
```


## Documentation

### DamageTrackerService Start() and Stop()

`DamageTrackerService.Start()` and `DamageTrackerService.Stop()` are used to start and stop a GameFiber which collects data the DamageTrackerFramework sends. This GameFiber will not start if it is already running, however, it is recommended to stop it when unloading a plugin to prevent any potential memory leaks.

### Event Delegate

This is the event delegate that is used by the DamageTrackerService. It is a template of what the function that is being subscribed to the event should look like.

```csharp
public delegate void PedTookDamageDelegate(Ped victimPed, Ped attackerPed, PedDamageInfo damageInfo);
```

### Damage Info

Damage info about peds is provided via a PedDamageInfo struct. This holds all the information about how a ped was damaged, including the handle for the ped, the handle for the attacker, the damage received to health, and damage to armor.

```csharp
[Serializable]
public struct PedDamageInfo
{
    public uint PedHandle; // Reference to Ped is provided via the event.
    public uint AttackerPedHandle; // Reference to AttackerPed is provided via the event.
    public int Damage;
    public int ArmourDamage;
    public WeaponDamageInfo WeaponInfo;
    public BoneDamageInfo BoneInfo;
}
```

WeaponDamageInfo provides information on what kind of weapon was used to damage the Ped. This can include firearms and other environmental things such as falling. `WeaponHashes` are grouped into categories such as `DamageType` and `DamageGroup`. The relationships can be found in [Weapon Lookups](#weapon-lookups)

```csharp
[Serializable]
public struct WeaponDamageInfo
{
    public WeaponHash Hash;
    public DamageType Type;
    public DamageGroup Group;
}
```

BoneDamageInfo provides information on what part of the body the Ped was damaged on. `BoneIds` have been grouped into categories such as `Limb` and `BodyRegion`. The relationships can be found in [Bone Lookups](#bone-lookups)

```csharp
[Serializable]
public struct BoneDamageInfo
{
    public BoneId BoneId;
    public Limb Limb;
    public BodyRegion BodyRegion;
}
```

### Bone Lookups

- Lookup Table


    | BoneId | Limb | BodyRegion |
    | --- | --- | --- |
    | BoneId.Root | Limb.Stomach | BodyRegion.Torso |
    | BoneId.LeftThumb1 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftThumb2 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftRingFinger1 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftRingFinger2 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftPinky1 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftPinky2 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftIndexFinger1 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftIndexFinger2 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftMiddleFinger1 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftMiddleFinger2 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.RightClavicle | Limb.Chest | BodyRegion.Torso |
    | BoneId.Pelvis | Limb.Stomach | BodyRegion.Torso |
    | BoneId.LeftFoot | Limb.LeftLeg | BodyRegion.Legs |
    | BoneId.LeftHand | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.Spine | Limb.Stomach | BodyRegion.Torso |
    | BoneId.RightPhFoot | Limb.RightLeg | BodyRegion.Legs |
    | BoneId.Spine1 | Limb.Stomach | BodyRegion.Torso |
    | BoneId.Spine2 | Limb.Chest | BodyRegion.Torso |
    | BoneId.Spine3 | Limb.Chest | BodyRegion.Torso |
    | BoneId.LeftThumb0 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftIndexFinger0 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftMiddleFinger0 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftRingFinger0 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftPinky0 | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.RightForearm | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightPhHand | Limb.RightArm | BodyRegion.Arms |
    | BoneId.Head | Limb.Head | BodyRegion.Head |
    | BoneId.RightCalf | Limb.RightLeg | BodyRegion.Legs |
    | BoneId.Neck | Limb.Head | BodyRegion.Head |
    | BoneId.RightUpperArm | Limb.RightArm | BodyRegion.Arms |
    | BoneId.LeftUpperArm | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.RightThigh | Limb.RightLeg | BodyRegion.Legs |
    | BoneId.RightFoot | Limb.RightLeg | BodyRegion.Legs |
    | BoneId.RightHand | Limb.RightArm | BodyRegion.Arms |
    | BoneId.SpineRoot | Limb.Stomach | BodyRegion.Torso |
    | BoneId.LeftPhFoot | Limb.LeftLeg | BodyRegion.Arms |
    | BoneId.LeftThigh | Limb.LeftLeg | BodyRegion.Legs |
    | BoneId.RightThumb0 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightIndexFinger0 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightMiddleFinger0 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightRingFinger0 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightPinky0 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.LeftPhHand | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftForeArm | Limb.LeftArm | BodyRegion.Arms |
    | BoneId.LeftCalf | Limb.LeftLeg | BodyRegion.Legs |
    | BoneId.RightThumb1 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightThumb2 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightRingFinger1 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightRingFinger2 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightPinky1 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightPinky2 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightIndexFinger1 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightIndexFinger2 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightMiddleFinger1 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.RightMiddleFinger2 | Limb.RightArm | BodyRegion.Arms |
    | BoneId.LeftClavicle | Limb.Chest | BodyRegion.Torso |

### Weapon Lookups

- Lookup Table


    | WeaponHash | DamageGroup | DamageType |
    | --- | --- | --- |
    | WeaponHash.Unknown | DamageGroup.Unknown | DamageType.Unknown |
    | WeaponHash.Antique_Cavalry_Dagger | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Baseball_Bat | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Bottle | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Crowbar | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Fist | DamageGroup.Melee | DamageType.Unarmed |
    | WeaponHash.Flashlight | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Golf_Club | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Hammer | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Hatchet | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Knuckle | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Knife | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Machete | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Switchblade | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Nightstick | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Pipe_Wrench | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Battle_Axe | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Pool_Cue | DamageGroup.Melee | DamageType.MeleeBlunt |
    | WeaponHash.Stone_Hatchet | DamageGroup.Melee | DamageType.MeleeStab |
    | WeaponHash.Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Pistol_MK2 | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Combat_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.APPistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Stun_Gun | DamageGroup.LessThanLethal | DamageType.LessThanLethal |
    | WeaponHash.Pistol50 | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.SNSPistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.SNSPistol_MK2 | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Heavy_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Vintage_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Flare_Gun | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Marksman_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Heavy_Revolver | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Heavy_Revolver_MK2 | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Double_Action | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Up_n_Atomizer | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Micro_SMG | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.SMG | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.SMGMK2 | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.Assault_SMG | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.Combat_PDW | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.Machine_Pistol | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.Mini_SMG | DamageGroup.Bullet | DamageType.SMG |
    | WeaponHash.Unholy_Hellbringer | DamageGroup.Bullet | DamageType.MG |
    | WeaponHash.Pump_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Pump_Shotgun_MK2 | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Sawed_Off_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Assault_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Bullpup_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Musket | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Heavy_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Double_Barrel_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Sweeper_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Assault_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Assault_Rifle_MK2 | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Carbine_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Carbine_Rifle_MK2 | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Advanced_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Special_Carbine | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Special_Carbine_MK2 | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Bullpup_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Bullpup_Rifle_MK2 | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Compact_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.MG | DamageGroup.Bullet | DamageType.MG |
    | WeaponHash.Combat_MG | DamageGroup.Bullet | DamageType.MG |
    | WeaponHash.Combat_MGMK2 | DamageGroup.Bullet | DamageType.MG |
    | WeaponHash.Gusenberg_Sweeper | DamageGroup.Bullet | DamageType.MG |
    | WeaponHash.Sniper_Rifle | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Heavy_Sniper | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Heavy_Sniper_MK2 | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Marksman_Rifle | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Marksman_Rifle_MK2 | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.RPG | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Grenade_Launcher | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Smoke_Grenade_Launcher | DamageGroup.NonDamaging | DamageType.Launcher |
    | WeaponHash.Minigun | DamageGroup.Bullet | DamageType.Launcher |
    | WeaponHash.Firework_Launcher | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Railgun | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Homing_Launcher | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Compact_Grenade_Launcher | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Ray_Minigun | DamageGroup.Bullet | DamageType.Launcher |
    | WeaponHash.Grenade | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.BZGas | DamageGroup.Gas | DamageType.Gas |
    | WeaponHash.Smoke_Grenade | DamageGroup.NonDamaging | DamageType.ThrowableNonLethal |
    | WeaponHash.Flare | DamageGroup.NonDamaging | DamageType.ThrowableNonLethal |
    | WeaponHash.Molotov | DamageGroup.Fire | DamageType.Fire |
    | WeaponHash.Sticky_Bomb | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.Proximity_Mine | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.Snowball | DamageGroup.NonDamaging | DamageType.ThrowableNonLethal |
    | WeaponHash.Pipe_Bomb | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.Baseball | DamageGroup.NonDamaging | DamageType.ThrowableNonLethal |
    | WeaponHash.Jerry_Can | DamageGroup.NonDamaging | DamageType.Misc |
    | WeaponHash.Fire_Extinguisher | DamageGroup.NonDamaging | DamageType.Misc |
    | WeaponHash.Parachute | DamageGroup.Unknown | DamageType.Unknown |
    | WeaponHash.Electric_Fence | DamageGroup.Environmental | DamageType.Electric |
    | WeaponHash.Hitby_Water_Cannon | DamageGroup.WaterCannon | DamageType.WaterCannon |
    | WeaponHash.Rammedby_Car | DamageGroup.Vehicle | DamageType.Vehicle |
    | WeaponHash.Run_Overby_Car | DamageGroup.Vehicle | DamageType.Vehicle |
    | WeaponHash.Fall | DamageGroup.Fall | DamageType.Fall |
    | WeaponHash.Animal | DamageGroup.Animal | DamageType.Animal |
    | WeaponHash.Airstrike_Rocket | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Bleeding | DamageGroup.Bodily | DamageType.Bodily |
    | WeaponHash.Briefcase | DamageGroup.Unknown | DamageType.Unknown |
    | WeaponHash.Briefcase02 | DamageGroup.Unknown | DamageType.Unknown |
    | WeaponHash.Cougar | DamageGroup.Animal | DamageType.Animal |
    | WeaponHash.Barbed_Wire | DamageGroup.Environmental | DamageType.BarbedWire |
    | WeaponHash.Drowning | DamageGroup.Drowning | DamageType.Drowning |
    | WeaponHash.Drowning_In_Vehicle | DamageGroup.Drowning | DamageType.Drowning |
    | WeaponHash.Explosion | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.Exhaustion | DamageGroup.Bodily | DamageType.Bodily |
    | WeaponHash.Fire | DamageGroup.Fire | DamageType.Fire |
    | WeaponHash.Heli_Crash | DamageGroup.Explosion | DamageType.Explosive |
    | WeaponHash.Vehicle_Rocket | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Vehicle_Akula_Barrage | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Akula_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Akula_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Akula_Turret_Dual | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Akula_Turret_Single | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_APCCannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_APCMG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_APCMissile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Ardent_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Avenger_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Barrage_Rear_GL | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Barrage_Rear_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Barrage_Rear_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Barrage_Top_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Barrage_Top_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Bombushka_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Bombushka_Dual_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Cannon_Blazer | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Caracara_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Caracara_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Cherno_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Comet_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Deluxo_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Deluxo_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Dogfighter_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Dogfighter_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Dune_Grenade_Launcher | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Dune_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Dune_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Enemy_Laser | DamageGroup.Unknown | DamageType.Unknown |
    | WeaponHash.Vehicle_Hacker_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Hacker_Missile_Homing | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Halftrack_Dual_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Halftrack_Quad_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Havok_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Hunter_Barrage | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Hunter_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Hunter_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Hunter_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Insurgent_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Khanjali_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Khanjali_Cannon_Heavy | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Khanjali_GL | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Khanjali_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Menacer_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Microlight_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mobileops_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Mogul_Dual_Nose | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mogul_Dual_Turret | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mogul_Nose | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mogul_Turret | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mule4MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Mule4Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Mule4Turret_GL | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Nightshark_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Nose_Turret_Valkyrie | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Oppressor_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Oppressor_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Oppressor2Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Oppressor2MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Oppressor2Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Plane_Rocket | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Player_Buzzard | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Player_Lazer | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Player_Savage | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Pounder2Barrage | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Pounder2GL | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Pounder2Mini | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Pounder2Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Radar | DamageGroup.NonDamaging | DamageType.NonDamaging |
    | WeaponHash.Vehicle_Revolter_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Rogue_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Rogue_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Rogue_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Ruiner_Bullet | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Ruiner_Rocket | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Savestra_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Scramjet_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Scramjet_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Seabreeze_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Searchlight | DamageGroup.NonDamaging | DamageType.NonDamaging |
    | WeaponHash.Vehicle_Space_Rocket | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Speedo4MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Speedo4Turret_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Speedo4Turret_Mini | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Strikeforce_Barrage | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Strikeforce_Cannon | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Strikeforce_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Subcar_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Subcar_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Subcar_Torpedo | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Tampa_Dual_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tampa_Fixed_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tampa_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Tampa_Mortar | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Tank | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Technical_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Thruster_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Thruster_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Trailer_Dualaa | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Trailer_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Trailer_Quad_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tula_Dual_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tula_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tula_Minigun | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Tula_Nose_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Turret_Boxville | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Turret_Insurgent | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Turret_Limo | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Turret_Technical | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Turret_Valkyrie | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Vigilante_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Vigilante_Missile | DamageGroup.Explosion | DamageType.VehicleLauncher |
    | WeaponHash.Vehicle_Viseris_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Volatol_Dual_MG | DamageGroup.Bullet | DamageType.VehicleFirearm |
    | WeaponHash.Vehicle_Water_Cannon | DamageGroup.WaterCannon | DamageType.WaterCannon |
    | WeaponHash.Vehicle_Helicopter_Rotors | DamageGroup.Vehicle | DamageType.Vehicle |
    | WeaponHash.Military_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Heavy_Rifle | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Service_Carbine | DamageGroup.Bullet | DamageType.Rifle |
    | WeaponHash.Precision_Rifle | DamageGroup.Bullet | DamageType.Sniper |
    | WeaponHash.Combat_Shotgun | DamageGroup.Bullet | DamageType.Shotgun |
    | WeaponHash.Compact_EMP_Launcher | DamageGroup.Explosion| DamageType.Launcher |
    | WeaponHash.Railgun_XM3 | DamageGroup.Explosion | DamageType.Launcher |
    | WeaponHash.Stun_Gun_MP | DamageGroup.LessThanLethal | DamageType.LessThanLethal |
    | WeaponHash.Perico_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.WM29_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Navy_Revolver | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Ceramic_Pistol | DamageGroup.Bullet | DamageType.Pistol |
    | WeaponHash.Candy_Cane | DamageGroup.Melee | DamageType.MeleeBlunt |
    
    Planned:
    | WeaponHash | DamageGroup | DamageType |
    | --- | --- | --- |
    | WeaponHash.Custom | Currently Unused | Currently Unused |
   
