module DJMapHelperClimbBlockerTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/climbBlockerTrigger" ClimbBlocker(x::Integer, y::Integer, width::Integer=16, height::Integer=16, wallJump::Bool=false, climb::Bool=false, mode::String="Contained")

const modes = String[
    "Contained",
    "Persistent"
]

const placements = Ahorn.PlacementDict(
    "Climb Blocker (DJMapHelper)" => Ahorn.EntityPlacement(
        ClimbBlocker,
        "rectangle"
    )
)

Ahorn.editingOptions(Trigger::ClimbBlocker) = Dict{String, Any}(
    "mode" => modes
)

end