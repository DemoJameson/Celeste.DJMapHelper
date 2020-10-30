module DJMapHelperShield

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/shield" Shield(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Shield (DJMapHelper)" => Ahorn.EntityPlacement(
        Shield
    )
)

function Ahorn.selection(entity::Shield)
    x, y = Ahorn.position(entity)
    
    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Shield, room::Maple.Room)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.drawCircle(ctx, 0, 0, 12, (1.0, 1.0, 1.0, 1.0))

    Ahorn.Cairo.restore(ctx)

end

end