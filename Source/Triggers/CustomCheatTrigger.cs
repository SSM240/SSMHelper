using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.SSMHelper.Triggers
{
    [CustomEntity("SSMHelper/CustomCheatTrigger")]
    public class CustomCheatTrigger : Trigger
    {
        public string Code;
        public string SoundEffect;
        public string Flag;
        public bool EnableFlag;
        public bool IgnoreDirections;
        public bool ResetOnLeave;
        
        private CustomCheatListener cheatListener;

        public CustomCheatTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Code = data.Attr("code", "lrjGuudlGc");
            SoundEffect = data.Attr("soundEffect", "game_06_feather_bubble_get");
            Flag = data.Attr("flag", "SSMCheatCode");
            EnableFlag = data.Bool("enableFlag", true);
            IgnoreDirections = data.Bool("ignoreDirections", false);

            cheatListener = new CustomCheatListener(Code, CodeEntered, IgnoreDirections);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(cheatListener);
        }

        public override void Update()
        {
            base.Update();
            if (!PlayerIsInside)
            {
                cheatListener.CurrentCheatInput = "";
            }
            cheatListener.Active = PlayerIsInside && SceneAs<Level>().Session.GetFlag(Flag) != EnableFlag;
        }

        public void CodeEntered()
        {
            SceneAs<Level>().Session.SetFlag(Flag, EnableFlag);
            if (!string.IsNullOrEmpty(SoundEffect))
            {
                Audio.Play(SFX.EventnameByHandle(SoundEffect));
            }
        }
    }
}
