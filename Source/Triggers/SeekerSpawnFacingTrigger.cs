using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.SSMHelper.Triggers
{
    [CustomEntity("SSMHelper/SeekerSpawnFacingTrigger")]
    [Tracked]
    public class SeekerSpawnFacingTrigger : Trigger
    {
        private Facings facing;

        public SeekerSpawnFacingTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
        {
            facing = data.Enum("facing", (Facings)0);
        }

        public static void Load()
        {
            On.Celeste.Seeker.Awake += On_Seeker_Awake;
        }

        public static void Unload()
        {
            On.Celeste.Seeker.Awake -= On_Seeker_Awake;
        }

        private static void On_Seeker_Awake(On.Celeste.Seeker.orig_Awake orig, Seeker self, Scene scene)
        {
            orig(self, scene);
            if (self.CollideFirst<SeekerSpawnFacingTrigger>() is { } trigger)
            {
                self.SnapFacing((float)trigger.facing);
            }
        }
    }
}
