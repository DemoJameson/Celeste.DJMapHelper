module DJMapHelperColorfulRefill

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/colorfulRefill" Refill(x::Integer, y::Integer, oneUse::Bool=false, color::String="Red")
const colors = String[
    "Red",
    "Blue",
    "Black"
]

const placements = Ahorn.PlacementDict(
    "Refill (Red) (DJMapHelper)" => Ahorn.EntityPlacement(
        Refill,
    ),
    "Refill (Blue) (DJMapHelper)" => Ahorn.EntityPlacement(
        Refill,
        "point",
        Dict{String, Any}(
            "color" => "Blue"
        )
    ),
	"Refill (Black) (DJMapHelper)" => Ahorn.EntityPlacement(
        Refill,
        "point",
        Dict{String, Any}(
            "color" => "Black"
        )
    )
)

Ahorn.editingOptions(entity::Refill) = Dict{String, Any}(
    "color" => colors
)

function Ahorn.selection(entity::Refill)
    x, y = Ahorn.position(entity)
	color = get(entity.data, "color", false)
    if color == "Red"
        return Ahorn.getSpriteRectangle("objects/DJMapHelper/redRefill/idle00.png", x, y)
    elseif color == "Blue"
        return Ahorn.getSpriteRectangle("objects/DJMapHelper/blueRefill/idle00.png", x, y)
    else
        return Ahorn.getSpriteRectangle("objects/DJMapHelper/blackRefill/idle00.png", x, y)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Refill, room::Maple.Room)
	color = get(entity.data, "color", false)
    if color == "Red"
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/redRefill/idle00.png", 0, 0)
    elseif color == "Blue"
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/blueRefill/idle00.png", 0, 0)
    else
        Ahorn.drawSprite(ctx, "objects/DJMapHelper/blackRefill/idle00.png", 0, 0)
    end
end

end