using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DJMapHelper.Triggers;

[Tracked]
[CustomEntity("DJMapHelper/climbBlockerTrigger")]
public class ClimbBlockerTrigger : Trigger {
    private const string BlockWallJumpKey = "DJMapHelper/ClimbBlockerTrigger_BlockWallJump";
    private const string BlockClimbKey = "DJMapHelper/ClimbBlockerTrigger_BlockClimb";

    private readonly bool climb;
    private readonly bool wallJump;
    private readonly Modes mode;

    enum Modes {
        Contained,
        Persistent
    }

    public ClimbBlockerTrigger(EntityData data, Vector2 offset) : base(data, offset) {
        wallJump = data.Bool("wallJump");
        climb = data.Bool("climb");
        mode = data.Enum<Modes>("mode");
    }

    public override void OnEnter(Player player) {
        if (mode == Modes.Persistent) {
            Session session = player.SceneAs<Level>().Session;
            session.SetFlag(BlockWallJumpKey, !wallJump);
            session.SetFlag(BlockClimbKey, !climb);
            RemoveSelf();
        }
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
        var triggers = self.CollideAll<ClimbBlockerTrigger>();
        bool wallJump;
        if (triggers.Count > 0) {
            wallJump = triggers.All(trigger => ((ClimbBlockerTrigger)trigger).wallJump);
        } else {
            Session session = self.SceneAs<Level>().Session;
            wallJump = !session.GetFlag(BlockWallJumpKey);
        }
        if (wallJump) {
            orig(self);
        }
    }

    private static bool PlayerOnClimbBoundsCheck(On.Celeste.Player.orig_ClimbBoundsCheck orig, Player self,
        int dir) {
        var triggers = self.CollideAll<ClimbBlockerTrigger>();

        bool wallJump;
        bool climb;

        Session session = self.SceneAs<Level>().Session;
        if (triggers.Count > 0) {
            wallJump = triggers.All(trigger => ((ClimbBlockerTrigger)trigger).wallJump);
            climb = triggers.All(trigger => ((ClimbBlockerTrigger)trigger).climb);
        } else {
            wallJump = !session.GetFlag(BlockWallJumpKey);
            climb = !session.GetFlag(BlockClimbKey);
        }

        if (wallJump && climb) {
            return orig(self, dir);
        }

        if (!wallJump && climb && !Input.Jump.Check && Input.Grab.Check) {
            return orig(self, dir);
        }

        if (wallJump && Input.Jump.Check && !Input.Grab.Check) {
            return orig(self, dir);
        }

        return false;
    }
}