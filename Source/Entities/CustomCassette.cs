using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste.Mod.SSMHelper.Entities
{
    // mostly copypasted + cleaned up from vanilla cassette
    [CustomEntity("SSMHelper/CustomCassette")]
    public class CustomCassette : Entity
    {
        public bool IsGhost;

        private Sprite sprite;

        private SineWave hover;

        private BloomPoint bloom;

        private VertexLight light;

        private Wiggler scaleWiggler;

        private bool collected;

        private Vector2[] nodes;

        private EventInstance remixSfx;

        private bool collecting;

        private EntityID id;

        public CustomCassette(Vector2 position, Vector2[] nodes, EntityID id)
            : base(position)
        {
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            this.nodes = nodes;
            Add(new PlayerCollider(OnPlayer));
            this.id = id;
        }

        public CustomCassette(EntityData data, Vector2 offset, EntityID id)
            : this(data.Position + offset, data.NodesOffset(offset), id)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            IsGhost = SaveData.Instance.Areas_Safe[SceneAs<Level>().Session.Area.ID].Cassette;
            Add(sprite = GFX.SpriteBank.Create(IsGhost ? "cassetteGhost" : "cassette"));
            sprite.Play("idle");
            Add(scaleWiggler = Wiggler.Create(0.25f, 4f, (float f) =>
            {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));
            Add(bloom = new BloomPoint(0.25f, 16f));
            Add(light = new VertexLight(Color.White, 0.4f, 32, 64));
            Add(hover = new SineWave(0.5f, 0f));
            hover.OnUpdate = (float f) =>
            {
                Sprite obj = sprite;
                VertexLight vertexLight = light;
                float num2 = (bloom.Y = f * 2f);
                float y = (vertexLight.Y = num2);
                obj.Y = y;
            };
            if (IsGhost)
            {
                sprite.Color = Color.White * 0.8f;
            }
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(remixSfx);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(remixSfx);
        }

        public override void Update()
        {
            base.Update();
            if (!collecting && base.Scene.OnInterval(0.1f))
            {
                SceneAs<Level>().Particles.Emit(Cassette.P_Shine, 1, base.Center, new Vector2(12f, 10f));
            }
        }

        private void OnPlayer(Player player)
        {
            if (!collected)
            {
                player?.RefillStamina();
                Audio.Play("event:/game/general/cassette_get", Position);
                collected = true;
                Celeste.Freeze(0.1f);
                Add(new Coroutine(CollectRoutine(player)));
            }
        }

        private IEnumerator CollectRoutine(Player player)
        {
            collecting = true;
            Level level = Scene as Level;
            CassetteBlockManager cbm = Scene.Tracker.GetEntity<CassetteBlockManager>();
            level.PauseLock = true;
            level.Frozen = true;
            Tag = Tags.FrozenUpdate;
            //level.Session.Cassette = true;  // do not
            level.Session.RespawnPoint = level.GetSpawnPoint(nodes[1]);
            level.Session.UpdateLevelStartDashes();
            SaveData.Instance.RegisterCassette(level.Session.Area);
            level.Session.DoNotLoad.Add(id);
            //cbm?.StopBlocks();  // do not
            Depth = -1000000;
            level.Shake();
            level.Flash(Color.White);
            level.Displacement.Clear();
            Vector2 camWas = level.Camera.Position;
            Vector2 camTo = (Position - new Vector2(160f, 90f)).Clamp(level.Bounds.Left - 64, level.Bounds.Top - 32, level.Bounds.Right + 64 - 320, level.Bounds.Bottom + 32 - 180);
            level.Camera.Position = camTo;
            level.ZoomSnap((Position - level.Camera.Position).Clamp(60f, 60f, 260f, 120f), 2f);
            sprite.Play("spin", restart: true);
            sprite.Rate = 2f;
            for (float p3 = 0f; p3 < 1.5f; p3 += Engine.DeltaTime)
            {
                sprite.Rate += Engine.DeltaTime * 4f;
                yield return null;
            }
            sprite.Rate = 0f;
            sprite.SetAnimationFrame(0);
            scaleWiggler.Start();
            yield return 0.25f;
            Vector2 from = Position;
            Vector2 to = new Vector2(X, level.Camera.Top - 16f);
            float duration2 = 0.4f;
            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
            {
                sprite.Scale.X = MathHelper.Lerp(1f, 0.1f, p3);
                sprite.Scale.Y = MathHelper.Lerp(1f, 3f, p3);
                Position = Vector2.Lerp(from, to, Ease.CubeIn(p3));
                yield return null;
            }
            Visible = false;
            remixSfx = Audio.Play("event:/game/general/cassette_preview", "remix", level.Session.Area.ID);
            Cassette.UnlockedBSide message = new Cassette.UnlockedBSide();
            Scene.Add(message);
            yield return message.EaseIn();
            while (!Input.MenuConfirm.Pressed)
            {
                yield return null;
            }
            Audio.SetParameter(remixSfx, "end", 1f);
            yield return message.EaseOut();
            duration2 = 0.25f;
            Add(new Coroutine(level.ZoomBack(duration2 - 0.05f)));
            for (float p3 = 0f; p3 < 1f; p3 += Engine.DeltaTime / duration2)
            {
                level.Camera.Position = Vector2.Lerp(camTo, camWas, Ease.SineInOut(p3));
                yield return null;
            }
            if (!player.Dead && nodes != null && nodes.Length >= 2)
            {
                Audio.Play("event:/game/general/cassette_bubblereturn", level.Camera.Position + new Vector2(160f, 90f));
                player.StartCassetteFly(nodes[1], nodes[0]);
            }
            foreach (SandwichLava item in level.Entities.FindAll<SandwichLava>())
            {
                item.Leave();
            }
            level.Frozen = false;
            yield return 0.25f;
            //cbm?.Finish();  // do not
            level.PauseLock = false;
            level.ResetZoom();
            RemoveSelf();
        }
    }
}
