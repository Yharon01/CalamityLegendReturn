using CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Passive
{
    internal sealed class AethersWhisperPassiveCannon : ModProjectile
    {
        public override string Texture => "CalamityMod/Items/Weapons/Magic/AethersWhisper";
        public override void SetDefaults() { Projectile.width = 42; Projectile.height = 22; Projectile.tileCollide = false; Projectile.ignoreWater = true; Projectile.timeLeft = 2; }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            Vector2 aim = (Main.MouseWorld - owner.MountedCenter).SafeNormalize(Vector2.UnitX * owner.direction);
            Projectile.localAI[0]++;
            // Omicron 式上下交替，但改为玩家背后的粉紫色短弧，而非原武器的侧翼复刻。
            float wave = (float)System.Math.Sin(Projectile.localAI[0] * 0.075f + Projectile.ai[0] * MathHelper.PiOver2) * 30f;
            Vector2 side = aim.RotatedBy(MathHelper.PiOver2) * Projectile.ai[0] * 66f;
            Vector2 desired = owner.MountedCenter - aim * 54f + side + new Vector2(0f, -46f + wave);
            Projectile.Center = Vector2.Lerp(Projectile.Center, desired, 0.16f);
            Projectile.rotation = Projectile.AngleTo(Main.MouseWorld); Projectile.spriteDirection = aim.X >= 0 ? 1 : -1; Projectile.timeLeft = 2;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            SpriteEffects fx = Projectile.spriteDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, AethersWhisperVisuals.AetherPurple * 0.8f, Projectile.rotation, texture.Size() * .5f, .42f, fx, 0);
            return false;
        }
    }
}
