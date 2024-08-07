﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities
{
    public class DashBoostFieldParticleRenderer : Component
    {
        private const float density = 0.02f;

        private List<Particle> particles = new List<Particle>();

        private DashBoostField BoostField => Entity as DashBoostField;
        // thank you to Vexatos for being better at math than me
        private float ParticlePositionRadius => (float)Math.Sqrt(BoostField.Radius * (BoostField.Radius + 75f));

        private float maximumDistance;

        public DashBoostFieldParticleRenderer()
            : base(active: true, visible: true) { }

        public override void Added(Entity entity)
        {
            base.Added(entity);
            int particleCount = (int)(Math.PI * Math.Pow(ParticlePositionRadius, 2) * density);
            for (int i = 0; i < particleCount; i++)
            {
                particles.Add(new Particle(this));
            }
            // err on the side of generosity
            maximumDistance = BoostField.Radius - 1f;
        }

        public override void Update()
        {
            base.Update();
            foreach (Particle particle in particles)
            {
                if (particle.Percent >= 1f)
                    particle.Reset();
                particle.Percent += Engine.DeltaTime / particle.Duration;
                particle.Position += particle.Velocity * Engine.DeltaTime;
            }
        }

        public override void Render()
        {
            base.Render();
            if (!InView())
            {
                return;
            }
            foreach (Particle particle in particles)
            {
                particle.Alpha = (particle.Percent >= 0.7f)
                    ? Calc.ClampedMap(particle.Percent, 0.7f, 1f, 1f, 0f)
                    : Calc.ClampedMap(particle.Percent, 0f, 0.3f);
                if (Vector2.DistanceSquared(BoostField.Center, particle.Position) <= maximumDistance * maximumDistance)
                {
                    particle.RenderPosition = particle.Position;
                }
                else
                {
                    // clamp particle's render position to boost field radius to create a sort of halo
                    Vector2 offset = particle.Position - BoostField.Center;
                    particle.RenderPosition = BoostField.Center + offset.SafeNormalize() * maximumDistance;
                }
                Draw.Point(particle.RenderPosition, particle.Color * particle.Alpha);
            }
        }

        private bool InView()
        {
            Camera camera = (base.Scene as Level).Camera;
            Vector2 center = BoostField.Center;
            float left = center.X - ParticlePositionRadius;
            float right = center.X + ParticlePositionRadius;
            float top = center.Y - ParticlePositionRadius;
            float bottom = center.Y + ParticlePositionRadius;
            return camera.X + 320f > left - 8f
                && camera.X < right + 8f
                && camera.Y + 180f > top - 8f
                && camera.Y < bottom - 8f;
        }

        private class Particle
        {
            public Vector2 Position;
            public Vector2 RenderPosition;
            public Color Color;
            public Vector2 Velocity;
            public float Duration;
            public float Percent;
            public float Alpha;

            private DashBoostFieldParticleRenderer parent;
            private Color[] colors;

            public Particle(DashBoostFieldParticleRenderer parent)
            {
                this.parent = parent;
                colors = new Color[] {
                    Color.Lerp(parent.BoostField.Color, Color.White, 0.25f),
                    Color.Lerp(parent.BoostField.Color, Color.White, 0.50f),
                    Color.Lerp(parent.BoostField.Color, Color.White, 0.75f),
                };
                Reset(Calc.Random.NextFloat());
            }

            public void Reset(float percent = 0f)
            {
                Percent = percent;
                float offsetX, offsetY;
                Vector2 offset;
                // rejection sampling is apparently not a terrible way to do this
                do
                {
                    offsetX = Calc.Random.Range(-parent.ParticlePositionRadius, parent.ParticlePositionRadius);
                    offsetY = Calc.Random.Range(-parent.ParticlePositionRadius, parent.ParticlePositionRadius);
                    offset = new Vector2(offsetX, offsetY);
                } while (Vector2.Distance(Vector2.Zero, offset) > parent.ParticlePositionRadius);
                Position = parent.BoostField.Center + offset;
                Color = Calc.Random.Choose(colors);
                float speed = Calc.Random.Range(4f, 8f);
                Velocity = Calc.AngleToVector(Calc.Random.NextAngle(), speed);
                Duration = Calc.Random.Range(0.6f, 1.5f);
            }
        }
    }
}
