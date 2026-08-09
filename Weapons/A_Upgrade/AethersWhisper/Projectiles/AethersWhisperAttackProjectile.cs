using System;
using CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendReturn.Weapons.A_Upgrade.AethersWhisper.Projectiles
{
    // 一个受控的统一弹幕载体：避免旧版蓄力弹/折射束继续混入新的五段循环。
    internal sealed class AethersWhisperAttackProjectile : ModProjectile
    {
        public const int Fireball = 0, LiquidCross = 1, Cyclone = 2, JellyLightning = 3, FinalBeam = 4, EnergyOrb = 5, DelayedMainBeam = 6;
        private int Mode => (int)Projectile.ai[0];
        private int Variant => (int)Projectile.ai[1];
        private int Age => Projectile.localAI[0] is var age ? (int)age : 0;
        public override string Texture => "CalamityMod/Particles/BloomCircle";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = Mode == LiquidCross ? -1 : 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 90;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            int age = Age;
            if (Mode == Fireball)
            {
                Projectile.velocity *= 1.012f;
                if (age > 40) Projectile.Kill();
            }
            else if (Mode == LiquidCross)
            {
                Projectile.velocity *= 1.035f;
                if (age > 45) Projectile.Kill();
            }
            else if (Mode == Cyclone)
            {
                NPC target = FindTarget(700f);
                if (target != null) Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(target.Center) * 22f, 0.075f);
                Projectile.rotation += 0.35f;
            }
            else if (Mode == JellyLightning)
            {
                Projectile.velocity.Y += age < 24 ? 0.23f : 0.72f;
                Projectile.velocity.X *= age < 24 ? 0.992f : 1.02f;
            }
            else if (Mode == FinalBeam || Mode == DelayedMainBeam)
            {
                if (Mode == DelayedMainBeam && age <= Variant) { Projectile.velocity = Vector2.Zero; return; }
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 28f;
                if (age > (Mode == FinalBeam ? 25 : Variant + 25)) Projectile.Kill();
            }
            else if (Mode == EnergyOrb)
                UpdateEnergyOrb();

            Lighting.AddLight(Projectile.Center, AethersWhisperVisuals.Lerp((age % 30) / 30f).ToVector3() * 0.45f);
            if (!Main.dedServ && age % 3 == 0)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, AethersWhisperVisuals.ElectricDust, -Projectile.velocity * 0.08f, 0, AethersWhisperVisuals.Lerp(Main.rand.NextFloat()), 0.8f);
                d.noGravity = true;
            }
        }

        private void UpdateEnergyOrb()
        {
            Projectile other = null;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.whoAmI != Projectile.whoAmI && p.owner == Projectile.owner && p.type == Type && (int)p.ai[0] == EnergyOrb && ((int)p.ai[1] / 10 == Variant / 10)) { other = p; break; }
            }
            if (other != null)
            {
                Vector2 force = Projectile.DirectionTo(other.Center) * (Age < 22 ? 0.42f : -0.38f);
                Projectile.velocity += force;
                if (Age > 22 && Vector2.DistanceSquared(Projectile.Center, other.Center) < 20f * 20f && Age % 8 == 0)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2) * 1.08f;
                    Projectile.damage = Math.Max(1, Projectile.damage);
                }
            }
            Projectile.velocity *= 0.992f;
            if (Age > 105) Projectile.Kill();
        }

        private static NPC FindTarget(float range)
        {
            NPC chosen = null; float best = range * range;
            foreach (NPC npc in Main.ActiveNPCs) if (npc.CanBeChasedBy() && Vector2.DistanceSquared(npc.Center, Main.player[Main.myPlayer].Center) < best) { best = Vector2.DistanceSquared(npc.Center, Main.player[Main.myPlayer].Center); chosen = npc; }
            return chosen;
        }

        public override void OnKill(int timeLeft)
        {
            if (Mode != Fireball) return;
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, AethersWhisperVisuals.ElectricDust, Main.rand.NextVector2Circular(5f, 5f), 0, AethersWhisperVisuals.ToWhite(AethersWhisperVisuals.ShimmerCyan, 0.4f), 1.2f);
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            AethersWhisperVisuals.BeginAdditive(sb);
            Color color = AethersWhisperVisuals.Lerp((Age % 40) / 40f) with { A = 0 };
            float radius = Mode == FinalBeam || Mode == DelayedMainBeam ? 48f : Mode == Cyclone ? 30f : 20f;
            if (Mode == FinalBeam || Mode == DelayedMainBeam)
            {
                Vector2 back = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitX) * 110f;
                AethersWhisperVisuals.DrawBeamSegment(sb, back, Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.UnitX) * 130f, color, AethersWhisperProgression.FinalityRift ? 30f : 18f);
            }
            else AethersWhisperVisuals.DrawEnergyOrb(sb, Projectile.Center, radius, color, 0.85f, new Vector2(1f, Mode == JellyLightning ? 1.45f : 1f));
            AethersWhisperVisuals.EndAdditive(sb);
            return false;
        }
    }
}
