using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities.SeekerStuff
{
    [CustomEntity("SSMHelper/SeekerFakeWall")]
    [TrackedAs(typeof(FakeWall))]
    public class SeekerFakeWall : FakeWall
    {
        public SeekerFakeWall(EntityData data, Vector2 offset, EntityID id)
            : base(id, data, offset, Modes.Wall)
        {
        }

        public override void Update()
        {
            base.Update();

            if (fade) return;

            foreach (Seeker seeker in CollideAll<Seeker>())
            {
                if (seeker.State != Seeker.StIdle)
                {
                    SceneAs<Level>().Session.DoNotLoad.Add(eid);
                    fade = true;
                    Audio.Play(SFX.game_gen_secret_revealed, Center);
                }
            }
        }
    }
}
