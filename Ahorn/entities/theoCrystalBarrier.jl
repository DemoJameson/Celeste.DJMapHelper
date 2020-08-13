module DJMapHelperTheoCrystalBarrier

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/theoCrystalBarrier" TheoBarrier(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Theo Barrier (DJMapHelper)" => Ahorn.EntityPlacement(
        TheoBarrier,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::TheoBarrier) = 8, 8
Ahorn.resizable(entity::TheoBarrier) = true, true

function Ahorn.selection(entity::TheoBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TheoBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.25, 0.50, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end