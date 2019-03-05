module MaxDashesTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/maxDashesTrigger" MaxDashes(x::Integer, y::Integer, width::Integer=16, height::Integer=16, dashes::String="One")
const Dashes = String[
    "One",
    "Two"
]

const placements = Ahorn.PlacementDict(
    "Max Dashes (One) (DJMapHelper)" => Ahorn.EntityPlacement(
        MaxDashes,
        "rectangle",
        Dict{String, Any}(
            "dashes" => "One"
        )
    ),
	"Max Dashes (Two) (DJMapHelper)" => Ahorn.EntityPlacement(
        MaxDashes,
        "rectangle",
        Dict{String, Any}(
            "dashes" => "Two"
        )
    )
)

Ahorn.editingOptions(entity::MaxDashes) = Dict{String, Any}(
    "dashes" => Dashes
)

end