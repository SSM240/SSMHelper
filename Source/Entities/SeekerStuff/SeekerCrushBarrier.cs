using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/SeekerCrushBarrier")]
    public class SeekerCrushBarrier : SeekerBarrier
    {
        public static readonly Color FillColor = Calc.HexToColor("d365be");
        public static readonly Color ParticleColor = Calc.HexToColor("ff9ae2");

        public string Flag;
        public EntityID ID;

        private bool flagMode = false;
        private bool removing = false;
        private float removingFlashAlpha;
        private Color removingFlashColor;
        private bool particlesVisible = true;

        public SeekerCrushBarrier(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            Collidable = true;
            SurfaceSoundIndex = SurfaceIndex.DreamBlockActive;

            Flag = data.Attr("flag");
            flagMode = !string.IsNullOrEmpty(Flag);

            ID = id;

            OnDashCollide = OnDashed;
            Add(new ClimbBlocker(edge: true));
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Tracker.GetEntity<SeekerCrushBarrierRenderer>().Track(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Tracker.GetEntity<SeekerCrushBarrierRenderer>().Untrack(this);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (flagMode)
            {
                return;
            }
            if (scene.Tracker.CountEntities<Seeker>() == 0 || CollideCheck<Player>())
            {
                RemoveSelf();
            }
        }

        public override void Update()
        {
            base.Update();
            if (!removing)
            {
                Level level = SceneAs<Level>();
                if (flagMode && level.Session.GetFlag(Flag))
                {
                    removing = true;
                    Add(new Coroutine(RemovalRoutine()));
                    level.Session.DoNotLoad.Add(ID);
                }
                else if (!flagMode && level.Tracker.CountEntities<Seeker>() == 0)
                {
                    removing = true;
                    Add(new Coroutine(RemovalRoutine()));
                }
            }
        }


        public override void Render()
        {
            if (particlesVisible)
            {
                Color color = ParticleColor * 0.5f;
                foreach (Vector2 particle in particles)
                {
                    Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
                }
            }
            if (Flashing)
            {
                Draw.Rect(Collider, Color.Lerp(FillColor, Color.White, 0.5f) * Flash * 0.5f);
            }
            if (removing)
            {
                float alpha = removingFlashAlpha * 0.6f;
                if (Settings.Instance.DisableFlashes)
                {
                    alpha *= 0.33f;
                }
                Draw.Rect(Collider, removingFlashColor * alpha);
            }
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Console.WriteLine(direction);
            Flash = 1f;
            Flashing = true;
            Solidify = 1f;
            solidifyDelay = 1f;
            Audio.Play(SFX.game_03_forcefield_bump, Position);
            if (player.StateMachine.State == Player.StRedDash)
            {
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
                SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 8f, 48f, 0.4f, Ease.QuadOut, Ease.QuadOut);
                player.StateMachine.State = Player.StHitSquash;
            }
            return DashCollisionResults.Bounce;
        }

        private IEnumerator RemovalRoutine()
        {
            Solidify = 1f;
            solidifyDelay = 1f;
            removing = true;
            yield return FlashFadeIn();
            Collidable = false;
            SceneAs<Level>().Tracker.GetEntity<SeekerCrushBarrierRenderer>().Untrack(this);
            particlesVisible = false;
            yield return FlashFadeOut();
            RemoveSelf();
        }

        private IEnumerator FlashFadeIn()
        {
            Tween flashFadeIn = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeOut, 0.15f, true);
            flashFadeIn.OnStart = (t) =>
            {
                removingFlashColor = FillColor;
                removingFlashAlpha = 0f;
            };
            flashFadeIn.OnUpdate = (t) =>
            {
                removingFlashColor = Color.Lerp(FillColor, Color.Lerp(FillColor, Color.White, 0.5f), t.Eased);
                removingFlashAlpha = t.Eased;
            };
            Add(flashFadeIn);
            yield return flashFadeIn.Wait();
        }

        private IEnumerator FlashFadeOut()
        {
            Tween flashFadeOut = Tween.Create(Tween.TweenMode.Oneshot, Ease.Linear, 0.1f, true);
            flashFadeOut.OnStart = (t) =>
            {
                removingFlashColor = Color.Lerp(FillColor, Color.White, 0.5f);
                removingFlashAlpha = 1f;
            };
            flashFadeOut.OnUpdate = (t) =>
            {
                removingFlashAlpha = 1f - t.Eased;
            };
            Add(flashFadeOut);
            yield return flashFadeOut.Wait();
        }

        public static void Load()
        {
            On.Celeste.SeekerBarrierRenderer.Track += On_SeekerBarrierRenderer_Track;
            On.Celeste.SeekerBarrierRenderer.Untrack += On_SeekerBarrierRenderer_Untrack;
        }

        public static void Unload()
        {
            On.Celeste.SeekerBarrierRenderer.Track -= On_SeekerBarrierRenderer_Track;
            On.Celeste.SeekerBarrierRenderer.Untrack -= On_SeekerBarrierRenderer_Untrack;
        }

        // disable this from being tracked by the regular seeker barrier renderer
        private static void On_SeekerBarrierRenderer_Track(On.Celeste.SeekerBarrierRenderer.orig_Track orig,
            SeekerBarrierRenderer self, SeekerBarrier block)
        {
            if (block is SeekerCrushBarrier)
            {
                return;
            }
            orig(self, block);
        }
        private static void On_SeekerBarrierRenderer_Untrack(On.Celeste.SeekerBarrierRenderer.orig_Untrack orig,
            SeekerBarrierRenderer self, SeekerBarrier block)
        {
            if (block is SeekerCrushBarrier)
            {
                return;
            }
            orig(self, block);
        }
    }
}
