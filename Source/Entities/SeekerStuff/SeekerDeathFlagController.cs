using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SSMHelper.Entities.SeekerStuff
{
    [CustomEntity("SSMHelper/SeekerDeathFlagController")]
    [Tracked]
    public class SeekerDeathFlagController : Entity
    {
        public string Flag;

        public bool Activated => SceneAs<Level>().Session.GetFlag(Flag);

        public SeekerDeathFlagController(EntityData data, Vector2 offset) : base()
        {
            Flag = data.Attr("flag");
            if (string.IsNullOrEmpty(Flag))
            {
                Flag = $"{data.Level.Name}_seekers_dead";
            }
        }

        public override void Awake(Scene scene)
        {
            if (Activated)
            {
                foreach (Seeker seeker in scene.Tracker.GetEntities<Seeker>())
                {
                    seeker.RemoveSelf();
                }
            }
        }

        public override void Update()
        {
            Level level = SceneAs<Level>();
            if (level.Tracker.CountEntities<Seeker>() == 0)
            {
                level.Session.SetFlag(Flag);
                RemoveSelf();
            }
        }
    }
}
