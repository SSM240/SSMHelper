using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/ForceLightningLoadController")]
    [Tracked]
    public class ForceLightningLoadController : Entity
    {
        public ForceLightningLoadController()
        {
        }

        public static void Load()
        {
            On.Celeste.Lightning.InView += On_Lightning_InView;
            On.Celeste.LightningRenderer.Edge.InView += On_Edge_InView;
        }

        public static void Unload()
        {
            On.Celeste.Lightning.InView -= On_Lightning_InView;
            On.Celeste.LightningRenderer.Edge.InView -= On_Edge_InView;
        }

        private static bool On_Lightning_InView(On.Celeste.Lightning.orig_InView orig, Lightning self)
        {
            if (self.Scene.Tracker.GetEntity<ForceLightningLoadController>() != null)
            {
                return true;
            }
            return orig(self);
        }

        private static bool On_Edge_InView(On.Celeste.LightningRenderer.Edge.orig_InView orig, object self, ref Rectangle view)
        {
            if ((self as LightningRenderer.Edge).Parent.Scene.Tracker.GetEntity<ForceLightningLoadController>() != null)
            {
                return true;
            }
            return orig(self, ref view);
        }
    }
}
