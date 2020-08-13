module DJMapHelperBadelineBoostTeleport

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/badelineBoostTeleport" TeleportBadelineBoost(x::Integer, y::Integer, Room::String="", ColorGrade::String="", KeyRoom::String="", KeyColorGrade::String="", GoldenRoom::String="", GoldenColorGrade::String="", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], KeyFirst::Bool=false)

const placements = Ahorn.PlacementDict(
    "Badeline Boost Teleport (DJMapHelper)" => Ahorn.EntityPlacement(
        TeleportBadelineBoost
    ),
)

Ahorn.nodeLimits(entity::TeleportBadelineBoost) = 0, -1

sprite = "objects/badelineboost/idle00.png"

function Ahorn.selection(entity::TeleportBadelineBoost)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TeleportBadelineBoost)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TeleportBadelineBoost, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y)
end

end