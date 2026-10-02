global using BTD_Mod_Helper;
global using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2Cpp;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.TowerFilters;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

[assembly: MelonInfo(
    typeof(RYNO.RYNO_Mod),
    "RYNO",
    "1.0.0",
    "HopeZEXAL"
)]

[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6-Epic")]

namespace RYNO;

public class RYNO_Mod : BloonsTD6Mod
{
    public override void OnApplicationStart()
    {
        ModHelper.Msg<RYNO_Mod>("RYNO LOADED!");
    }
}


public class RYNO : ModHero
{
    public override string BaseTower => TowerType.DartlingGunner;
    public override int Cost => 4000;
    public override string DisplayName => "R.Y.N.O.";
    public override string Title => "Rapid Yield Neutralization Operator";
    public override string Description => "Ripping Bloons a New One since 2010.";

    public override string Level1Description =>
        "R.Y.N.O. fires a powerful minigun at Bloons.";

    public override int MaxLevel => 20;
    public override float XpRatio => 0.40f;

    public override string NameStyle => TowerType.Silas;
    public override string BackgroundStyle => TowerType.Silas;
    public override string GlowStyle => TowerType.Silas;

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        // Level 1-9 use the original custom R.Y.N.O. texture.
        // Level 10-19 and Level 20 have their own ModTowerDisplay classes.
        towerModel.SetDisplay<RYNODisplay>();

        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        var emission =
            weapon.emission as RandomEmissionModel;

        if (emission != null)
        {
            emission.angle = 7f;
        }
    }
}


// ============================================================
// LEVEL 1-9 R.Y.N.O. DISPLAY
// ============================================================
// Uses the original Dartling Gunner 000 display and applies the
// custom R.Y.N.O. texture used by the early levels.
// ============================================================

public class RYNODisplay : ModDisplay
{
    public override string BaseDisplay =>
        GetDisplay(
            TowerType.DartlingGunner,
            0,
            0,
            0
        );

    public override void ModifyDisplayNode(
        Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        SetMeshTexture(
            node,
            Name
        );
    }
}


// ============================================================
// LEVEL 2
// ============================================================

public class RYNOLevel2 : ModHeroLevel<RYNO>
{
    public override int Level => 2;

    public override string Description =>
        "Increased projectile pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        weapon.projectile.pierce += 2;
    }
}


// ============================================================
// LEVEL 3
// ============================================================

public class RYNOLevel3 : ModHeroLevel<RYNO>
{
    public override int Level => 3;

    public override string Description =>
        "Armor-piercing rounds can pop Lead and deal bonus damage to Lead and Ceramic Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        var projectile =
            weapon.projectile;

        var damageModel =
            projectile.GetBehavior<DamageModel>();

        if (damageModel != null)
        {
            damageModel.immuneBloonProperties &=
                ~BloonProperties.Lead;
        }

        projectile.AddBehavior(
            new DamageModifierForTagModel(
                "RYNO_LeadDamage",
                "Lead",
                1f,
                2f,
                false,
                false
            )
            {
                tags = new[] { "Lead" },
                collisionPass = 0
            }
        );

        projectile.AddBehavior(
            new DamageModifierForTagModel(
                "RYNO_CeramicDamage",
                "Ceramic",
                1f,
                2f,
                false,
                false
            )
            {
                tags = new[] { "Ceramic" },
                collisionPass = 0
            }
        );

        projectile.hasDamageModifiers = true;
    }
}


// ============================================================
// LEVEL 4
// ============================================================

public class RYNOLevel4 : ModHeroLevel<RYNO>
{
    public override int Level => 4;

    public override string Description =>
        "High-caliber ammunition increases damage and pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        var projectile =
            weapon.projectile;

        projectile.pierce += 3;

        var damageModel =
            projectile.GetBehavior<DamageModel>();

        if (damageModel != null)
        {
            damageModel.damage += 2;
        }
    }
}


// ============================================================
// LEVEL 5
// ============================================================

public class RYNOLevel5 : ModHeroLevel<RYNO>
{
    public override int Level => 5;

    public override string Description =>
        "Tactical stabilizers reduce the minigun's projectile spread.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        var emission =
            weapon.emission as RandomEmissionModel;

        if (emission != null)
        {
            // Level 1 uses a 7 degree spread.
            // Cut that spread in half.
            emission.angle *= 0.5f;
        }
    }
}


// ============================================================
// LEVEL 6
// ============================================================

public class RYNOLevel6 : ModHeroLevel<RYNO>
{
    public override int Level => 6;

    public override string Description =>
        "Increases cash generation by 50%.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var cashIncrease =
            towerModel.GetBehavior<CashIncreaseModel>();

        if (cashIncrease == null)
        {
            towerModel.AddBehavior(
                new CashIncreaseModel(
                    "RYNO_CashIncrease",
                    0f,
                    1.5f
                )
            );
        }
        else
        {
            cashIncrease.multiplier = 1.5f;
        }
    }
}


// ============================================================
// LEVEL 7
// ============================================================

public class RYNOLevel7 : ModHeroLevel<RYNO>
{
    public override int Level => 7;

    public override string Description =>
        "High-velocity rounds travel faster.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        var travelModel =
            weapon.projectile.GetBehavior<TravelStraitModel>();

        if (travelModel != null)
        {
            travelModel.speed *= 1.25f;
        }
    }
}


// ============================================================
// LEVEL 8
// ============================================================

public class RYNOLevel8 : ModHeroLevel<RYNO>
{
    public override int Level => 8;

    public override string Description =>
        "Adds standard targeting options.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        attackModel.AddBehavior(
            new TargetFirstModel(
                "RYNO_TargetFirst",
                true,
                false
            )
        );

        attackModel.AddBehavior(
            new TargetLastModel(
                "RYNO_TargetLast",
                true,
                false
            )
        );

        attackModel.AddBehavior(
            new TargetCloseModel(
                "RYNO_TargetClose",
                true,
                false
            )
        );

        attackModel.AddBehavior(
            new TargetStrongModel(
                "RYNO_TargetStrong",
                true,
                false
            )
        );

        var rotateToPointer =
            attackModel.GetBehavior<RotateToPointerModel>();

        if (rotateToPointer != null)
        {
            attackModel.AddBehavior(
                new RotateToTargetModel(
                    "RYNO_RotateToTarget",
                    rotateToPointer.rotateOnlyOnEmit,
                    rotateToPointer.rotateTower,
                    true,
                    1,
                    true,
                    true
                )
            );
        }

        towerModel.UpdateTargetProviders();
    }
}


// ============================================================
// LEVEL 9
// ============================================================

public class RYNOLevel9 : ModHeroLevel<RYNO>
{
    public override int Level => 9;

    public override string Description =>
        "Gains Camo detection.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.AddBehavior(
            new OverrideCamoDetectionModel(
                "RYNO_CamoDetection",
                true
            )
        );
    }
}


// ============================================================
// LEVEL 10
// ============================================================

public class RYNOLevel10 : ModHeroLevel<RYNO>
{
    public override int Level => 10;

    public override string Description =>
        "Trades darts for Rockets";

    // Level 10+ hero menu artwork.
    // These names refer to embedded PNG resources without the .png extension.
    public override string Portrait =>
        "RYNOLevel10-Portrait";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var hydraTower =
            Game.instance.model.GetTowerFromId(
                "DartlingGunner-030"
            );

        if (hydraTower == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 10: Could not find DartlingGunner-030!"
            );

            return;
        }

        var hydraAttack =
            hydraTower.GetAttackModel();

        if (hydraAttack == null ||
            hydraAttack.weapons == null ||
            hydraAttack.weapons.Length == 0 ||
            hydraAttack.weapons[0] == null ||
            hydraAttack.weapons[0].projectile == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 10: Hydra attack/projectile is invalid!"
            );

            return;
        }

        var attackModel =
            towerModel.GetAttackModel();

        if (attackModel == null ||
            attackModel.weapons == null ||
            attackModel.weapons.Length == 0 ||
            attackModel.weapons[0] == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 10: R.Y.N.O. attack/weapon is invalid!"
            );

            return;
        }

        var weapon =
            attackModel.weapons[0];

        // Replace the primary projectile with an exact copy of the
        // Hydra Rocket Pods projectile. This carries over the Hydra
        // rocket display, damage, pierce, collision behavior, and
        // both explosion behaviors from DartlingGunner-030.
        weapon.projectile =
            hydraAttack.weapons[0].projectile.Duplicate();

        // Safeguard: the Level 10 Hydra projectile can damage every
        // Bloon type. This is also preserved by the Level 20 conversion.
        var hydraDamage =
            weapon.projectile.GetBehavior<DamageModel>();

        if (hydraDamage != null)
        {
            hydraDamage.immuneBloonProperties = BloonProperties.None;
            hydraDamage.immuneBloonPropertiesOriginal = BloonProperties.None;
        }

        // Apply the black/blue Hydra texture to the fired rockets.
        weapon.projectile.ApplyDisplay<RYNOHydraRocketDisplay>();

        // The Hydra rocket carries its own explosion projectiles.
        // The diagnostic hooks that inspected those projectiles have
        // been removed, so there is nothing else to do here.

        // Keep the R.Y.N.O. targeting system from earlier levels.
        // Level 8's Normal/First/Last/Close/Strong/Locked targeting
        // behaviors and the targeting Harmony fixes remain untouched.
        towerModel.UpdateTargetProviders();

        ModHelper.Msg<RYNO_Mod>(
            "RYNO Level 10: Hydra Rocket Pods model and projectile applied!"
        );
    }
}


// ============================================================
// LEVEL 11
// ============================================================

public class RYNOLevel11 : ModHeroLevel<RYNO>
{
    public override int Level => 11;

    public override string Description =>
        "Rapid Cycling increases fire rate.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        var weapon =
            attackModel.weapons[0];

        // 25% faster firing.
        weapon.rate *= 0.75f;
    }
}


// ============================================================
// LEVEL 12
// ============================================================

public class RYNOLevel12 : ModHeroLevel<RYNO>
{
    public override int Level => 12;

    public override string Description =>
        "Increases cash generation by 100%.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var cashIncrease =
            towerModel.GetBehavior<CashIncreaseModel>();

        if (cashIncrease != null)
        {
            cashIncrease.multiplier = 2f;
        }
        else
        {
            towerModel.AddBehavior(
                new CashIncreaseModel(
                    "RYNO_CashIncrease",
                    0f,
                    2f
                )
            );
        }
    }
}


// ============================================================
// LEVEL 13
// ============================================================

public class RYNOLevel13 : ModHeroLevel<RYNO>
{
    public override int Level => 13;

    public override string Description =>
        "+5 damage against Lead, Ceramic, Fortified, and MOAB-Class Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;

        AddBonus(projectile, "Lead", "RYNO_Level13_Lead", 5f);
        AddBonus(projectile, "Ceramic", "RYNO_Level13_Ceramic", 5f);
        AddBonus(projectile, "Fortified", "RYNO_Level13_Fortified", 5f);
        AddBonus(projectile, "Moab", "RYNO_Level13_Moab", 5f);

        projectile.hasDamageModifiers = true;
    }

    private static void AddBonus(
        ProjectileModel projectile,
        string tag,
        string id,
        float bonus)
    {
        projectile.AddBehavior(
            new DamageModifierForTagModel(
                id,
                tag,
                1f,
                bonus,
                false,
                false)
            {
                tags = new[] { tag },
                collisionPass = 0
            });
    }
}


// ============================================================
// LEVEL 14
// ============================================================

public class RYNOLevel14 : ModHeroLevel<RYNO>
{
    public override int Level => 14;

    public override string Description =>
        "+2 damage, +1 pierce, and +15% projectile flight speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;

        projectile.pierce += 1;

        var damage = projectile.GetBehavior<DamageModel>();
        if (damage != null)
            damage.damage += 2;

        var travel = projectile.GetBehavior<TravelStraitModel>();
        if (travel != null)
            travel.speed *= 1.15f;
    }
}


// ============================================================
// LEVEL 15
// ============================================================

public class RYNOLevel15 : ModHeroLevel<RYNO>
{
    public override int Level => 15;

    public override string Description =>
        "Incendiary rounds ignite Bloons, dealing damage over time.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        if (attackModel == null ||
            attackModel.weapons == null ||
            attackModel.weapons.Length == 0 ||
            attackModel.weapons[0] == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 15: R.Y.N.O. attack/weapon is invalid!"
            );

            return;
        }

        var weapon =
            attackModel.weapons[0];

        var projectile =
            weapon.projectile;

        if (projectile == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 15: R.Y.N.O. projectile is invalid!"
            );

            return;
        }

        // ----------------------------------------------------
        // COPY THE EXACT HOT SHOT BURN BEHAVIOR
        // ----------------------------------------------------
        // MonkeyBuccaneer-020 contains the vanilla Hot Shot
        // projectile behavior. In the exported model, the burn
        // is on the Grape Shot Primary weapon (weapon index 2).
        //
        // We duplicate the AddBehaviorToBloonModel so R.Y.N.O.
        // receives the same fire-based DamageOverTimeModel:
        //
        //   Damage:       2
        //   Interval:     1.5 seconds
        //   Lifespan:     3.1 seconds
        //   Fire based:   true
        //   Overlay:      Fire
        //
        // The duplicate is then given its own mutation ID so it
        // belongs to R.Y.N.O. rather than sharing Buccaneer's ID.

        var hotShotTower =
            Game.instance.model.GetTowerFromId(
                "MonkeyBuccaneer-020"
            );

        if (hotShotTower == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 15: Could not find MonkeyBuccaneer-020!"
            );

            return;
        }

        var hotShotAttack =
            hotShotTower.GetAttackModel();

        if (hotShotAttack == null ||
            hotShotAttack.weapons == null ||
            hotShotAttack.weapons.Length <= 2 ||
            hotShotAttack.weapons[2] == null ||
            hotShotAttack.weapons[2].projectile == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 15: Hot Shot Grape Shot projectile is invalid!"
            );

            return;
        }

        var hotShotProjectile =
            hotShotAttack.weapons[2].projectile;

        var hotShotBurn =
            hotShotProjectile.GetBehavior<AddBehaviorToBloonModel>();

        if (hotShotBurn == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 15: Could not find Hot Shot burn behavior!"
            );

            return;
        }

        var rynoBurn =
            hotShotBurn.Duplicate();

        rynoBurn.mutationId =
            "Dot:RYNO";

        // Double the Hot Shot burn damage and halve its interval.
        // The DamageOverTimeModel in this BTD6 build does not expose
        // lifespan/lifespanFrames, so those values are left unchanged.
        // AddBehaviorToBloonModel contains the actual DOT model.
        // Use the BTD6 Mod Helper-specific enumerator for this nested
        // behavior instead of Model.GetDescendants(), which returns an
        // IL2CPP IEnumerator that C# cannot foreach over directly.
        foreach (var dot in
            AddBehaviorToBloonModelBehaviorExt.GetBehaviors<DamageOverTimeModel>(rynoBurn))
        {
            dot.damage *= 2f;
            dot.interval *= 0.5f;
        }

        projectile.AddBehavior(rynoBurn);

        ModHelper.Msg<RYNO_Mod>(
            "RYNO Level 15: Incendiary Rounds activated!"
        );
    }
}


// ============================================================
// LEVEL 16
// ============================================================

public class RYNOLevel16 : ModHeroLevel<RYNO>
{
    public override int Level => 16;

    public override string Description =>
        "+5 projectile damage and +2 projectile pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;

        projectile.pierce += 2;

        var damage = projectile.GetBehavior<DamageModel>();
        if (damage != null)
            damage.damage += 5;
    }
}


// ============================================================
// LEVEL 17
// ============================================================

public class RYNOLevel17 : ModHeroLevel<RYNO>
{
    public override int Level => 17;

    public override string Description =>
        "Increases R.Y.N.O.'s radius by 250% and grants nearby towers +33% attack speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        // +250% radius means 350% of the original radius.
        towerModel.range *= 3.5f;

        RYNOAuraSupport.AddRateAura(towerModel, 1f / 1.33f,
            "RYNO_Level17_RateAura");
    }
}


// ============================================================
// LEVEL 18
// ============================================================

public class RYNOLevel18 : ModHeroLevel<RYNO>
{
    public override int Level => 18;

    public override string Description =>
        "Towers in R.Y.N.O.'s radius gain +3 damage and +3 pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        RYNOAuraSupport.AddDamageAura(towerModel, 3f,
            "RYNO_Level18_DamageAura");
        RYNOAuraSupport.AddPierceAura(towerModel, 3f,
            "RYNO_Level18_PierceAura");
    }
}


// ============================================================
// LEVEL 19
// ============================================================

public class RYNOLevel19 : ModHeroLevel<RYNO>
{
    public override int Level => 19;

    public override string Description =>
        "Towers in R.Y.N.O.'s radius generate 50% more cash.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        // Native Monkey City-style income support gives nearby towers
        // the requested +50% cash generation.
        RYNOAuraSupport.AddCashAura(
            towerModel,
            1.5f,
            "RYNO_Level19_CashAura"
        );

        // Pop-generation support is not exposed as a standalone
        // SupportModel in the BTD6 model API available to this build.
        // We intentionally do not fake it with Call to Arms because
        // that would also apply unrelated attack-speed/damage buffs.
    }
}

class RYNOLevel20 : ModHeroLevel<RYNO>
{
    public override int Level => 20;

    // Level 20 hero menu artwork.
    public override string Portrait =>
        "RYNOLevel20-Portrait";

    public override string Description =>
        "Rip Ya a New One.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel =
            towerModel.GetAttackModel();

        if (attackModel == null ||
            attackModel.weapons == null ||
            attackModel.weapons.Length == 0 ||
            attackModel.weapons[0] == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 20: R.Y.N.O. attack/weapon is invalid!"
            );
            return;
        }

        var weapon =
            attackModel.weapons[0];

        if (weapon.projectile == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 20: R.Y.N.O. projectile is null!"
            );
            return;
        }

        // ----------------------------------------------------
        // GET VANILLA ADORA
        // ----------------------------------------------------
        // Level 20 permanently uses Adora's projectile style.
        // No Hydra projectile, toggle, or ability is used here.
        var adoraTower =
            Game.instance.model.GetTowerFromId("Adora");

        if (adoraTower == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 20: Could not find Adora!"
            );
            return;
        }

        var adoraAttack =
            adoraTower.GetAttackModel();

        if (adoraAttack == null ||
            adoraAttack.weapons == null ||
            adoraAttack.weapons.Length == 0 ||
            adoraAttack.weapons[0] == null ||
            adoraAttack.weapons[0].projectile == null ||
            adoraAttack.weapons[0].emission == null)
        {
            ModHelper.Msg<RYNO_Mod>(
                "RYNO Level 20: Adora projectile/emission is invalid!"
            );
            return;
        }

        var adoraWeapon =
            adoraAttack.weapons[0];

        var oldProjectile =
            weapon.projectile;

        var adoraProjectile =
            adoraWeapon.projectile.Duplicate();

        // ----------------------------------------------------
        // PRESERVE R.Y.N.O. PRIMARY PROJECTILE UPGRADES
        // ----------------------------------------------------
        // Levels 10-19 changed the primary projectile to Hydra.
        // Compare the current Level 19 projectile against vanilla
        // Hydra so the R.Y.N.O.-specific numeric upgrades carry over
        // to Adora's projectile.
        var vanillaHydraTower =
            Game.instance.model.GetTowerFromId(
                "DartlingGunner-030"
            );

        if (vanillaHydraTower != null)
        {
            var vanillaHydraAttack =
                vanillaHydraTower.GetAttackModel();

            if (vanillaHydraAttack != null &&
                vanillaHydraAttack.weapons != null &&
                vanillaHydraAttack.weapons.Length > 0 &&
                vanillaHydraAttack.weapons[0] != null &&
                vanillaHydraAttack.weapons[0].projectile != null)
            {
                var vanillaHydraProjectile =
                    vanillaHydraAttack.weapons[0].projectile;

                // Preserve R.Y.N.O.'s pierce upgrades.
                var pierceDifference =
                    oldProjectile.pierce -
                    vanillaHydraProjectile.pierce;

                adoraProjectile.pierce +=
                    pierceDifference;

                var oldDamage =
                    oldProjectile.GetBehavior<DamageModel>();

                var vanillaHydraDamage =
                    vanillaHydraProjectile.GetBehavior<DamageModel>();

                var adoraDamage =
                    adoraProjectile.GetBehavior<DamageModel>();

                if (oldDamage != null &&
                    vanillaHydraDamage != null &&
                    adoraDamage != null)
                {
                    var damageDifference =
                        oldDamage.damage -
                        vanillaHydraDamage.damage;

                    adoraDamage.damage +=
                        damageDifference;

                    adoraDamage.immuneBloonProperties =
                        oldDamage.immuneBloonProperties;

                    adoraDamage.immuneBloonPropertiesOriginal =
                        oldDamage.immuneBloonPropertiesOriginal;
                }

                // Preserve R.Y.N.O.'s projectile-speed upgrades.
                var oldTravel =
                    oldProjectile.GetBehavior<TravelStraitModel>();

                var vanillaHydraTravel =
                    vanillaHydraProjectile.GetBehavior<TravelStraitModel>();

                var adoraTravel =
                    adoraProjectile.GetBehavior<TravelStraitModel>();

                if (oldTravel != null &&
                    vanillaHydraTravel != null &&
                    adoraTravel != null &&
                    vanillaHydraTravel.speed != 0f)
                {
                    var speedMultiplier =
                        oldTravel.speed /
                        vanillaHydraTravel.speed;

                    adoraTravel.speed *=
                        speedMultiplier;
                }
            }
        }

        // ----------------------------------------------------
        // COPY R.Y.N.O.-SPECIFIC PROJECTILE BEHAVIORS
        // ----------------------------------------------------
        // Preserve the Level 3/16 damage modifiers and the Level 15
        // incendiary behavior. Do NOT copy Hydra's explosion behavior.
        foreach (var behavior in oldProjectile.behaviors)
        {
            if (behavior == null)
                continue;

            var typeName =
                behavior.GetIl2CppType().Name;

            if (typeName == "DamageModifierForTagModel" ||
                typeName == "AddBehaviorToBloonModel")
            {
                var copiedBehavior =
                    behavior.Duplicate();

                if (typeName == "AddBehaviorToBloonModel")
                {
                    var burnBehavior =
                        copiedBehavior.Cast<AddBehaviorToBloonModel>();

                    burnBehavior.mutationId =
                        "Dot:RYNO";
                }

                adoraProjectile.AddBehavior(
                    copiedBehavior
                );
            }
        }

        // ----------------------------------------------------
        // LEVEL 20 FINAL PROJECTILE BONUSES
        // ----------------------------------------------------
        // These are applied after the Adora conversion so they remain
        // present on the final five-bolt homing projectile.
        adoraProjectile.pierce += 5;

        var finalAdoraDamage =
            adoraProjectile.GetBehavior<DamageModel>();

        if (finalAdoraDamage != null)
            finalAdoraDamage.damage += 5;

        // Level 20 safeguard: Adora's bolts can damage every Bloon type.
        if (finalAdoraDamage != null)
        {
            finalAdoraDamage.immuneBloonProperties = BloonProperties.None;
            finalAdoraDamage.immuneBloonPropertiesOriginal = BloonProperties.None;
        }

        adoraProjectile.AddBehavior(
            new DamageModifierForTagModel(
                "RYNO_Level20_Ceramic",
                "Ceramic",
                1f,
                5f,
                false,
                false)
            {
                tags = new[] { "Ceramic" },
                collisionPass = 0
            });

        adoraProjectile.AddBehavior(
            new DamageModifierForTagModel(
                "RYNO_Level20_Fortified",
                "Fortified",
                1f,
                5f,
                false,
                false)
            {
                tags = new[] { "Fortified" },
                collisionPass = 0
            });

        adoraProjectile.AddBehavior(
            new DamageModifierForTagModel(
                "RYNO_Level20_Moab",
                "Moab",
                1f,
                5f,
                false,
                false)
            {
                tags = new[] { "Moab" },
                collisionPass = 0
            });

        adoraProjectile.hasDamageModifiers =
            true;

        // ----------------------------------------------------
        // REAL ADORA FIVE-BOLT EMISSION
        // ----------------------------------------------------
        // Keep Adora's native emission type so the projectile keeps
        // Adora's normal homing/firing behavior.
        var adoraEmission =
            adoraWeapon.emission
                .Duplicate()
                .Cast<AdoraEmissionModel>();

        adoraEmission.count = 3;
        adoraEmission.angleBetween = 30f;

        // Permanently use Adora's projectile and emission.
        weapon.projectile =
            adoraProjectile;

        weapon.emission =
            adoraEmission;

        weapon.name =
            "RYNO_Level20_AdoraWeapon";

        // ----------------------------------------------------
        // KEEP THE ROCKET STORM PHYSICAL MODEL
        // ----------------------------------------------------
        // The Level 20 ModTowerDisplay below supplies the Rocket Storm
        // monkey/gun appearance. We do NOT copy Rocket Storm's attack.
        towerModel.UpdateTargetProviders();

        ModHelper.Msg<RYNO_Mod>(
            "RYNO Level 20: Rocket Storm model + permanent five-bolt Adora mode applied!"
        );
    }
}


// ============================================================
// LEVEL 20 ADORA PROJECTILE DISPLAY
// ============================================================
// Keep Adora's projectile completely vanilla for now.
// The projectile uses Adora's native display, including its original
// gold/white colors, homing effects, and particles. R.Y.N.O.'s Level 20
// visual identity will instead be handled by the monkey model.

// ============================================================
// LEVEL 10+ HYDRA DISPLAY
// ============================================================
// Heroes are represented by Mod Helper as Level-0-0 tower tiers,
// so tiers[0] is the hero level. ModTowerDisplay is used here
// instead of changing TowerModel.display inside ApplyUpgrade.
// This lets Mod Helper re-apply the visual when the hero upgrades.
public class RYNOHydraDisplay : ModTowerDisplay<RYNO>
{
    public override string BaseDisplay =>
        "028a7597e522bb14688c8e2fc762f9b7";

    public override bool UseForTower(int[] tiers)
    {
        return tiers != null &&
               tiers.Length > 0 &&
               tiers[0] >= 10 &&
               tiers[0] < 20;
    }

    public override void ModifyDisplayNode(
        Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        // Use the Hydra model, but replace its atlas with the
        // custom black/blue Hydra texture.
        SetMeshTexture(node, "RYNO_Hydra_BlackBlue");
    }
}


// ============================================================
// LEVEL 20 ROCKET STORM DISPLAY
// ============================================================
// At Level 20 R.Y.N.O. physically switches to the vanilla
// Dartling Gunner 0-4-0 Rocket Storm model.
//
// DartlingGunner-040 display:
// ae4c02ac42860384484949a6347c7d77
//
// This is intentionally separate from the Level 20 projectile
// upgrade. The Level 20 upgrade changes the attack/projectile,
// while this display changes the physical tower model.
public class RYNORocketStormDisplay : ModTowerDisplay<RYNO>
{
    public override string BaseDisplay =>
        "ae4c02ac42860384484949a6347c7d77";

    public override bool UseForTower(int[] tiers)
    {
        return tiers != null &&
               tiers.Length > 0 &&
               tiers[0] >= 20;
    }

    public override void ModifyDisplayNode(
        Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        if (node == null || node.gameObject == null)
            return;

        // The Rocket Storm display contains more than one mesh renderer.
        // SetMeshTexture() changes the first SkinnedMeshRenderer, which
        // recolors the monkey but leaves the separate gun mesh unchanged.
        //
        // Apply the same gold/white atlas directly to every mesh renderer
        // in the Rocket Storm display so both the monkey AND gun use it.
        var goldWhiteTexture =
            GetTexture("RYNO_RocketStorm_GoldWhite");

        if (goldWhiteTexture == null)
            return;

        var renderers =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.Renderer>(true);

        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;

            var material = renderer.material;

            if (material == null)
                continue;

            // Standard Unity shader texture property.
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture(
                    "_MainTex",
                    goldWhiteTexture
                );
            }

            // URP-compatible texture property.
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture(
                    "_BaseMap",
                    goldWhiteTexture
                );
            }

            // Also update Unity's mainTexture reference.
            material.mainTexture = goldWhiteTexture;
        }
    }
}



// ============================================================
// HYDRA EXPLOSION DISPLAY
// ============================================================
// Hydra's missile explosion is a separate projectile display from
// the rocket itself. These displays reuse that exact vanilla Hydra
// explosion display, then tint its renderers/particles.
//
// Level 10-19 = blue explosion.
// Level 20     = gold explosion.

public abstract class RYNOHydraExplosionDisplay : ModDisplay
{
    public override PrefabReference BaseDisplayReference
    {
        get
        {
            var hydraTower =
                Game.instance.model.GetTowerFromId("DartlingGunner-030");

            if (hydraTower == null)
                return default!;

            var hydraAttack =
                hydraTower.GetAttackModel();

            if (hydraAttack == null ||
                hydraAttack.weapons == null ||
                hydraAttack.weapons.Length == 0 ||
                hydraAttack.weapons[0] == null ||
                hydraAttack.weapons[0].projectile == null)
                return default!;

            var rocket =
                hydraAttack.weapons[0].projectile;

            foreach (var behavior in rocket.behaviors)
            {
                if (behavior == null)
                    continue;

                var typeName = behavior.GetIl2CppType().Name;

                if (typeName == "CreateProjectileOnExhaustPierceModel")
                {
                    var exhaust =
                        behavior.Cast<CreateProjectileOnExhaustPierceModel>();

                    if (exhaust.projectile != null)
                        return exhaust.projectile.display;
                }

                if (typeName == "CreateProjectileOnBlockerCollideModel")
                {
                    var blocker =
                        behavior.Cast<CreateProjectileOnBlockerCollideModel>();

                    if (blocker.projectile != null)
                        return blocker.projectile.display;
                }
            }

            return default!;
        }
    }

    protected abstract UnityEngine.Color Tint { get; }

    public override void ModifyDisplayNode(
        Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        if (node == null || node.gameObject == null)
            return;

        var tint = Tint;

        // Tint particle systems used by the Hydra explosion.
        var particles =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.ParticleSystem>(true);

        foreach (var particle in particles)
        {
            if (particle == null)
                continue;

            particle.startColor = tint;
        }

        // Tint sprite-based explosion elements.
        var sprites =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.SpriteRenderer>(true);

        foreach (var sprite in sprites)
        {
            if (sprite == null)
                continue;

            sprite.color = tint;
        }

        // Tint any mesh/particle renderers whose material exposes a
        // standard color property. This catches Hydra's glowing
        // explosion elements that are not SpriteRenderers.
        var renderers =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.Renderer>(true);

        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;

            var material = renderer.material;

            if (material == null)
                continue;

            if (material.HasProperty("_Color"))
                material.SetColor("_Color", tint);

            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", tint);

            if (material.HasProperty("_TintColor"))
                material.SetColor("_TintColor", tint);
        }
    }
}

public class RYNOHydraExplosionBlueDisplay : RYNOHydraExplosionDisplay
{
    protected override UnityEngine.Color Tint =>
        new UnityEngine.Color(38f / 255f, 89f / 255f, 157f / 255f, 1f);
}

public class RYNOHydraExplosionGoldDisplay : RYNOHydraExplosionDisplay
{
    protected override UnityEngine.Color Tint =>
        new UnityEngine.Color(1f, 0.78f, 0.12f, 1f);
}


// ============================================================
// HYDRA ROCKET DISPLAY
// ============================================================
// Exact vanilla Hydra Rocket display:
// 28b2c3ab5c19cb34f867f1423666ef8c
//
// The vanilla Hydra rocket is a SpriteRenderer using a 58x160
// Texture2D. The recolored R.Y.N.O. rocket texture is embedded
// in the mod as:
//
// HYDRA_ROCKET_BLUE.png
//
// Add HYDRA_ROCKET_BLUE.png to the project with:
// Build Action = Embedded Resource
//
// This code no longer searches the scene, extracts textures, or
// writes diagnostic files. It simply replaces the vanilla rocket
// Sprite with the new embedded blue version.
public class RYNOHydraRocketDisplay : ModDisplay
{
    public override string BaseDisplay =>
        "28b2c3ab5c19cb34f867f1423666ef8c";

    public override void ModifyDisplayNode(
        Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        if (node == null || node.gameObject == null)
            return;

        var blueTexture =
            GetTexture("HYDRA_ROCKET_BLUE");

        if (blueTexture == null)
            return;

        var spriteRenderers =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.SpriteRenderer>(true);

        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer == null)
                continue;

            var originalSprite =
                spriteRenderer.sprite;

            if (originalSprite == null)
                continue;

            // Preserve the original Hydra rocket's pivot and
            // pixels-per-unit so the new sprite sits exactly where
            // the vanilla rocket did.
            var pivot =
                new UnityEngine.Vector2(
                    originalSprite.pivot.x / originalSprite.rect.width,
                    originalSprite.pivot.y / originalSprite.rect.height
                );

            var newSprite =
                UnityEngine.Sprite.Create(
                    blueTexture,
                    new UnityEngine.Rect(
                        0f,
                        0f,
                        blueTexture.width,
                        blueTexture.height
                    ),
                    pivot,
                    originalSprite.pixelsPerUnit
                );

            if (newSprite == null)
                continue;

            newSprite.name =
                "RYNO_HydraRocketPod_Blue";

            spriteRenderer.sprite =
                newSprite;
        }

        // --------------------------------------------------------
        // HYDRA ROCKET TRAIL
        // --------------------------------------------------------
        // The vanilla Hydra rocket uses a TrailRenderer rather than
        // the rocket sprite texture for its exhaust trail. The trail
        // therefore stayed green when we replaced the sprite.
        //
        // Use a blue matching the recolored Hydra rocket/model.
        var trails =
            node.gameObject.GetComponentsInChildren<
                UnityEngine.TrailRenderer>(true);

        var trailStart =
            new UnityEngine.Color(
                38f / 255f,
                89f / 255f,
                157f / 255f,
                0.85f
            );

        var trailEnd =
            new UnityEngine.Color(
                38f / 255f,
                89f / 255f,
                157f / 255f,
                0f
            );

        foreach (var trail in trails)
        {
            if (trail == null)
                continue;

            trail.startColor = trailStart;
            trail.endColor = trailEnd;

            // The Hydra trail also uses a material that can retain the
            // vanilla green color, so override the material color too.
            var material = trail.material;

            if (material != null)
            {
                if (material.HasProperty("_Color"))
                    material.SetColor("_Color", trailStart);

                if (material.HasProperty("_BaseColor"))
                    material.SetColor("_BaseColor", trailStart);

                material.color = trailStart;
            }
        }
    }
}


// ============================================================
// R.Y.N.O. AURA SUPPORT
// ============================================================
// Uses vanilla support model types so the Level 17-19 auras behave
// like native BTD6 tower buffs and automatically respect R.Y.N.O.'s
// current range.
public static class RYNOAuraSupport
{
    private static TowerModel GetTower(string id)
    {
        return Game.instance.model.GetTowerFromId(id);
    }

    public static void AddRateAura(
        TowerModel towerModel,
        float multiplier,
        string id)
    {
        var source = GetTower("MonkeyVillage-020");
        var sourceBehavior = source?.GetBehavior<RateSupportModel>();

        if (sourceBehavior == null)
            return;

        var aura = sourceBehavior.Duplicate();
        aura.name = id;
        aura.mutatorId = id;
        aura.multiplier = multiplier;
        aura.isUnique = true;
        aura.isGlobal = false;
        ((SupportModel)aura).filters =
            new Il2CppReferenceArray<TowerFilterModel>(0L);

        towerModel.AddBehavior(aura);
    }

    public static void AddDamageAura(
        TowerModel towerModel,
        float increase,
        string id)
    {
        var source = GetTower("MonkeyVillage-040");
        var sourceBehavior = source?.GetBehavior<DamageSupportModel>();

        if (sourceBehavior == null)
            return;

        var aura = sourceBehavior.Duplicate();
        aura.name = id;
        aura.mutatorId = id;
        aura.increase = increase;
        aura.isUnique = true;
        aura.isGlobal = false;
        ((SupportModel)aura).filters =
            new Il2CppReferenceArray<TowerFilterModel>(0L);

        towerModel.AddBehavior(aura);
    }

    public static void AddPierceAura(
        TowerModel towerModel,
        float increase,
        string id)
    {
        var source = GetTower("MonkeyVillage-040");
        var sourceBehavior = source?.GetBehavior<PierceSupportModel>();

        if (sourceBehavior == null)
            return;

        var aura = sourceBehavior.Duplicate();
        aura.name = id;
        aura.mutatorId = id;
        aura.pierce = increase;
        aura.isUnique = true;
        aura.isGlobal = false;
        ((SupportModel)aura).filters =
            new Il2CppReferenceArray<TowerFilterModel>(0L);

        towerModel.AddBehavior(aura);
    }

    public static void AddCashAura(
        TowerModel towerModel,
        float multiplier,
        string id)
    {
        var source = GetTower("MonkeyVillage-004");
        var sourceBehavior =
            source?.GetBehavior<MonkeyCityIncomeSupportModel>();

        if (sourceBehavior == null)
            return;

        var aura = sourceBehavior.Duplicate();
        aura.name = id;
        aura.uniqueMutatorId = id;
        aura.incomeModifier = multiplier;
        aura.isGlobal = false;
        ((SupportModel)aura).filters =
            new Il2CppReferenceArray<TowerFilterModel>(0L);

        towerModel.AddBehavior(aura);
    }


}


// ============================================================
// TARGETING ROTATION FIX
// ============================================================
// R.Y.N.O. keeps the original RotateToPointer behavior for
// Normal/Locked targeting, but suppresses it when a standard
// target-priority mode is selected so RotateToTarget can control
// the barrel without the two rotations fighting each other.

public static class RYNO_TargetingFix
{
    [HarmonyPatch(typeof(RotateToPointer), "SetRotation")]
    internal static class RotateToPointer_SetRotation
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotateToPointer __instance)
        {
            var attack = ((AttackBehavior)__instance).attack;

            TargetSupplier targetSupplier = default!;

            if (!Il2CppSystemObjectExt.Is<TargetSupplier>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)attack.activeTargetSupplier!,
                    out targetSupplier))
            {
                return true;
            }

            if (!AttackBehaviorExt.HasAttackBehavior<RotateToPointer>(attack) ||
                !AttackBehaviorExt.HasAttackBehavior<RotateToTarget>(attack))
            {
                return true;
            }

            bool priorityTargeting =
                Il2CppSystemObjectExt.Is<TargetFirst>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetLast>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetClose>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetStrong>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier);

            return !priorityTargeting;
        }
    }


    [HarmonyPatch(typeof(RotateToTarget), "ApplyRotation")]
    internal static class RotateToTarget_ApplyRotation
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotateToTarget __instance)
        {
            var attack = ((AttackBehavior)__instance).attack;

            TargetSupplier targetSupplier = default!;

            if (!Il2CppSystemObjectExt.Is<TargetSupplier>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)attack.activeTargetSupplier!,
                    out targetSupplier))
            {
                return true;
            }

            if (!AttackBehaviorExt.HasAttackBehavior<RotateToPointer>(attack) ||
                !AttackBehaviorExt.HasAttackBehavior<RotateToTarget>(attack))
            {
                return true;
            }

            return
                Il2CppSystemObjectExt.Is<TargetFirst>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetLast>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetClose>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetStrong>(
                    (Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier);
        }
    }
}