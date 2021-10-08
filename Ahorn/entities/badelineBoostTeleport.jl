module DJMapHelperBadelineBoostTeleport

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/badelineBoostTeleport" TeleportBadelineBoost(x::Integer, y::Integer, DefaultRoom::String="", DefaultColorGrade::String="", DefaultSpawnPointX::Int=0, DefaultSpawnPointY::Int=0, KeyRoom::String="", KeyColorGrade::String="", KeySpawnPointX::Int=0, KeySpawnPointY::Int=0, GoldenRoom::String="", GoldenColorGrade::String="", GoldenSpawnPointX::Int=0, GoldenSpawnPointY::Int=0, MoonRoom::String="", MoonColorGrade::String="", MoonSpawnPointX::Int=0, MoonSpawnPointY::Int=0, Priority::String="Moon -> Golden -> Key", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const priorities = String[
    "Moon -> Golden -> Key",
    "Moon -> Key -> Golden",
    "Golden -> Moon -> Key",
    "Golden -> Key -> Moon",
    "Key -> Moon -> Golden",
    "Key -> Golden -> Moon"
]

const placements = Ahorn.PlacementDict(
    "Badeline Boost Teleport (DJMapHelper)" => Ahorn.EntityPlacement(
        TeleportBadelineBoost
    ),
)

Ahorn.editingOptions(entity::TeleportBadelineBoost) = Dict{String, Any}(
    "Priority" => priorities
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