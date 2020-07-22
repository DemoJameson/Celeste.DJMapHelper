using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.DJMapHelper.Triggers {
    [CustomEntity("DJMapHelper/colorGradeTrigger")]
    public class ColorGradeTrigger : Trigger {
        private string colorGrade;

        public ColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGrade = data.Attr("ColorGrade", "none");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (colorGrade == "none") {
                colorGrade = null;
            }

            (Scene as Level)?.SnapColorGrade(colorGrade);
        }
    }
}