module BadelineBoostDown

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/badelineBoostDown" DownBadelineBoost(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Badeline Boost Down (DJMapHelper)" => Ahorn.EntityPlacement(
        DownBadelineBoost
    ),
)

Ahorn.nodeLimits(entity::DownBadelineBoost) = 0, -1

sprite = "objects/badelineboost/idle00.png"

function Ahorn.selection(entity::DownBadelineBoost)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::DownBadelineBoost)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::DownBadelineBoost, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

end