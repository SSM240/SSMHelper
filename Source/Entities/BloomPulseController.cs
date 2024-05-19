using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/BloomPulseController")]
    [Tracked]
    public class BloomPulseController : Entity
    {
        public bool ModifyBloomBase;
        public float BloomBaseFrom;
        public float BloomBaseTo;

        public bool ModifyBloomStrength;
        public float BloomStrengthFrom;
        public float BloomStrengthTo;

        public float Duration;
        public Tween.TweenMode TweenMode;
        public Ease.Easer Easer;

        private Tween tween;

        // taken from frost helper
        // https://github.com/JaThePlayer/FrostHelper/blob/31e7376d42d7cbc40e9d565780b16ffafe85bf80/Code/FrostHelper/Helpers/EaseHelper.cs#L75-L78
        private static readonly Dictionary<string, Ease.Easer> easerDict =
            typeof(Ease).GetFields(BindingFlags.Static | BindingFlags.Public)
            .Where(f => f.FieldType == typeof(Ease.Easer))
            .ToDictionary(f => f.Name, f => (Ease.Easer)f.GetValue(null), StringComparer.OrdinalIgnoreCase);

        public BloomPulseController(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            ModifyBloomBase = data.Bool("modifyBloomBase", false);
            BloomBaseFrom = data.Float("bloomBaseFrom");
            BloomBaseTo = data.Float("bloomBaseTo");

            ModifyBloomStrength = data.Bool("modifyBloomStrength", true);
            BloomStrengthFrom = data.Float("bloomStrengthFrom");
            BloomStrengthTo = data.Float("bloomStrengthTo");

            TweenMode = data.Enum("tweenMode", Tween.TweenMode.YoyoLooping);
            Duration = data.Float("duration");
            if (TweenMode == Tween.TweenMode.YoyoLooping)
            {
                Duration *= 0.5f;
            }

            string easerStr = data.Attr("easer", "Linear");
            if (easerDict.TryGetValue(easerStr, out Ease.Easer easer))
            {
                Easer = easer;
            }
            else
            {
                throw new ArgumentException($"'{easerStr}' is not a valid easer.");
            }

            Tag = Tags.TransitionUpdate;
            if (data.Bool("persistent", false))
            {
                Tag |= Tags.Persistent;
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            // remove self if entering a new room with a controller
            // this is so persistent ones don't build up forever
            Add(new TransitionListener
            {
                OnOutBegin = () =>
                {
                    if (Scene.Tracker.CountEntities<BloomPulseController>() > 1)
                    {
                        RemoveSelf();
                    }
                }
            });

            tween = Tween.Create(TweenMode, Easer, Duration);
            tween.OnUpdate = (t) =>
            {
                Level level = SceneAs<Level>();
                if (ModifyBloomBase)
                {
                    level.Bloom.Base = MathHelper.Lerp(BloomBaseFrom, BloomBaseTo, t.Eased);
                }
                if (ModifyBloomStrength)
                {
                    level.Bloom.Strength = MathHelper.Lerp(BloomStrengthFrom, BloomStrengthTo, t.Eased);
                }
            };
            tween.Start();
            Add(tween);
        }
    }
}
