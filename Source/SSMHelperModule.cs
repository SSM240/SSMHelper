using Microsoft.Xna.Framework;
using Monocle;
using System;
using Celeste.Mod.SSMHelper.Entities;
using Celeste.Mod.SSMHelper.Triggers;
using MonoMod.ModInterop;

namespace Celeste.Mod.SSMHelper
{
    public class SSMHelperModule : EverestModule
    {
        public static SSMHelperModule Instance;

        public SSMHelperModule()
        {
            Instance = this;

#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(SSMHelperModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(SSMHelperModule), LogLevel.Info);
#endif
        }

        public override void Load()
        {
            StarjumpTilesetHelper.Load();
            ReverseKillbox.Load();
            ResizableDashSwitch.Load();
            //ChangeMaxCassetteTrigger.Load();
            RedirectableBooster.Load();
            SeekerCrushBarrier.Load();
            SeekerCrushBarrierRenderer.Load();
            ToggleSwapBlock.Load();
            DashBoostField.Load();
            BarrierDashSwitch.Load();
            CrystalBombBadelineBoss.Load();
            HoldableDashTrigger.Load();
            ForceLightningLoadController.Load();
            SeekerSpawnFacingTrigger.Load();

            typeof(CavernHelperImports).ModInterop();
        }

        public override void Unload()
        {
            StarjumpTilesetHelper.Unload();
            ReverseKillbox.Unload();
            ResizableDashSwitch.Unload();
            //ChangeMaxCassetteTrigger.Unload();
            RedirectableBooster.Unload();
            SeekerCrushBarrier.Unload();
            SeekerCrushBarrierRenderer.Unload();
            ToggleSwapBlock.Unload();
            DashBoostField.Unload();
            BarrierDashSwitch.Unload();
            CrystalBombBadelineBoss.Unload();
            HoldableDashTrigger.Unload();
            ForceLightningLoadController.Unload();
            SeekerSpawnFacingTrigger.Unload();
        }

        public override void LoadContent(bool firstLoad)
        {
            base.LoadContent(firstLoad);
            
            RedirectableBooster.LoadParticles();
            ToggleSwapBlock.LoadParticles();
            ResizableDashSwitch.LoadParticles();
            DashBoostField.LoadParticles();
        }
    }
}
