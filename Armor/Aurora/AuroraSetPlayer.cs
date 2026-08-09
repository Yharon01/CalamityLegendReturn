using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.Armor.Aurora
{
    /// <summary>
    /// Drives everything dynamic about the Aurora set: the velocity-reactive motion
    /// trail (same feel as the reference set), a smoothed "danger" value read from the
    /// player's health, and the full-set particle aura.
    /// </summary>
    public class AuroraSetPlayer : ModPlayer
    {
        public const int MaxHistory = 30;
        public const float MaxLength = 90f;

        private const float LengthPerSpeed = 9f;
        private const float GrowRate = 12f;
        private const float BaseRetract = 3f;
        private const float RetractPerDrop = 6f;
        private const float BoostDecay = 0.85f;

        // Motion trail
        public List<Vector2> Centers = new();
        public float TrailLength;
        private Vector2 previousVelocity;
        private float retractBoost;

        // Aurora state
        public float Danger;         // 0 = healthy, 1 = near death (smoothed for a soft pulse)
        public bool SetBonusActive;  // full set worn this frame; set in UpdateArmorSet, cleared here

        public override void ResetEffects()
        {
            SetBonusActive = false;
        }

        /// <summary>True when at least one glowing Aurora piece (head or body) is worn.</summary>
        public bool WearingGlowPiece()
        {
            int headSlot = EquipLoader.GetEquipSlot(Mod, nameof(AuroraHelm), EquipType.Head);
            int bodySlot = EquipLoader.GetEquipSlot(Mod, nameof(AuroraPlate), EquipType.Body);
            return Player.head == headSlot || Player.body == bodySlot;
        }

        public override void PostUpdate()
        {
            // Smoothly track how hurt the player is so the glow can react without flickering.
            float lifeFraction = MathHelper.Clamp(Player.statLife / (float)Math.Max(Player.statLifeMax2, 1), 0f, 1f);
            Danger = MathHelper.Lerp(Danger, 1f - lifeFraction, 0.05f);

            if (!WearingGlowPiece())
            {
                Centers.Clear();
                TrailLength = 0f;
                retractBoost = 0f;
                previousVelocity = Player.velocity;
                return;
            }

            Centers.Insert(0, Player.MountedCenter);

            if (Centers.Count > MaxHistory)
                Centers.RemoveAt(Centers.Count - 1);

            float target = MathHelper.Clamp(Player.velocity.Length() * LengthPerSpeed, 0f, MaxLength);
            float drop = (previousVelocity - Player.velocity).Length();

            retractBoost = MathHelper.Max(retractBoost * BoostDecay, drop * RetractPerDrop);

            if (target > TrailLength)
                TrailLength = MathHelper.Min(target, TrailLength + GrowRate);
            else
                TrailLength = MathHelper.Max(target, TrailLength - BaseRetract - retractBoost);

            previousVelocity = Player.velocity;

            if (SetBonusActive)
                EmitAuroraAura();
        }

        /// <summary>Full-set flourish: drifting aurora sparks that rise around the player.</summary>
        private void EmitAuroraAura()
        {
            if (Main.dedServ)
                return;

            float speed = Player.velocity.Length();
            int count = 1 + (int)MathHelper.Clamp(speed * 0.4f, 0f, 3f);

            for (int i = 0; i < count; i++)
            {
                if (!Main.rand.NextBool(2))
                    continue;

                Vector2 pos = Player.Center + new Vector2(Main.rand.NextFloat(-16f, 16f), Main.rand.NextFloat(-24f, 24f));
                float hue = (0.34f + 0.36f * (0.5f + 0.5f * (float)Math.Sin(Main.timeForVisualEffects * 0.02 + pos.Y * 0.05f))) % 1f;
                Color color = Main.hslToRgb(hue, 1f, 0.6f);

                Dust dust = Dust.NewDustPerfect(pos, DustID.RainbowMk2, new Vector2(0f, -1.2f), 0, color, 1.1f);
                dust.noGravity = true;
                dust.fadeIn = 0.6f;
            }
        }
    }
}
