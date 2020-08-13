module DJMapHelperColorGradeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/colorGradeTrigger" colorGrade(x::Integer, y::Integer, width::Integer=16, height::Integer=16, ColorGrade::String="none")
const colorGrades = String[
    "none",
    "cold",
    "credits",
    "hot",
    "oldsite",
    "panicattack",
    "reflection",
    "templevoid",
]

const placements = Ahorn.PlacementDict(
    "Color Grade Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        colorGrade,
        "rectangle"
    )
)

Ahorn.editingOptions(Trigger::colorGrade) = Dict{String, Any}(
    "ColorGrade" => colorGrades
)


end