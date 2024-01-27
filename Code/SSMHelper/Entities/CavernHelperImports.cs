using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.SSMHelper.Entities
{
    [ModImportName("CavernHelper")]
    public static class CavernHelperImports
    {
        public static Func<Action<Vector2>, Collider, Component> GetCrystalBombExplosionCollider;
        public static Func<Collider, Component> GetCrystalBombExploderCollider;
    }
}
