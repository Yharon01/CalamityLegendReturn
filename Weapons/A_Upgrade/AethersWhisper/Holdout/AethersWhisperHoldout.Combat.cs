using System;
using CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Projectiles;
using CalamityMod;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Holdout
{
    internal sealed partial class AethersWhisperHoldout
    {
        private void FireNextLeftAttack()
        {
            int mode = leftAttackIndex++ % AethersWhisperProgression.UnlockedLeftAttacks;
            if (!Owner.CheckMana(AethersWhisperBalance.LeftManaCost, true)) return;
            Vector2 aim = AimDirection;
            int damage = Owner.GetWeaponDamage(Owner.HeldItem);
            Vector2 tip = GetSafeMuzzle(aim);
            Vector2 side = aim.RotatedBy(MathHelper.PiOver2);
            switch (mode)
            {
                case AethersWhisperAttackProjectile.LiquidCross:
                    // 两侧浮游炮交叉液态弹：速度逐步提升并无限穿透。
                    SpawnAttack(Owner.MountedCenter + side * 74f, (aim + side * 0.28f).SafeNormalize(aim) * 13f, mode, damage);
                    SpawnAttack(Owner.MountedCenter - side * 74f, (aim - side * 0.28f).SafeNormalize(aim) * 13f, mode, damage);
                    break;
                case AethersWhisperAttackProjectile.Cyclone:
                    for (int i = -1; i <= 1; i++) SpawnAttack(tip + side * i * 18f, aim.RotatedBy(i * 0.18f) * 18f, mode, (int)(damage * 0.72f));
                    break;
                case AethersWhisperAttackProjectile.JellyLightning:
                    for (int cannon = -1; cannon <= 1; cannon += 2)
                    for (int shot = 0; shot < 2; shot++)
                        SpawnAttack(Owner.MountedCenter + side * cannon * 74f, aim.RotatedBy(cannon * (0.62f + shot * 0.12f)) * 10f + new Vector2(0f, -6f), mode, (int)(damage * 0.55f));
                    break;
                default:
                    SpawnAttack(tip, aim * AethersWhisperBalance.LeftAttackSpeed, mode, mode == AethersWhisperAttackProjectile.FinalBeam && AethersWhisperProgression.FinalityRift ? (int)(damage * 1.15f) : damage);
                    break;
            }
            recoilOffset = mode == 4 ? 20f : 7f;
            muzzleFlashTimer = mode == 4 ? 16 : 8;
            starPhaseKick += MathHelper.PiOver4;
            if (mode == 4) Owner.velocity -= aim * AethersWhisperBalance.FinalRecoil;
            SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.45f, Pitch = 0.15f + mode * 0.08f }, GunTip);
        }

        private void ReleaseRightCharge(int charge)
        {
            if (charge < AethersWhisperBalance.RightChargeTicks || !Owner.CheckMana(AethersWhisperBalance.RightManaCost, true)) return;
            Vector2 aim = AimDirection;
            Vector2 side = aim.RotatedBy(MathHelper.PiOver2);
            int groups = AethersWhisperProgression.IndependentEnergyBalls ? 2 : 1;
            int damage = (int)(Owner.GetWeaponDamage(Owner.HeldItem) * AethersWhisperBalance.RightOrbDamageMultiplier / groups);
            for (int group = 0; group < groups; group++)
            for (int cannon = -1; cannon <= 1; cannon += 2)
            {
                Vector2 origin = Owner.MountedCenter + side * cannon * 74f;
                Vector2 velocity = aim.RotatedBy(groups == 1 ? 0f : (group == 0 ? -0.38f : 0.38f)) * 7f + side * cannon * 1.8f;
                SpawnAttack(origin, velocity, AethersWhisperAttackProjectile.EnergyOrb, damage, group * 10 + (cannon == 1 ? 1 : 0));
            }
            rightFlashTimer = 12;
            recoilOffset = 11f;
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.55f, Pitch = 0.25f }, Owner.Center);
            if (AethersWhisperProgression.RightMainHandShot)
                SpawnAttack(GetSafeMuzzle(aim), aim * 22f, AethersWhisperAttackProjectile.DelayedMainBeam, (int)(damage * (AethersWhisperProgression.FinalityRift ? 1.55f : 1.35f)), 30);
        }

        private void SpawnAttack(Vector2 tip, Vector2 velocity, int mode, int damage, int variant = 0)
        {
            if (Main.myPlayer != Projectile.owner) return;
            int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), tip, velocity, ModContent.ProjectileType<AethersWhisperAttackProjectile>(), Math.Max(1, damage), AethersWhisperBalance.KnockBack, Projectile.owner, mode, variant);
            if (Main.projectile.IndexInRange(p)) { Main.projectile[p].CritChance = Owner.GetWeaponCrit(Owner.HeldItem); Main.projectile[p].netUpdate = true; }
        }
    }
}
