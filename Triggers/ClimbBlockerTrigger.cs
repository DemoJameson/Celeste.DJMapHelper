using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [Tracked]
    public class ClimbBlockerTrigger : Trigger {
        private readonly bool climb;
        private readonly bool wallJump;

        public ClimbBlockerTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            wallJump = data.Bool("wallJump");
            climb = data.Bool("climb");
        }

        public static void OnLoad() {
            On.Celeste.Player.ClimbBoundsCheck += PlayerOnClimbBoundsCheck;
            On.Celeste.Player.ClimbJump += PlayerOnClimbJump;
        }

        public static void OnUnLoad() {
            On.Celeste.Player.ClimbBoundsCheck -= PlayerOnClimbBoundsCheck;
            On.Celeste.Player.ClimbJump -= PlayerOnClimbJump;
        }

        private static void PlayerOnClimbJump(On.Celeste.Player.orig_ClimbJump orig, Player self) {
            var triggers =
                self.CollideAll<ClimbBlockerTrigger>().Cast<ClimbBlockerTrigger>().ToList();
            var wallJump = triggers.All(trigger => trigger.wallJump);
            if (wallJump) orig(self);
        }

        private static bool PlayerOnClimbBoundsCheck(On.Celeste.Player.orig_ClimbBoundsCheck orig, Player self,
            int dir) {
            var triggers =
                self.CollideAll<ClimbBlockerTrigger>().Cast<ClimbBlockerTrigger>().ToList();
            var wallJump = triggers.All(trigger => trigger.wallJump);
            var climb = triggers.All(trigger => trigger.climb);

            if (wallJump && climb)
                return orig(self, dir);
            if (!wallJump && climb && !Input.Jump.Check && Input.Grab.Check)
                return orig(self, dir);
            if (wallJump && Input.Jump.Check && !Input.Grab.Check) return orig(self, dir);

            return false;
        }
    }
}