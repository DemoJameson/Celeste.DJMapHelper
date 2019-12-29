module ChangeBossPatternTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/changeBossPatternTrigger" ChangeBossPattern(x::Integer, y::Integer, width::Integer=16, height::Integer=16, mode::String="All", patternIndex::Integer=1, dashless::Bool=false)
const modes = String[
    "All",
    "Contained"
]

const patterns = Int[
    1, 2, 3, 4,
    5, 6, 7, 8, 9, 
    10, 11, 12, 13, 14,
    15
]

const placements = Ahorn.PlacementDict(
    "Change Boss Pattern Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        ChangeBossPattern,
        "rectangle"
    )
)

Ahorn.editingOptions(Trigger::ChangeBossPattern) = Dict{String, Any}(
    "mode" => modes,
    "patternIndex" => patterns
)

end