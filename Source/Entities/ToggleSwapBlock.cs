using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using System;

namespace Celeste.Mod.SSMHelper.Entities
{
    // yes i know like 2 other versions of this mechanic exist
    // i still want more control over how this one works
    [CustomEntity("SSMHelper/ToggleSwapBlock")]
    [Tracked]
    [TrackedAs(typeof(SwapBlock))]
    public class ToggleSwapBlock : SwapBlock
    {
        public static ParticleType P_MoveBlue;
        public static ParticleType P_MoveRed;

        public bool RenderBG;

        public ToggleSwapBlock(Vector2 position, float width, float height, Vector2 node, Themes theme) 
            : base(position, width, height, node, theme)
        {
        }

        public ToggleSwapBlock(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum("theme", Themes.Normal))
        {
            RenderBG = data.Bool("renderBG", true);
            Remove(Get<DashListener>());
            Add(new DashListener
            {
                OnDash = OnDash
            });
            Direction *= -1f; // dumb hack

            MTexture blockTexture;
            MTexture blockRedTexture;
            if (Theme == Themes.Moon)
            {
                blockTexture = GFX.Game["objects/SSMHelper/swapblock/moon/block"];
                blockRedTexture = GFX.Game["objects/SSMHelper/swapblock/moon/blockRed"];
            }
            else
            {
                blockTexture = GFX.Game["objects/SSMHelper/swapblock/block"];
                blockRedTexture = GFX.Game["objects/SSMHelper/swapblock/blockRed"];
            }
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    nineSliceGreen[i, j] = blockTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                    nineSliceRed[i, j] = blockRedTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
                }
            }
            Remove(middleGreen);
            Remove(middleRed);
            if (Theme == Themes.Moon)
            {
                Add(middleGreen = GFX.SpriteBank.Create("ssmhelper_swapBlockLightMoon"));
                Add(middleRed = GFX.SpriteBank.Create("ssmhelper_swapBlockLightRedMoon"));
            }
            else
            {
                Add(middleGreen = GFX.SpriteBank.Create("ssmhelper_swapBlockLight"));
                Add(middleRed = GFX.SpriteBank.Create("ssmhelper_swapBlockLightRed"));
            }
        }

        private new void OnDash(Vector2 direction)
        {
            Swapping = true;
            target = 1 - target;
            Direction *= -1f;
            burst = (Scene as Level).Displacement.AddBurst(Center, 0.2f, 0f, 16f);
            float betterLerp = lerp;
            if (betterLerp > 0.5f)
            {
                betterLerp = 1f - betterLerp;
            }
            if (betterLerp >= 0.2f)
            {
                speed = maxForwardSpeed;
            }
            else
            {
                speed = MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, betterLerp / 0.2f);
            }
            Audio.Stop(moveSfx);
            moveSfx = Audio.Play(SFX.game_05_swapblock_move, Center);
        }

        // calls Solid.Update while skipping SwapBlock.Update
        [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
        private void Solid_Update()
        {
        }

        // largely copied from SwapBlock.Update
        public override void Update()
        {
            Solid_Update();
            if (burst != null)
            {
                burst.Position = Center;
            }
            redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
            if (lerp == 0f || lerp == 1f)
            {
                middleRed.SetAnimationFrame(0);
                middleGreen.SetAnimationFrame(0);
            }
            speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
            float prevLerp = lerp;
            lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            if (lerp != prevLerp)
            {
                Vector2 liftSpeed = (end - start) * speed;
                Vector2 prevPosition = Position;
                liftSpeed = (end - start) * maxForwardSpeed;
                if (lerp < prevLerp)
                {
                    liftSpeed *= -1f;
                }
                if (Scene.OnInterval(0.02f))
                {
                    MoveParticles(end - start);
                }
                MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
                if (prevPosition != Position)
                {
                    Audio.Position(moveSfx, Center);
                    if ((Position == start && target == 0) || (Position == end && target == 1))
                    {
                        Audio.Play(SFX.game_05_swapblock_move_end, Center);
                    }
                }
            }
            if (Swapping && lerp >= 1f)
            {
                Swapping = false;
            }
            StopPlayerRunIntoAnimation = lerp <= 0f || lerp >= 1f;
        }

        public override void Render()
        {
            Vector2 vector = Position + Shake;
            if (lerp != target && speed > 0f)
            {
                Vector2 vector2 = (end - start).SafeNormalize();
                if (target == 1)
                {
                    vector2 *= -1f;
                }
                float blur = this.speed / this.maxForwardSpeed;
                float len = 16f * blur;
                for (int i = 2; i < len; i += 2)
                {
                    MTexture[,] nineSlice = (target == 1) ? nineSliceGreen : nineSliceRed;
                    Sprite middle = (target == 1) ? middleGreen : middleRed;
                    DrawBlockStyle(vector + vector2 * i, Width, Height, nineSlice, middle, Color.White * (1f - i / len));
                }
            }
            if (redAlpha < 1f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceGreen, middleGreen, Color.White);
            }
            if (redAlpha > 0f)
            {
                DrawBlockStyle(vector, Width, Height, nineSliceRed, middleRed, Color.White * redAlpha);
            }
        }

        private new void MoveParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 positionRange;
            float direction;
            float add;
            if (normal.X > 0f)
            {
                position = CenterLeft;
                positionRange = Vector2.UnitY * (Height - 6f);
                direction = MathF.PI;
                add = Math.Max(2f, Height / 14f);
            }
            else if (normal.X < 0f)
            {
                position = CenterRight;
                positionRange = Vector2.UnitY * (Height - 6f);
                direction = 0f;
                add = Math.Max(2f, Height / 14f);
            }
            else if (normal.Y > 0f)
            {
                position = TopCenter;
                positionRange = Vector2.UnitX * (Width - 6f);
                direction = -MathF.PI / 2f;
                add = Math.Max(2f, Width / 14f);
            }
            else
            {
                position = BottomCenter;
                positionRange = Vector2.UnitX * (Width - 6f);
                direction = MathF.PI / 2f;
                add = Math.Max(2f, Width / 14f);
            }
            particlesRemainder += add;
            int amount = (int)particlesRemainder;
            particlesRemainder -= amount;
            positionRange *= 0.5f;
            if (target == 1)
            {
                SceneAs<Level>().Particles.Emit(P_MoveBlue, amount, position, positionRange, direction);
            }
            else
            {
                SceneAs<Level>().Particles.Emit(P_MoveRed, amount, position, positionRange, direction);
            }
        }

        public static void LoadParticles()
        {
            P_MoveBlue = new ParticleType(P_Move)
            {
                Color = Calc.HexToColor("36fbbf"),
                Color2 = Calc.HexToColor("308fbe")
            };
            P_MoveRed = new ParticleType(P_Move)
            {
                Color = Calc.HexToColor("ffa5a1"),
                Color2 = Calc.HexToColor("ca2424")
            };
        }

        public static void Load()
        {
            On.Celeste.SwapBlock.PathRenderer.Render += On_PathRenderer_Render;
        }

        public static void Unload()
        {
            On.Celeste.SwapBlock.PathRenderer.Render -= On_PathRenderer_Render;
        }

        private static void On_PathRenderer_Render(On.Celeste.SwapBlock.PathRenderer.orig_Render orig, Entity self)
        {
            SwapBlock block = (self as PathRenderer)?.block;
            Themes? oldTheme = null;
            if (block != null && block is ToggleSwapBlock toggleBlock)
            {
                oldTheme = block.Theme;
                // the only thing theme affects in this method is whether the path is drawn
                block.Theme = toggleBlock.RenderBG ? Themes.Normal : Themes.Moon;
            }
            orig(self);
            if (oldTheme != null)
            {
                block.Theme = oldTheme.Value;
            }
        }
    }
}
