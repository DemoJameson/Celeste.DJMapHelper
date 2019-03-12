using Microsoft.Xna.Framework;
namespace Celeste.Mod.DJMapHelper.Triggers
{
    public class ColorGradeTrigger:Trigger
    {
        private string colorGrade;
        public ColorGradeTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            colorGrade = data.Attr("ColorGrade", "Null");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (colorGrade == "Null") colorGrade = null;
            Level scene = Scene as Level;
            scene.SnapColorGrade(colorGrade);
        }
    }
}