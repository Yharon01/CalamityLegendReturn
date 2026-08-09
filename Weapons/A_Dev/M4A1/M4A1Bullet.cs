using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.Weapons.A_Dev.M4A1
{
    /// <summary>
    /// 左键特殊子弹：短促、低密度的荧光绿曳光弹。它是实体步枪火力的延伸，
    /// 而不是在高速连射中堆叠成一串能量光球。
    /// 命中提升战术同步率并累积伸冤者印记；一层伤害 / 二层破甲。
    /// </summary>
    public class M4A1Bullet : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            if (Projectile.velocity != Vector2.Zero)
                Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.18f, 0.5f, 0.12f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int marks = M4A1MarkGlobalNPC.Of(target).MarkLevel;
            if (marks >= 1)
                modifiers.SourceDamage *= 1f + BalanceM4A1.Mark1DamageBonus;
            if (marks >= 2)
                modifiers.ArmorPenetration += BalanceM4A1.Mark2ArmorPen;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Player owner = Main.player[Projectile.owner];
                bool isBoss = target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type];
                M4A1Player.Get(owner).GainSync(isBoss, hit.Crit);
                M4A1MarkGlobalNPC.RegisterHit(target, owner, damageDone);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            float rot = Projectile.rotation;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            // 一条 14 像素的硬朗短曳光。长度不足以在连射时堆成雾带。
            Main.EntitySpriteDraw(pixel, pos - direction * 7f, null, (M4A1Visuals.NeonGreen with { A = 0 }) * 0.78f, rot, new Vector2(0f, 0.5f), new Vector2(14f, 1.15f), SpriteEffects.None, 0);
            // 只有弹头有极小亮核，用来确保高速移动时仍可辨识。
            Main.EntitySpriteDraw(bloom, pos, null, (M4A1Visuals.NeonGreenBright with { A = 0 }) * 0.58f, 0f, bloom.Size() * 0.5f, 0.055f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(pixel, pos, null, new Color(238, 255, 225, 0) * 0.9f, rot, new Vector2(0.5f), new Vector2(2.4f, 1.35f), SpriteEffects.None, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
