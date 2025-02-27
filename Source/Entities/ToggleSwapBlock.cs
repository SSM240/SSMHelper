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
