using System;
using CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Passive
{
    // 背包收藏或主手时的常驻成长；不依赖旧持枪弹幕，因此切武器不会遗留攻击状态。
    internal sealed class AethersWhisperPassivePlayer : ModPlayer
    {
        private bool active;
        private bool held;
        private int idleLaserTimer;

        public override void PostUpdateEquips()
        {
            active = HasLockedOrHeld(out held);
            if (!active || Player.dead) return;
            if (held && AethersWhisperProgression.FlightTimeBoost && Player.wingTimeMax > 0)
                Player.wingTimeMax = (int)(Player.wingTimeMax * 1.5f);
            if (Player.wingTimeMax > 0)
                Player.wingAccRunSpeed *= 1.3f;
            else
                Player.GetJumpState(ExtraJump.SandstormInABottle).Enable();
        }

        public override void FrameEffects()
        {
            // 浮游炮是背部轮廓的替代，不让翅膀从其间穿出；只处理视觉槽位，不移除飞行属性。
            if (!Player.dead && HasLockedOrHeld(out _))
            {
                Player.wings = 0;
                Player.cWings = 0;
            }
        }

        public override void PostUpdate()
        {
            if (!active || Player.dead) { idleLaserTimer = 0; return; }
            if (Main.myPlayer == Player.whoAmI)
                EnsureCannons();

            if (held || !AethersWhisperProgression.PassiveIdleCannonLaser || ++idleLaserTimer < 300 || Main.myPlayer != Player.whoAmI)
                return;
            idleLaserTimer = 0;
            Vector2 aim = (Main.MouseWorld - Player.MountedCenter).SafeNormalize(Vector2.UnitX * Player.direction);
            Item weapon = FindItem();
            int damage = weapon == null ? 1 : Math.Max(1, (int)(Player.GetWeaponDamage(weapon) * 0.15f));
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter, aim * 18f,
                ModContent.ProjectileType<AethersWhisperAttackProjectile>(), damage, 0f, Player.whoAmI,
                AethersWhisperAttackProjectile.DelayedMainBeam, 0f);
        }

        private void EnsureCannons()
        {
            int type = ModContent.ProjectileType<AethersWhisperPassiveCannon>();
            if (Player.ownedProjectileCounts[type] >= 2) return;
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, type, 0, 0f, Player.whoAmI, -1f);
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, type, 0, 0f, Player.whoAmI, 1f);
        }

        private bool HasLockedOrHeld(out bool isHeld)
        {
            isHeld = Player.HeldItem.type == ModContent.ItemType<AethersWhisper>();
            if (isHeld) return true;
            foreach (Item item in Player.inventory)
                if (item.type == ModContent.ItemType<AethersWhisper>() && item.favorited) return true;
            return false;
        }
        private Item FindItem()
        {
            if (Player.HeldItem.type == ModContent.ItemType<AethersWhisper>()) return Player.HeldItem;
            foreach (Item item in Player.inventory) if (item.type == ModContent.ItemType<AethersWhisper>() && item.favorited) return item;
            return null;
        }
    }
}
