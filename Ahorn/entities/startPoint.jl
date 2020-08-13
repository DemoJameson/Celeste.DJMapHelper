module DJMapHelperStartPoint

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/startPoint" Point(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Start Level Spawn Point (DJMapHelper)" => Ahorn.EntityPlacement(
        Point
    )
)

sprite = "characters/player/sitDown15.png"

function Ahorn.selection(entity::Point)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=1.0)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Point) = Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=1.0)

end