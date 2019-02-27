module TempleGateReversed

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/templeGateReversed" TempleGate(x::Integer, y::Integer, height::Integer=48, type::String="CloseBehindPlayer", sprite::String="default")

const placements = Ahorn.PlacementDict(
    "Temple Gate Reversed (DJMapHelper)" => Ahorn.EntityPlacement(
        TempleGate
    )
)

textures = String["default", "mirror", "theo"]

const temple_gate_modes = String[
    "CloseBehindPlayer",
    "CloseBehindPlayerAlways",
    "HoldingTheo",
    "CloseBehindPlayerAndTheo"
]

Ahorn.editingOptions(entity::TempleGate) = Dict{String, Any}(
    "type" => temple_gate_modes,
    "sprite" => textures
)

function Ahorn.selection(entity::TempleGate)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x - 4, y, 15, height)
end

sprites = Dict{String, String}(
    "default" => "objects/door/TempleDoor00",
    "mirror" => "objects/door/TempleDoorB00",
    "theo" => "objects/door/TempleDoorC00"
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TempleGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "default")

    if haskey(sprites, sprite)
        Ahorn.drawImage(ctx, sprites[sprite], -4, 0)
    end
end

end