using System;
using System.IO;
using CalamityMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.Weapons.BlossomFlux.RightClick
{
    // The charged arrow is a controller: it rises, then calls down exactly four large
    // explosive arrows and one mouse-locked extra-large arrow at a fixed sixteen-frame cadence.
    internal sealed class BFBombardChargeArrow : ModProjectile
    {
        private const int FirstFallFrame = 16;
        private const int FallInterval = 16;
        private const int FallingArrowCount = 5;

        private Vector2 targetPoint;
        private int elapsedFrames;
        private int arrowsReleased;

        public override string Texture => "CalamityLegendsReturn/Weapons/BlossomFlux/RightClick/DBomb/BFArrow_DBomb";

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 42;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = FirstFallFrame + FallInterval * FallingArrowCount + 30;
        }

        public void Configure(Vector2 target, float upwardSpeed)
        {
            targetPoint = target;
            Projectile.velocity = -Vector2.UnitY * upwardSpeed * Main.player[Projectile.owner].gravDir;
            Projectile.netUpdate = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WriteVector2(targetPoint);
            writer.Write(elapsedFrames);
            writer.Write(arrowsReleased);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            targetPoint = reader.ReadVector2();
            elapsedFrames = reader.ReadInt32();
            arrowsReleased = reader.ReadInt32();
        }

        public override void AI()
        {
            elapsedFrames++;
            Projectile.velocity *= 0.985f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (arrowsReleased >= FallingArrowCount || elapsedFrames < FirstFallFrame || (elapsedFrames - FirstFallFrame) % FallInterval != 0)
                return;

            ReleaseFallingArrow(arrowsReleased == FallingArrowCount - 1);
            arrowsReleased++;
            Projectile.netUpdate = true;

            if (arrowsReleased >= FallingArrowCount)
                Projectile.Kill();
        }

        private void ReleaseFallingArrow(bool extraLarge)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            Vector2 target = targetPoint == Vector2.Zero ? Main.MouseWorld : targetPoint;
            Vector2 direction = (target - Projectile.Center).SafeNormalize(Vector2.UnitY * Main.player[Projectile.owner].gravDir);
            if (!extraLarge)
                direction = direction.RotatedBy(Main.rand.NextFloat(-MathHelper.Pi / 8f, MathHelper.Pi / 8f));

            int damage = Math.Max(1, (int)(Projectile.damage * (extraLarge ? 1.75f : 1f)));
            int index = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * (extraLarge ? 34f : 29f),
                ModContent.ProjectileType<BFBombardExplosiveArrow>(),
                damage,
                Projectile.knockBack,
                Projectile.owner,
                extraLarge ? 1f : 0f);

            if (index >= 0 && index < Main.maxProjectiles)
                Main.projectile[index].netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 224, 74, 0), Projectile.rotation,
                texture.Size() * 0.5f, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            return false;
        }
    }

    // A pure yellow, deliberately thick impact arrow. It can strike three targets and creates a
    // separate dust-heavy explosion at every successful impact.
    internal sealed class BFBombardExplosiveArrow : ModProjectile
    {
        private bool ExtraLarge => Projectile.ai[0] == 1f;

        public override string Texture => "CalamityLegendsReturn/Weapons/BlossomFlux/RightClick/DBomb/BFArrow_DBomb";

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.scale = ExtraLarge ? 3.15f : 2.25f;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.82f, 0.08f) * 0.85f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Explode(target.Center);

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode(Projectile.Center);
            return true;
        }

        private void Explode(Vector2 center)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                int explosionSize = ExtraLarge ? 236 : 156;
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(), center, Vector2.Zero,
                    ModContent.ProjectileType<BFBombardExplosiveBlast>(),
                    Math.Max(1, (int)(Projectile.damage * 0.72f)), Projectile.knockBack, Projectile.owner, explosionSize);
            }

            if (Main.dedServ)
                return;

            int dustCount = ExtraLarge ? 112 : 72;
            for (int i = 0; i < dustCount; i++)
            {
                Dust dust = Dust.NewDustPerfect(center, DustID.GoldFlame,
                    Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4f, ExtraLarge ? 16f : 11f),
                    40, new Color(255, 220, 40), Main.rand.NextFloat(1.1f, ExtraLarge ? 2.5f : 1.9f));
                dust.noGravity = true;
            }

            SoundEngine.PlaySound(BlossomFluxSounds.RightBombardExplosion with { Pitch = ExtraLarge ? -0.12f : 0.08f }, center);
            Main.player[Projectile.owner].SetScreenshake(ExtraLarge ? 12f : 7f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Color yellowShaderTint = ExtraLarge ? new Color(255, 247, 116, 0) : new Color(255, 220, 44, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, yellowShaderTint, Projectile.rotation,
                texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }

    internal sealed class BFBombardExplosiveBlast : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] != 0f)
                return;

            Projectile.localAI[0] = 1f;
            int diameter = Math.Max(32, (int)Projectile.ai[0]);
            Vector2 center = Projectile.Center;
            Projectile.width = diameter;
            Projectile.height = diameter;
            Projectile.Center = center;
        }
    }
}
