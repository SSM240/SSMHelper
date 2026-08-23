using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.SSMHelper
{
    [ModImportName("GravityHelper")]
    public static class GravityHelperImports
    {
        public static Func<bool> IsPlayerInverted;
    }
}
