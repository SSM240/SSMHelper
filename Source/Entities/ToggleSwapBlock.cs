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
            if (returnTimer > 0f)
            {
                returnTimer -= Engine.DeltaTime;
                if (returnTimer <= 0f)
                {
                    target = 0;
                    speed = 0f;
                    returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", base.Center);
                }
            }
            if (burst != null)
            {
                burst.Position = base.Center;
            }
            redAlpha = Calc.Approach(redAlpha, (target != 1) ? 1 : 0, Engine.DeltaTime * 32f);
            if (target == 0 && lerp == 0f)
            {
                middleRed.SetAnimationFrame(0);
                middleGreen.SetAnimationFrame(0);
            }
            if (target == 1)
            {
                speed = Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
            }
            else
            {
                speed = Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime);
            }
            float num = lerp;
            lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            if (lerp != num)
            {
                Vector2 liftSpeed = (end - start) * speed;
                Vector2 position = Position;
                if (target == 1)
                {
                    liftSpeed = (end - start) * maxForwardSpeed;
                }
                if (lerp < num)
                {
                    liftSpeed *= -1f;
                }
                if (target == 1 && base.Scene.OnInterval(0.02f))
                {
                    MoveParticles(end - start);
                }
                MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
                if (position != Position)
                {
                    Audio.Position(moveSfx, base.Center);
                    Audio.Position(returnSfx, base.Center);
                    if (Position == start && target == 0)
                    {
                        Audio.SetParameter(returnSfx, "end", 1f);
                        Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", base.Center);
                    }
                    else if (Position == end && target == 1)
                    {
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", base.Center);
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
