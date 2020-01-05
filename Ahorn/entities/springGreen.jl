module SpringGreen

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/springGreen" springGreen(x::Integer, y::Integer, orientation::String="Floor")
const orientations = String[
    "Floor",
    "WallLeft",
    "WallRight"
]

const placements = Ahorn.PlacementDict(
    "SpringGreen (Up) (DJMapHelper)" => Ahorn.EntityPlacement(
        springGreen,
    ),
    "SpringGreen (Left) (DJMapHelper)" => Ahorn.EntityPlacement(
        springGreen,
        "point",
        Dict{String, Any}(
            "orientation" => "WallLeft"
        )
    ),
	"SpringGreen (Right) (DJMapHelper)" => Ahorn.EntityPlacement(
        springGreen,
        "point",
        Dict{String, Any}(
            "orientation" => "WallRight"
        )
    )
)

Ahorn.editingOptions(entity::springGreen) = Dict{String, Any}(
    "orientation" => orientations
)

function Ahorn.selection(entity::springGreen)
    x, y = Ahorn.position(entity)
	orientation = get(entity.data, "orientation", false)
    if orientation == "Floor"
        return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
    elseif orientation == "WallLeft"
        return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
    else
        return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::springGreen, room::Maple.Room)
    orientation = get(entity.data, "orientation", false)
    sprite = "objects/DJMapHelper/springGreen/00.png"
    if orientation == "Floor"
        Ahorn.drawSprite(ctx, sprite, 0, -8)
    elseif orientation == "WallLeft"
        Ahorn.drawSprite(ctx, sprite, 24, 0, rot=pi / 2)
    else
        Ahorn.drawSprite(ctx, sprite, -8, 16, rot=-pi / 2)
    end
end

end