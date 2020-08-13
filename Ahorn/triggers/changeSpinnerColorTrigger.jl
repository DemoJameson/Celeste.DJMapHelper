module DJMapHelperChangeSpinnerColorTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/changeSpinnerColorTrigger" SpinnerColor(x::Integer, y::Integer, width::Integer=16, height::Integer=16, mode::String="OnPlayerEnter", color::String="Default")
const modes = String[
    "OnPlayerEnter",
    "OnLevelStart"
]
const colors = String[
    "Blue",
    "Red",
    "Purple",
    "Rainbow",
    "Default"
]

const placements = Ahorn.PlacementDict(
    "Change Spinner Color Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        SpinnerColor,
        "rectangle"
    )
)

Ahorn.editingOptions(Trigger::SpinnerColor) = Dict{String, Any}(
    "mode" => modes,
    "color" => colors
)

end