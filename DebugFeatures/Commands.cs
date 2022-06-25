using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugFeatures; 

// ReSharper disable once UnusedMember.Global
public static class Commands {
    [Command("metadata", "check some metadata values (DJ Map Helper)")]
    private static void CmdMetadata() {
        if (Engine.Scene is not Level level) {
            return;
        }

        Engine.Commands.Log($"color grade ({level.Session.ColorGrade})");
        Engine.Commands.Log($"lighting base ({level.BaseLightingAlpha}), session lighting add ({level.Session.LightingAlphaAdd}), current lighting ({level.Lighting.Alpha})");
        Engine.Commands.Log($"bloom base ({(level.Bloom.Base - level.Session.BloomBaseAdd)}), session bloom base add ({level.Session.BloomBaseAdd}), current bloom base ({level.Bloom.Base})");
        Engine.Commands.Log($"bloom strength ({level.Bloom.Strength})");
    }
}