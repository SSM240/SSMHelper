﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/DashBoostField")]
    [Tracked]
    public class DashBoostField : Entity
    {
        public Color Color;
        public bool PreserveDash;
        public float DashSpeedMult;
        public float TargetTimeRateMult;
        public float Radius;

        private Image boostFieldTexture;

        public const string DefaultColor = "ffffff";
        public const float DefaultDashSpeedMult = 1.7f;
        public const float DefaultTimeRateMult = 0.65f;
        public const float DefaultRadius = 1.5f;
        public const bool DefaultPreserveDash = true;

        public static ParticleType P_RedRefill;
        public static float CurrentTimeRateMult = 1f;

        private static BindingFlags privateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private static MethodInfo dashCoroutineInfo = typeof(Player).GetMethod("DashCoroutine", privateInstance).GetStateMachineTarget();
        private static ILHook dashCoroutineHook;

        // for some reason the default Center isn't actually the exact center
        public new Vector2 Center => Position - new Vector2(0.5f, 0.5f);

        public DashBoostField(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Color = Calc.HexToColor(data.Attr("color", DefaultColor));
            DashSpeedMult = data.Float("dashSpeedMultiplier", DefaultDashSpeedMult);
            TargetTimeRateMult = data.Float("timeRateMultiplier", DefaultTimeRateMult);
            Radius = data.Float("radius", DefaultRadius) * 8;
            PreserveDash = data.Bool("preserve", DefaultPreserveDash);

            Depth = Depths.Above;
            Collider = new Circle(Radius);
            Add(boostFieldTexture = new Image(GFX.Game[$"objects/SSMHelper/dashBoostField/white"]));
            boostFieldTexture.Color = Color;
            boostFieldTexture.CenterOrigin();
            Color lightColor = Color.Lerp(Color, Color.White, 0.9f);
            int startFade = (int)(Radius + 5f);
            int endFade = (int)(Radius + 5f);
            Add(new VertexLight(new Vector2(-0.5f, -0.5f), lightColor, 1f, startFade, endFade));
            DashBoostFieldParticleRenderer particleRenderer = new DashBoostFieldParticleRenderer();
            Add(particleRenderer);

            Tag = Tags.TransitionUpdate;
            Add(new TransitionListener
            {
                OnInBegin = () => Active = particleRenderer.Active = false,
                OnInEnd = () => Active = particleRenderer.Active = true,
                OnOutBegin = () => Active = particleRenderer.Active = false
            });
        }

        public static void Load()
        {
            IL.Celeste.Level.Update += IL_Level_Update;
            On.Celeste.Player.Update += On_Player_Update;
            On.Celeste.Player.UnderwaterMusicCheck += On_Player_UnderwaterMusicCheck;
            On.Celeste.Player.DashBegin += On_Player_DashBegin;
            dashCoroutineHook = new ILHook(dashCoroutineInfo, IL_Player_DashCoroutine);
            IL.Celeste.Player.SuperWallJump += IL_Player_SuperWallJump;
            IL.Celeste.Player.SuperJump += IL_Player_SuperJump;
            IL.Celeste.Player.DreamDashBegin += IL_Player_DreamDashBegin;
            On.Celeste.Player.Die += On_Player_Die;
        }

        public static void Unload()
        {
            IL.Celeste.Level.Update -= IL_Level_Update;
            On.Celeste.Player.Update -= On_Player_Update;
            On.Celeste.Player.UnderwaterMusicCheck -= On_Player_UnderwaterMusicCheck;
            On.Celeste.Player.DashBegin -= On_Player_DashBegin;
            dashCoroutineHook?.Dispose();
            IL.Celeste.Player.SuperWallJump -= IL_Player_SuperWallJump;
            IL.Celeste.Player.SuperJump -= IL_Player_SuperJump;
            IL.Celeste.Player.DreamDashBegin -= IL_Player_DreamDashBegin;
            On.Celeste.Player.Die -= On_Player_Die;
        }

        public static void LoadParticles()
        {
            P_RedRefill = new ParticleType(Refill.P_Shatter)
            {
                Color = Calc.HexToColor("ffb0b0"),
                Color2 = Calc.HexToColor("ffd8d8"),
            };
        }

        private static float ModifyTimeRate(float timeRate, Level level)
        {
            if (!level.Paused)
            {
                timeRate *= CurrentTimeRateMult;
            }
            return timeRate;
        }

        private static void IL_Level_Update(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            // modify TimeRateB so we get audio slowing
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdcR4(10f),
                instr => instr.MatchDiv()))
            {

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate(ModifyTimeRate);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "SSMHelper", "Could not hook Level.Update!");
            }
        }

        private static bool IsDashingOrRespawning(Player player)
        {
            int state = player.StateMachine.State;
            return state == Player.StDash || state == Player.StIntroRespawn;
        }

        private static void On_Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            bool wasDashing = self.DashAttacking || self.StateMachine.State == Player.StDash;
            orig(self);
            // having the player itself handle collision is nicer
            DashBoostField boostField = self.CollideFirst<DashBoostField>();
            if (!self.Dead && boostField != null && boostField.Active && !IsDashingOrRespawning(self))
                CurrentTimeRateMult = boostField.TargetTimeRateMult;
            else
                CurrentTimeRateMult = 1f;

            // the last dash has ended so this should definitely be reset
            if (wasDashing && !(self.DashAttacking || self.StateMachine.State == Player.StDash))
            {
                DynamicData.For(self).Set("ssmhelper_dashBoosted", false);
            }
        }

        private static bool On_Player_UnderwaterMusicCheck(On.Celeste.Player.orig_UnderwaterMusicCheck orig, Player self)
        {
            if (self.CollideCheck<DashBoostField>() && !IsDashingOrRespawning(self))
                return true;
            return orig(self);
        }

        private static void On_Player_DashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
        {
            DynamicData playerData = DynamicData.For(self);
            DashBoostField boostField = self.CollideFirst<DashBoostField>();
            if (boostField != null && boostField.Active)
            {
                playerData.Set("ssmhelper_dashBoosted", true);
                playerData.Set("ssmhelper_dashBoostSpeed", boostField.DashSpeedMult);
                self.Add(new Coroutine(RefillDashIfSelected(self)));
            }
            else
            {
                // just to be on the safe side
                playerData.Set("ssmhelper_dashBoosted", false);
            }
            orig(self);
        }

        private static T SafeGet<T>(DynamicData data, string key, T defaultValue = default) where T : struct
        {
            T? value = data.Get<T?>(key);
            if (value != null)
            {
                return (T)value;
            }
            else
            {
                return defaultValue;
            }
        }

        private static float ModifySpeed(float speed, Player player)
        {
            DynamicData playerData = DynamicData.For(player);
            if (SafeGet(playerData, "ssmhelper_dashBoosted", defaultValue: false))
            {
                speed *= SafeGet(playerData, "ssmhelper_dashBoostSpeed", defaultValue: 1f);
            }
            return speed;
        }

        private static Vector2 ModifySpeed(Vector2 speed, Player player)
        {
            DynamicData playerData = DynamicData.For(player);
            if (SafeGet(playerData, "ssmhelper_dashBoosted", defaultValue: false))
            {
                speed *= SafeGet(playerData, "ssmhelper_dashBoostSpeed", defaultValue: 1f);
            }
            return speed;
        }

        private static void IL_Player_DashCoroutine(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            FieldInfo f_this = dashCoroutineInfo.DeclaringType.GetField("<>4__this");
            // right as Speed is set for the first time
            if (cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld<Player>("Speed")))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, f_this);
                cursor.EmitDelegate<Func<Vector2, Player, Vector2>>(ModifySpeed);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "SSMHelper", "Could not hook Player.DashCoroutine!");
            }
        }

        private static IEnumerator RefillDashIfSelected(Player player)
        {
            DashBoostField boostField = player.CollideFirst<DashBoostField>();
            if (boostField?.PreserveDash == true)
            {
                // wait a frame so that the player's speed will be correct
                yield return null;
                player.Dashes += 1;
                Audio.Play(SFX.game_gen_diamond_touch);
                Level level = player.SceneAs<Level>();
                float angle = player.Speed.Angle();
                level.ParticlesFG.Emit(P_RedRefill, 5, player.Position, Vector2.One * 4f, angle - (float)Math.PI / 2f);
                level.ParticlesFG.Emit(P_RedRefill, 5, player.Position, Vector2.One * 4f, angle + (float)Math.PI / 2f);
            }
        }

        private static void IL_Player_SuperWallJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            // uncomment to boost X value as well
            // if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(170f))) {
            //     cursor.Emit(OpCodes.Ldarg_0);
            //     cursor.EmitDelegate<Func<float, Player, float>>(ModifySpeed);
            // }
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(-160f)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>(ModifySpeed);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "SSMHelper", "Could not hook Player.SuperWallJump!");
            }
        }

        private static void IL_Player_SuperJump(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(260f)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>(ModifySpeed);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "SSMHelper", "Could not hook Player.SuperJump!");
            }
        }

        private static void IL_Player_DreamDashBegin(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(240f)))
            {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>(ModifySpeed);
            }
            else
            {
                Logger.Log(LogLevel.Warn, "SSMHelper", "Could not hook Player.DreamDashBegin!");
            }
        }

        private static PlayerDeadBody On_Player_Die(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            CurrentTimeRateMult = 1f;
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }
    }
}
