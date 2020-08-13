module DJMapHelperColorfulFlyFeather

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/colorfulFlyFeather" Feather(x::Integer, y::Integer, shielded::Bool=false, singleUse::Bool=false, color::String="Blue")

const colors = String[
    "Blue",
    "Green",
    "Red"
]

const placements = Ahorn.PlacementDict(
    "Blue Feather (DJMapHelper)" => Ahorn.EntityPlacement(
        Feather
    ),
    "Green Feather (DJMapHelper)" => Ahorn.EntityPlacement(
        Feather,
        "point",
        Dict{String, Any}(
            "color" => "Green"
        )
    ),
    "Red Feather (DJMapHelper)" => Ahorn.EntityPlacement(
        Feather,
        "point",
        Dict{String, Any}(
            "color" => "Red"
        )
    )
)

Ahorn.editingOptions(entity::Feather) = Dict{String, Any}(
    "color" => colors
)

function Ahorn.selection(entity::Feather)
    x, y = Ahorn.position(entity)
    shielded = get(entity.data, "shielded", false)
    
    return shielded ? Ahorn.Rectangle(x - 12, y - 12, 24, 24) : Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Feather, room::Maple.Room)
    if get(entity.data, "shielded", false)
        Ahorn.Cairo.save(ctx)

        Ahorn.set_antialias(ctx, 1)
        Ahorn.set_line_width(ctx, 1);

        Ahorn.drawCircle(ctx, 0, 0, 12, (1.0, 1.0, 1.0, 1.0))

        Ahorn.Cairo.restore(ctx)
    end

    color = get(entity.data, "color", false)
    if color == "Blue"
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/blueFlyFeather/idle00.png", 0, 0)
    elseif color == "Green"
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/greenFlyFeather/idle00.png", 0, 0)
    else
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/redFlyFeather/idle00.png", 0, 0)
    end
end

end