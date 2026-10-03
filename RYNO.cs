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
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using MelonLoader;

[assembly: MelonInfo(typeof(RYNO.RYNO_Mod), "RYNO", "1.0.0", "HopeZEXAL")]
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
    public override string Level1Description => "R.Y.N.O. fires a powerful minigun at Bloons.";
    public override int MaxLevel => 20;
    public override float XpRatio => 0.40f;
    public override string NameStyle => TowerType.Silas;
    public override string BackgroundStyle => TowerType.Silas;
    public override string GlowStyle => TowerType.Silas;

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        towerModel.SetDisplay<RYNODisplay>();
        var attack = towerModel.GetAttackModel();
        if (attack?.weapons == null || attack.weapons.Length == 0 || attack.weapons[0] == null)
            return;
        if (attack.weapons[0].emission is RandomEmissionModel emission)
            emission.angle = 7f;
    }
}

public class RYNODisplay : ModDisplay
{
    public override string BaseDisplay => GetDisplay(TowerType.DartlingGunner, 0, 0, 0);

    public override void ModifyDisplayNode(Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        SetMeshTexture(node, Name);
    }
}

// ============================================================
// LEVEL 2 - +2 damage, +2 pierce
// ============================================================
public class RYNOLevel2 : ModHeroLevel<RYNO>
{
    public override int Level => 2;
    public override string Description => "+2 damage and +2 pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        projectile.pierce += 2;
        var damage = projectile.GetBehavior<DamageModel>();
        if (damage != null) damage.damage += 2;
    }
}

// ============================================================
// LEVEL 3 - +20% fire rate
// ============================================================
public class RYNOLevel3 : ModHeroLevel<RYNO>
{
    public override int Level => 3;
    public override string Description => "+20% attack speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.GetAttackModel().weapons[0].rate *= 0.8f;
    }
}

// ============================================================
// LEVEL 4 - Lead + Camo
// ============================================================
public class RYNOLevel4 : ModHeroLevel<RYNO>
{
    public override int Level => 4;
    public override string Description => "Can pop Lead and detect Camo Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        var damage = projectile.GetBehavior<DamageModel>();
        if (damage != null)
            damage.immuneBloonProperties &= ~BloonProperties.Lead;

        towerModel.AddBehavior(new OverrideCamoDetectionModel("RYNO_CamoDetection", true));
    }
}

// ============================================================
// LEVEL 5 - Incendiary Rounds: 4 damage / 0.75 interval
// ============================================================
public class RYNOLevel5 : ModHeroLevel<RYNO>
{
    public override int Level => 5;
    public override string Description => "Incendiary rounds burn Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        AddHotShotBurn(towerModel, 4f, 0.75f, "Dot:RYNO");
    }

    internal static void AddHotShotBurn(TowerModel towerModel, float damage, float interval, string mutationId)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        var hotShotTower = Game.instance.model.GetTowerFromId("MonkeyBuccaneer-020");
        var hotShotAttack = hotShotTower?.GetAttackModel();
        if (hotShotAttack?.weapons == null || hotShotAttack.weapons.Length <= 2 || hotShotAttack.weapons[2]?.projectile == null)
            return;

        var hotShotBurn = hotShotAttack.weapons[2].projectile.GetBehavior<AddBehaviorToBloonModel>();
        if (hotShotBurn == null)
            return;

        var burn = hotShotBurn.Duplicate();
        burn.mutationId = mutationId;
        foreach (var dot in AddBehaviorToBloonModelBehaviorExt.GetBehaviors<DamageOverTimeModel>(burn))
        {
            dot.damage = damage;
            dot.interval = interval;
        }
        projectile.AddBehavior(burn);
    }
}

// ============================================================
// LEVEL 6 - +2 bonus vs Fortified, Ceramic, MOAB-Class
// ============================================================
public class RYNOLevel6 : ModHeroLevel<RYNO>
{
    public override int Level => 6;
    public override string Description => "+2 bonus damage against Fortified, Ceramic, and MOAB-Class Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var p = towerModel.GetAttackModel().weapons[0].projectile;
        AddBonus(p, "Fortified", "RYNO_L6_Fortified", 2f);
        AddBonus(p, "Ceramic", "RYNO_L6_Ceramic", 2f);
        AddBonus(p, "Moab", "RYNO_L6_Moab", 2f);
        p.hasDamageModifiers = true;
    }

    internal static void AddBonus(ProjectileModel projectile, string tag, string id, float bonus)
    {
        projectile.AddBehavior(new DamageModifierForTagModel(id, tag, 1f, bonus, false, false)
        {
            tags = new[] { tag },
            collisionPass = 0
        });
    }
}

// ============================================================
// LEVEL 7 - range +125%; aura: +50% rate, +2 damage, +2 pierce,
//          +50% cash generation
// ============================================================
public class RYNOLevel7 : ModHeroLevel<RYNO>
{
    public override int Level => 7;
    public override string Description => "+50% damage, +50% pierce, and +50% cash generation.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        var damage = projectile.GetBehavior<DamageModel>();
        var dartling = Game.instance.model.GetTowerFromId("DartlingGunner");
        var baseProjectile = dartling?.GetAttackModel()?.weapons?[0]?.projectile;
        if (baseProjectile != null)
        {
            projectile.pierce += baseProjectile.pierce * 0.5f;
            var baseDamage = baseProjectile.GetBehavior<DamageModel>();
            if (damage != null && baseDamage != null)
                damage.damage += baseDamage.damage * 0.5f;
        }

        var cash = towerModel.GetBehavior<CashIncreaseModel>();
        if (cash != null) cash.multiplier = 1.5f;
        else towerModel.AddBehavior(new CashIncreaseModel("RYNO_CashIncrease", 0f, 1.5f));
    }
}

// ============================================================
// LEVEL 8 - +3 damage, +3 pierce, +15% fire rate
// ============================================================
public class RYNOLevel8 : ModHeroLevel<RYNO>
{
    public override int Level => 8;
    public override string Description => "+3 damage, +3 pierce, and +15% attack speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attack = towerModel.GetAttackModel();
        attack.weapons[0].rate *= 1f / 1.15f;
        var p = attack.weapons[0].projectile;
        p.pierce += 3;
        var d = p.GetBehavior<DamageModel>();
        if (d != null) d.damage += 3;
    }
}

// ============================================================
// LEVEL 9 - +3 damage
// ============================================================
public class RYNOLevel9 : ModHeroLevel<RYNO>
{
    public override int Level => 9;
    public override string Description => "+3 damage.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var d = towerModel.GetAttackModel().weapons[0].projectile.GetBehavior<DamageModel>();
        if (d != null) d.damage += 3;
    }
}

// ============================================================
// LEVEL 10 - advanced targeting only
// ============================================================
public class RYNOLevel10 : ModHeroLevel<RYNO>
{
    public override int Level => 10;
    public override string Description => "Gains First, Last, Close, and Strong targeting options.";
    public override string Portrait => "RYNOLevel10-Portrait";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attack = towerModel.GetAttackModel();
        attack.AddBehavior(new TargetFirstModel("RYNO_TargetFirst", true, false));
        attack.AddBehavior(new TargetLastModel("RYNO_TargetLast", true, false));
        attack.AddBehavior(new TargetCloseModel("RYNO_TargetClose", true, false));
        attack.AddBehavior(new TargetStrongModel("RYNO_TargetStrong", true, false));

        var pointer = attack.GetBehavior<RotateToPointerModel>();
        if (pointer != null)
        {
            attack.AddBehavior(new RotateToTargetModel(
                "RYNO_RotateToTarget",
                pointer.rotateOnlyOnEmit,
                pointer.rotateTower,
                true,
                1,
                true,
                true));
        }
        towerModel.UpdateTargetProviders();
    }
}

// ============================================================
// LEVEL 11 - +2 bonus vs Fortified, Ceramic, MOAB-Class
// ============================================================
public class RYNOLevel11 : ModHeroLevel<RYNO>
{
    public override int Level => 11;
    public override string Description => "+2 additional bonus damage against Fortified, Ceramic, and MOAB-Class Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var p = towerModel.GetAttackModel().weapons[0].projectile;
        RYNOLevel6.AddBonus(p, "Fortified", "RYNO_L11_Fortified", 2f);
        RYNOLevel6.AddBonus(p, "Ceramic", "RYNO_L11_Ceramic", 2f);
        RYNOLevel6.AddBonus(p, "Moab", "RYNO_L11_Moab", 2f);
        p.hasDamageModifiers = true;
    }
}

// ============================================================
// LEVEL 12 - +20% fire rate
// ============================================================
public class RYNOLevel12 : ModHeroLevel<RYNO>
{
    public override int Level => 12;
    public override string Description => "+20% attack speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.GetAttackModel().weapons[0].rate *= 0.8f;
    }
}

// ============================================================
// LEVEL 13 - double Level 5 burn: 8 damage / 0.375 interval
// ============================================================
public class RYNOLevel13 : ModHeroLevel<RYNO>
{
    public override int Level => 13;
    public override string Description => "Incendiary rounds burn for 8 damage every 0.375 seconds.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        RYNOLevel5.AddHotShotBurn(towerModel, 8f, 0.375f, "Dot:RYNO_L13");
    }
}

// ============================================================
// LEVEL 14 - +5 MOAB-Class bonus
// ============================================================
public class RYNOLevel14 : ModHeroLevel<RYNO>
{
    public override int Level => 14;
    public override string Description => "+5 bonus damage against MOAB-Class Bloons.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var p = towerModel.GetAttackModel().weapons[0].projectile;
        RYNOLevel6.AddBonus(p, "Moab", "RYNO_L14_Moab", 5f);
        p.hasDamageModifiers = true;
    }
}

// ============================================================
// LEVEL 15 - +5 damage, +5 pierce, +15% fire rate
// ============================================================
public class RYNOLevel15 : ModHeroLevel<RYNO>
{
    public override int Level => 15;
    public override string Description => "+5 damage, +5 pierce, and +15% attack speed.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attack = towerModel.GetAttackModel();
        attack.weapons[0].rate *= 1f / 1.15f;
        var p = attack.weapons[0].projectile;
        p.pierce += 5;
        var d = p.GetBehavior<DamageModel>();
        if (d != null) d.damage += 5;
    }
}

// ============================================================
// LEVEL 16 - +3 pierce
// ============================================================
public class RYNOLevel16 : ModHeroLevel<RYNO>
{
    public override int Level => 16;
    public override string Description => "+3 pierce.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        towerModel.GetAttackModel().weapons[0].projectile.pierce += 3;
    }
}

// ============================================================
// LEVEL 17 - can damage all Bloon types
// ============================================================
public class RYNOLevel17 : ModHeroLevel<RYNO>
{
    public override int Level => 17;
    public override string Description => "Can damage all Bloon types.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var d = towerModel.GetAttackModel().weapons[0].projectile.GetBehavior<DamageModel>();
        if (d != null)
        {
            d.immuneBloonProperties = BloonProperties.None;
            d.immuneBloonPropertiesOriginal = BloonProperties.None;
        }
    }
}

// ============================================================
// LEVEL 18 - 10% crit chance, 10x damage
// ============================================================
public class RYNOLevel18 : ModHeroLevel<RYNO>
{
    public override int Level => 18;
    public override string Description => "Critical Rounds: 10% chance to deal 10x damage.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var superMonkey = Game.instance.model.GetTowerFromId("SuperMonkey-030");
        var superAttack = superMonkey?.GetAttackModel();
        var sourceProjectile = superAttack?.weapons?[0]?.projectile;
        var sourceCrit = sourceProjectile?.GetBehavior<CritMultiplierModel>();
        if (sourceCrit == null)
            return;

        var crit = sourceCrit.Duplicate();
        crit.damage = 10f;
        crit.lower = 0;
        crit.upper = 10;
        towerModel.GetAttackModel().weapons[0].projectile.AddBehavior(crit);
    }
}

// ============================================================
// LEVEL 19 - final range 250% original; nearby towers +100% rate
//          and +100% cash generation
// ============================================================
public class RYNOLevel19 : ModHeroLevel<RYNO>
{
    public override int Level => 19;
    public override string Description => "+100% total damage, +100% total pierce, and +100% cash generation.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var projectile = towerModel.GetAttackModel().weapons[0].projectile;
        var damage = projectile.GetBehavior<DamageModel>();
        var dartling = Game.instance.model.GetTowerFromId("DartlingGunner");
        var baseProjectile = dartling?.GetAttackModel()?.weapons?[0]?.projectile;
        if (baseProjectile != null)
        {
            projectile.pierce += baseProjectile.pierce * 0.5f;
            var baseDamage = baseProjectile.GetBehavior<DamageModel>();
            if (damage != null && baseDamage != null)
                damage.damage += baseDamage.damage * 0.5f;
        }

        var cash = towerModel.GetBehavior<CashIncreaseModel>();
        if (cash != null) cash.multiplier = 2f;
        else towerModel.AddBehavior(new CashIncreaseModel("RYNO_CashIncrease", 0f, 2f));
    }
}

// ============================================================
// LEVEL 20 - Adora bolts, 3-shot 40-degree spread
// ============================================================
public class RYNOLevel20 : ModHeroLevel<RYNO>
{
    public override int Level => 20;
    public override string Portrait => "RYNOLevel20-Portrait";
    public override string Description => "Fires three Adora bolts in a 40-degree spread with a custom R.Y.N.O. texture.";

    public override void ApplyUpgrade(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();
        if (attackModel == null || attackModel.weapons == null || attackModel.weapons.Length == 0 || attackModel.weapons[0] == null) return;
        var weapon = attackModel.weapons[0];
        var oldProjectile = weapon.projectile;
        // Lock the final primary attack to exactly 0.1s = 10 shots/sec.
        weapon.rate = 0.1f;

        var adoraTower = Game.instance.model.GetTowerFromId("Adora");
        if (adoraTower == null) return;
        var adoraAttack = adoraTower.GetAttackModel();
        if (adoraAttack == null || adoraAttack.weapons == null || adoraAttack.weapons.Length == 0 || adoraAttack.weapons[0]?.projectile == null) return;

        var adoraProjectile = adoraAttack.weapons[0].projectile.Duplicate();
        var adoraEmission = adoraAttack.weapons[0].emission;
        if (adoraEmission != null)
        {
            var emission = adoraEmission.Duplicate().Cast<AdoraEmissionModel>();
            emission.count = 3;
            emission.angleBetween = 40f;
            weapon.emission = emission;
        }

        // Keep the exact accumulated R.Y.N.O. base damage and pierce at Level 20.
        var oldDamage = oldProjectile?.GetBehavior<DamageModel>();
        var newDamage = adoraProjectile.GetBehavior<DamageModel>();
        if (oldDamage != null && newDamage != null)
        {
            newDamage.damage = oldDamage.damage;
            newDamage.immuneBloonProperties = BloonProperties.None;
            newDamage.immuneBloonPropertiesOriginal = BloonProperties.None;
        }
        if (oldProjectile != null) adoraProjectile.pierce = oldProjectile.pierce;

        // Preserve R.Y.N.O. class bonuses and burn effects.
        if (oldProjectile != null)
        {
            foreach (var behavior in oldProjectile.behaviors)
            {
                if (behavior == null) continue;
                var typeName = behavior.GetIl2CppType().Name;
                if (typeName == "DamageModifierForTagModel" || typeName == "AddBehaviorToBloonModel")
                {
                    var copied = behavior.Duplicate();
                    if (typeName == "AddBehaviorToBloonModel") copied.Cast<AddBehaviorToBloonModel>().mutationId = "Dot:RYNO_L20";
                    adoraProjectile.AddBehavior(copied);
                }
            }
            adoraProjectile.hasDamageModifiers = oldProjectile.hasDamageModifiers;
            var oldCrit = oldProjectile.GetBehavior<CritMultiplierModel>();
            if (oldCrit != null)
            {
                var rynoCrit = oldCrit.Duplicate();
                rynoCrit.lower = 0; rynoCrit.upper = 20; rynoCrit.damage = 10f;
                adoraProjectile.AddBehavior(rynoCrit);
            }
        }

        var crit = adoraProjectile.GetBehavior<CritMultiplierModel>();
        if (crit != null) { crit.lower = 0; crit.upper = 20; crit.damage = 10f; }
        adoraProjectile.ApplyDisplay<RYNOAdoraProjectileDisplay>();
        weapon.projectile = adoraProjectile;
        towerModel.SetDisplay<RYNOHydraDisplay>();
    }
}

// ============================================================
// HYDRA TOWER DISPLAY
// ============================================================
public class RYNOHydraDisplay : ModTowerDisplay<RYNO>
{
    public override string BaseDisplay => "028a7597e522bb14688c8e2fc762f9b7";

    public override bool UseForTower(int[] tiers)
    {
        return tiers != null && tiers.Length > 0 && tiers[0] >= 20;
    }

    public override void ModifyDisplayNode(Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        SetMeshTexture(node, "RYNO_Hydra_BlackBlue");
    }
}

// ============================================================
// ADORA PROJECTILE DISPLAY
// ============================================================
public class RYNOAdoraProjectileDisplay : ModDisplay
{
    public override string BaseDisplay => "dfc16ec49f4894148bce0161ebb0bd32";

    public override void ModifyDisplayNode(Il2CppAssets.Scripts.Unity.Display.UnityDisplayNode node)
    {
        if (node == null || node.gameObject == null) return;
        var blueTexture = GetTexture("RYNO_AdoraProjectile_Blue");
        if (blueTexture == null) return;
        var sprites = node.gameObject.GetComponentsInChildren<UnityEngine.SpriteRenderer>(true);
        foreach (var sr in sprites)
        {
            if (sr == null || sr.sprite == null) continue;
            var old = sr.sprite;
            var pivot = new UnityEngine.Vector2(old.pivot.x / old.rect.width, old.pivot.y / old.rect.height);
            var sprite = UnityEngine.Sprite.Create(blueTexture, new UnityEngine.Rect(0f, 0f, blueTexture.width, blueTexture.height), pivot, old.pixelsPerUnit);
            if (sprite != null) { sprite.name = "RYNO_AdoraProjectile_Blue"; sr.sprite = sprite; }
        }
        var trailStart = new UnityEngine.Color(38f / 255f, 89f / 255f, 157f / 255f, 0.85f);
        var trailEnd = new UnityEngine.Color(38f / 255f, 89f / 255f, 157f / 255f, 0f);
        foreach (var trail in node.gameObject.GetComponentsInChildren<UnityEngine.TrailRenderer>(true))
        {
            if (trail == null) continue;
            trail.startColor = trailStart; trail.endColor = trailEnd;
            var material = trail.material;
            if (material != null)
            {
                if (material.HasProperty("_Color")) material.SetColor("_Color", trailStart);
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", trailStart);
                material.color = trailStart;
            }
        }
    }
}

// ============================================================
// TARGETING FIX
// ============================================================
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
                    out targetSupplier)) return true;

            if (!AttackBehaviorExt.HasAttackBehavior<RotateToPointer>(attack) ||
                !AttackBehaviorExt.HasAttackBehavior<RotateToTarget>(attack)) return true;

            bool priority =
                Il2CppSystemObjectExt.Is<TargetFirst>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetLast>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetClose>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetStrong>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier);

            return !priority;
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
                    out targetSupplier)) return true;

            if (!AttackBehaviorExt.HasAttackBehavior<RotateToPointer>(attack) ||
                !AttackBehaviorExt.HasAttackBehavior<RotateToTarget>(attack)) return true;

            return
                Il2CppSystemObjectExt.Is<TargetFirst>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetLast>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetClose>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier) ||
                Il2CppSystemObjectExt.Is<TargetStrong>((Il2CppInterop.Runtime.InteropTypes.Il2CppObjectBase)(object)targetSupplier);
        }
    }
}
