using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityLegendsReturn.Armor.Aurora
{
    /// <summary>
    /// Paints the masked regions of the Aurora helm/plate with a living aurora glow and
    /// leaves a velocity-reactive light trail behind the player.
    ///
    /// Innovations over the reference set (which used a single time-driven rainbow color):
    ///  - The glow rides a green->violet "aurora band" that slides with time.
    ///  - As the player's health drops, the whole set shifts toward a hot red alarm color
    ///    and gains a heartbeat pulse (state-driven color, per the extension tutorial).
    ///  - Each connected glow region fans out into its own hue by vertical position, so the
    ///    trail reads as a flowing aurora curtain instead of one flat color.
    ///  - Wearing the full set brightens the whole effect.
    /// </summary>
    public class AuroraGlowLayer : PlayerDrawLayer
    {
        private const float TrailAlpha = 0.7f;
        private const float TrailTailWidth = 0.15f;
        private const byte AlphaThreshold = 10;

        private const string MaskFolder = "CalamityLegendsReturn/Armor/Aurora/";

        private static readonly Dictionary<Texture2D, Color[]> pixelCache = new();
        private static readonly Dictionary<Texture2D, Dictionary<Rectangle, GlowRegion[]>> regionCache = new();

        private class GlowRegion
        {
            public Vector2 Center;
            public Vector2[] Boundary;
        }

        public override void Unload()
        {
            pixelCache.Clear();
            regionCache.Clear();
        }

        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.ArmOverItem);

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => !drawInfo.drawPlayer.dead;

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;

            int headSlot = EquipLoader.GetEquipSlot(Mod, nameof(AuroraHelm), EquipType.Head);
            int bodySlot = EquipLoader.GetEquipSlot(Mod, nameof(AuroraPlate), EquipType.Body);

            Texture2D headSource = player.head == headSlot ? TextureAssets.ArmorHead[headSlot].Value : null;
            Texture2D bodySource = player.body == bodySlot ? TextureAssets.ArmorBodyComposite[bodySlot].Value : null;

            if (headSource == null && bodySource == null)
                return;

            AuroraSetPlayer state = player.GetModPlayer<AuroraSetPlayer>();
            double t = Main.timeForVisualEffects;
            float danger = state.Danger;

            // Aurora band drifts between green and violet with time...
            float bandHue = MathHelper.Lerp(0.34f, 0.70f, 0.5f + 0.5f * (float)Math.Sin(t * 0.02));
            // ...and slides toward a hot red the closer the player is to death.
            float baseHue = MathHelper.Lerp(bandHue, 0.02f, danger);

            // Gentle shimmer, plus a heartbeat pulse when hurt, plus a full-set brightness boost.
            float light = 0.5f + 0.05f * (float)Math.Sin(t * 0.15);
            light += danger * 0.18f * (0.5f + 0.5f * (float)Math.Sin(t * 0.5));
            if (state.SetBonusActive)
                light += 0.08f;
            light = MathHelper.Clamp(light, 0f, 0.85f);

            Color glowColor = Main.hslToRgb(baseHue % 1f, 1f, light);

            List<DrawData> cache = drawInfo.DrawDataCache;
            List<Vector2> history = state.Centers;
            float trailLength = state.TrailLength;

            Texture2D headMask = headSource == null ? null : ModContent.Request<Texture2D>(MaskFolder + "AuroraHelm_Head_Mask").Value;
            Texture2D bodyMask = bodySource == null ? null : ModContent.Request<Texture2D>(MaskFolder + "AuroraPlate_Body_Mask").Value;

            int scanned = cache.Count;

            for (int i = 0; i < scanned; i++)
            {
                DrawData source = cache[i];
                Texture2D mask = null;

                if (headSource != null && source.texture == headSource)
                    mask = headMask;
                else if (bodySource != null && source.texture == bodySource)
                    mask = bodyMask;

                if (mask == null)
                    continue;

                if (history.Count >= 2 && trailLength > 1f && !source.useDestinationRectangle)
                {
                    Rectangle frame = source.sourceRect ?? mask.Bounds;
                    float frameHeight = Math.Max(frame.Height, 1);

                    foreach (GlowRegion region in GetRegions(mask, frame))
                    {
                        // Fan each region out into its own aurora hue by vertical position.
                        float regionHue = (baseHue + region.Center.Y / frameHeight * 0.20f) % 1f;
                        Color regionColor = Main.hslToRgb(regionHue, 1f, light);
                        DrawRegionTrail(cache, source, frame, region, history, trailLength, regionColor);
                    }
                }

                DrawData glow = source;
                glow.texture = mask;
                glow.color = glowColor;
                glow.shader = 0;
                cache.Add(glow);
            }
        }

        private static void DrawRegionTrail(List<DrawData> cache, DrawData source, Rectangle frame, GlowRegion region,
            List<Vector2> history, float trailLength, Color glowColor)
        {
            Vector2 origin = TexelToScreen(source, frame, region.Center);
            Vector2[] outline = new Vector2[region.Boundary.Length];

            for (int i = 0; i < outline.Length; i++)
                outline[i] = TexelToScreen(source, frame, region.Boundary[i]) - origin;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle pixelFrame = new(0, 0, 1, 1);
            Vector2 pixelOrigin = new(0f, 0.5f);
            float traveled = 0f;

            for (int i = 0; i < history.Count - 1; i++)
            {
                Vector2 start = origin + (history[i] - history[0]);
                Vector2 end = origin + (history[i + 1] - history[0]);
                Vector2 delta = end - start;
                float length = delta.Length();

                if (length < 0.01f)
                    continue;

                float progress = traveled / trailLength;

                if (progress >= 1f)
                    break;

                traveled += length;

                Vector2 perpendicular = new(-delta.Y / length, delta.X / length);
                float width = ProjectedExtent(outline, perpendicular) * MathHelper.Lerp(1f, TrailTailWidth, progress);

                DrawData segment = new(pixel, start, pixelFrame, glowColor * MathHelper.Lerp(TrailAlpha, 0f, progress),
                    delta.ToRotation(), pixelOrigin, new Vector2(length, width), SpriteEffects.None, 0);

                cache.Add(segment);
            }
        }

        private static float ProjectedExtent(Vector2[] outline, Vector2 axis)
        {
            float min = float.MaxValue;
            float max = float.MinValue;

            foreach (Vector2 point in outline)
            {
                float projection = Vector2.Dot(point, axis);

                if (projection < min) min = projection;
                if (projection > max) max = projection;
            }

            return MathHelper.Max(max - min + 1f, 1f);
        }

        private static Vector2 TexelToScreen(DrawData data, Rectangle frame, Vector2 texel)
        {
            float u = texel.X;
            float v = texel.Y;

            if ((data.effect & SpriteEffects.FlipHorizontally) != 0)
                u = frame.Width - u;

            if ((data.effect & SpriteEffects.FlipVertically) != 0)
                v = frame.Height - v;

            Vector2 offset = new((u - data.origin.X) * data.scale.X, (v - data.origin.Y) * data.scale.Y);

            return data.position + offset.RotatedBy(data.rotation);
        }

        private static GlowRegion[] GetRegions(Texture2D mask, Rectangle frame)
        {
            if (!regionCache.TryGetValue(mask, out Dictionary<Rectangle, GlowRegion[]> frames))
            {
                frames = new Dictionary<Rectangle, GlowRegion[]>();
                regionCache[mask] = frames;
            }

            if (frames.TryGetValue(frame, out GlowRegion[] cached))
                return cached;

            if (!pixelCache.TryGetValue(mask, out Color[] pixels))
            {
                pixels = new Color[mask.Width * mask.Height];
                mask.GetData(pixels);
                pixelCache[mask] = pixels;
            }

            GlowRegion[] result = BuildRegions(pixels, mask.Width, mask.Height, frame);
            frames[frame] = result;

            return result;
        }

        private static GlowRegion[] BuildRegions(Color[] pixels, int textureWidth, int textureHeight, Rectangle frame)
        {
            bool[] solid = new bool[frame.Width * frame.Height];

            for (int y = 0; y < frame.Height; y++)
            {
                int texY = frame.Y + y;

                if (texY < 0 || texY >= textureHeight)
                    continue;

                for (int x = 0; x < frame.Width; x++)
                {
                    int texX = frame.X + x;

                    if (texX >= 0 && texX < textureWidth && pixels[texY * textureWidth + texX].A > AlphaThreshold)
                        solid[y * frame.Width + x] = true;
                }
            }

            List<GlowRegion> regions = new();
            bool[] visited = new bool[solid.Length];
            Stack<int> pending = new();
            List<int> members = new();

            for (int seed = 0; seed < solid.Length; seed++)
            {
                if (!solid[seed] || visited[seed])
                    continue;

                members.Clear();
                pending.Push(seed);
                visited[seed] = true;

                while (pending.Count > 0)
                {
                    int index = pending.Pop();
                    members.Add(index);

                    int x = index % frame.Width;
                    int y = index / frame.Width;

                    TryVisit(solid, visited, pending, frame.Width, frame.Height, x - 1, y);
                    TryVisit(solid, visited, pending, frame.Width, frame.Height, x + 1, y);
                    TryVisit(solid, visited, pending, frame.Width, frame.Height, x, y - 1);
                    TryVisit(solid, visited, pending, frame.Width, frame.Height, x, y + 1);
                }

                regions.Add(CreateRegion(solid, members, frame.Width, frame.Height));
            }

            return regions.ToArray();
        }

        private static void TryVisit(bool[] solid, bool[] visited, Stack<int> pending, int width, int height, int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            int index = y * width + x;

            if (!solid[index] || visited[index])
                return;

            visited[index] = true;
            pending.Push(index);
        }

        private static GlowRegion CreateRegion(bool[] solid, List<int> members, int width, int height)
        {
            long sumX = 0;
            long sumY = 0;
            List<Vector2> boundary = new();

            foreach (int index in members)
            {
                int x = index % width;
                int y = index / width;

                sumX += x;
                sumY += y;

                if (IsEdge(solid, width, height, x, y))
                    boundary.Add(new Vector2(x + 0.5f, y + 0.5f));
            }

            return new GlowRegion
            {
                Center = new Vector2(sumX / (float)members.Count + 0.5f, sumY / (float)members.Count + 0.5f),
                Boundary = boundary.ToArray()
            };
        }

        private static bool IsEdge(bool[] solid, int width, int height, int x, int y)
        {
            return !IsSolid(solid, width, height, x - 1, y)
                || !IsSolid(solid, width, height, x + 1, y)
                || !IsSolid(solid, width, height, x, y - 1)
                || !IsSolid(solid, width, height, x, y + 1);
        }

        private static bool IsSolid(bool[] solid, int width, int height, int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height && solid[y * width + x];
        }
    }
}
