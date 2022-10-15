﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/SeekerCrushZone")]
    public class SeekerCrushZone : Entity
    {
        private bool activated = false;

        private Seeker capturedSeeker;

        private Hitbox detectHitbox;

        public SeekerCrushZone(Vector2 position, int width, int height, 
            char tile1, char tile2)
            : base(position)
        {
            Collider = new Hitbox(width, height);
            detectHitbox = new Hitbox(width - 12f, height - 12f, 6f, 6f);
        }

        public SeekerCrushZone(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height,
                  data.Char("tile1", 'g'), data.Char("tile2", 'G'))
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
                Collider collider = Collider;
                Collider = detectHitbox;
                Level level = SceneAs<Level>();
                foreach (Seeker seeker in level.Tracker.GetEntities<Seeker>())
                {
                    if (seeker.CollideCheck(this))
                    {
                        activated = true;
                        capturedSeeker = seeker;
                        SeekerCrushZoneBlock block = level.Tracker.GetEntity<SeekerCrushZoneBlock>();
                        block?.Activate(this);
                        break;
                    }
                }
                Collider = collider;
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

        public override void Render()
        {
            base.Render();
            Draw.Rect(Position + Vector2.One, Width - 2, Height - 2, Color.Violet * 0.15f);
            Draw.HollowRect(Collider, Color.Violet * 0.5f);
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Collider collider = Collider;
            Collider = detectHitbox;
            Draw.HollowRect(Collider, Color.MediumSeaGreen);
            Collider = collider;
        }
    }
}
