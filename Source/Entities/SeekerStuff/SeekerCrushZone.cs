using Celeste.Mod.Entities;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/SeekerCrushZone")]
    [Tracked]
    public class SeekerCrushZone : Entity
    {
        private bool activated = false;

        private Seeker capturedSeeker;

        private Hitbox detectHitbox;

        public SeekerCrushZone(Vector2 position, int width, int height)
            : base(position)
        {
            Collider = new Hitbox(width, height);
            detectHitbox = new Hitbox(width - 10f, height - 10f, 5f, 5f);
        }

        public SeekerCrushZone(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
        }

        public override void Update()
        {
            base.Update();
            if (!activated)
            {
                Collider normalCollider = Collider;
                Collider = detectHitbox;
                Level level = SceneAs<Level>();
                foreach (Seeker seeker in level.Tracker.GetEntities<Seeker>())
                {
                    if (seeker.CollideCheck(this))
                    {
                        activated = true;
                        capturedSeeker = seeker;
                        SeekerCrushZoneBlock block = level.Tracker.GetNearestEntity<SeekerCrushZoneBlock>(Position);
                        block?.Activate(this);
                        break;
                    }
                }
                Collider = normalCollider;
            }
            else if (capturedSeeker != null)
            {
                KeepInside(capturedSeeker);
            }
        }

        private void KeepInside(Seeker seeker)
        {
            seeker.Left = Math.Max(seeker.Left, this.Left + 6f);
            seeker.Right = Math.Min(seeker.Right, this.Right - 6f);
            seeker.Top = Math.Max(seeker.Top, this.Top + 6f);
            seeker.Bottom = Math.Min(seeker.Bottom, this.Bottom - 6f);
        }

        // don't override since rendering is being done manually
        public new void Render()
        {
            base.Render();
            Vector2 position = Position - SceneAs<Level>().Camera.Position;
            Draw.Rect(position + Vector2.One, Width - 2, Height - 2, Color.Violet * 0.15f);
            Draw.HollowRect(position, Width, Height, Color.Violet * 0.5f);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider collider = Collider;
            Collider = detectHitbox;
            Draw.HollowRect(Collider, Color.MediumSeaGreen);
            Collider = collider;
        }

        public static void Load()
        {
            Everest.Events.LevelLoader.OnLoadingThread += LevelLoader_OnLoadingThread;
        }

        public static void Unload()
        {
            Everest.Events.LevelLoader.OnLoadingThread -= LevelLoader_OnLoadingThread;
        }

        private static void LevelLoader_OnLoadingThread(Level level)
        {
            level.Add(new SeekerCrushZoneRenderer());
        }

        /// <summary>
        /// Renders <see cref="SeekerCrushZone"/>s on the SubHUD layer to avoid being affected by lighting.
        /// </summary>
        private class SeekerCrushZoneRenderer : Entity
        {
            private VirtualRenderTarget crushZonesTarget;

            private bool hasSeekerCrushZones;

            public SeekerCrushZoneRenderer()
            {
                Tag = Tags.Global | TagsExt.SubHUD;
                Add(new BeforeRenderHook(BeforeRender));
            }

            private void BeforeRender()
            {
                List<Entity> crushZones = Scene.Tracker.GetEntities<SeekerCrushZone>();
                hasSeekerCrushZones = crushZones.Count > 0;
                if (!hasSeekerCrushZones)
                {
                    return;
                }
                crushZonesTarget ??= VirtualContent.CreateRenderTarget("seeker-crush-zones", 320, 180);
                Engine.Graphics.GraphicsDevice.SetRenderTarget(crushZonesTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                foreach (Entity entity in crushZones)
                {
                    if (entity.Visible)
                    {
                        (entity as SeekerCrushZone).Render();
                    }
                }
                Draw.SpriteBatch.End();
            }

            public override void Render()
            {
                if (!hasSeekerCrushZones)
                {
                    return;
                }
                if (crushZonesTarget != null && !crushZonesTarget.IsDisposed)
                {
                    SubHudRenderer.EndRender();
                    // PointClamp and scale of 6 to basically pretend we're on a gameplay layer
                    SubHudRenderer.BeginRender(sampler: SamplerState.PointClamp);
                    Draw.SpriteBatch.Draw((RenderTarget2D)crushZonesTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 6f, SpriteEffects.None, 0f);
                    SubHudRenderer.EndRender();
                    SubHudRenderer.BeginRender();
                }
            }

            public override void Removed(Scene scene)
            {
                base.Removed(scene);
                Dispose();
            }

            public override void SceneEnd(Scene scene)
            {
                base.SceneEnd(scene);
                Dispose();
            }

            public void Dispose()
            {
                if (crushZonesTarget != null && !crushZonesTarget.IsDisposed)
                {
                    crushZonesTarget.Dispose();
                }
                crushZonesTarget = null;
            }
        }
    }
}
