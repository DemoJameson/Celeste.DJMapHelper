module DJMapHelperAngryOshiroRight

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/oshiroBossRight" OshiroRight(x::Integer, y::Integer, fromCutscene::Bool=false)
const placements = Ahorn.PlacementDict(
    "Oshiro Boss Right (DJMapHelper)" => Ahorn.EntityPlacement(
		OshiroRight
    )
)

sprite = "characters/oshiro/boss13.png"

function Ahorn.selection(entity::OshiroRight)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OshiroRight, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=0.5, sx=-1)

end