module DJMapHelperFeatherBarrier

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/featherBarrier" Barrier(x::Integer, y::Integer, shielded::Bool=false, singleUse::Bool=false, color::String="Blue")

const colors = String[
    "Blue",
    "Green",
    "Red",
    "Yellow"
]

const placements = Ahorn.PlacementDict(
    "Blue Feather Barrier (DJMapHelper)" => Ahorn.EntityPlacement(
        Barrier,
        "rectangle"
    ),
    "Green Feather Barrier (DJMapHelper)" => Ahorn.EntityPlacement(
        Barrier,
        "rectangle",
        Dict{String, Any}(
            "color" => "Green"
        )
    ),
    "Red Feather Barrier (DJMapHelper)" => Ahorn.EntityPlacement(
        Barrier,
        "rectangle",
        Dict{String, Any}(
            "color" => "Red"
        )
    ),
    "Yellow Feather Barrier (DJMapHelper)" => Ahorn.EntityPlacement(
        Barrier,
        "rectangle",
        Dict{String, Any}(
            "color" => "Yellow"
        )
    )
)

Ahorn.minimumSize(entity::Barrier) = 8, 8
Ahorn.resizable(entity::Barrier) = true, true

Ahorn.editingOptions(entity::Barrier) = Dict{String, Any}(
    "color" => feather_colors
)

function Ahorn.selection(entity::Barrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Barrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    color = get(entity.data, "color", false)
    if color == "Blue"
        Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.25, 0.25, 0.75, 0.8), (0.0, 0.0, 0.0, 0.0))
    elseif color == "Green"
        Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.25, 0.75, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
    elseif color == "Red"
        Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.75, 0.25, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
    else
        Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.75, 0.75, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
    end
end

end