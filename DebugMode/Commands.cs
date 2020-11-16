using Monocle;

namespace Celeste.Mod.DJMapHelper.DebugMode {
    // ReSharper disable once UnusedMember.Global
    public static class Commands {
        [Command("metadata", "check metadata values(DJ Map Helper)")]
        private static void CmdMetadata() {
            Level scene = Engine.Scene as Level;
            if (scene == null) {
                return;
            }

            Engine.Commands.Log("color grade (" + scene.Session.ColorGrade + ")");
            Engine.Commands.Log("base lighting (" + scene.BaseLightingAlpha + "), session lighting add(" +
                                scene.Session.LightingAlphaAdd + "), current lighting (" +
                                scene.Lighting.Alpha + ")");
            Engine.Commands.Log("base bloom (" + (scene.Bloom.Base - scene.Session.BloomBaseAdd) +
                                "), session bloom add(" +
                                scene.Session.BloomBaseAdd + "), current bloom (" +
                                scene.Bloom.Base + ")");
        }
    }
}