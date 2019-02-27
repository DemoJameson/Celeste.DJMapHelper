module ClimbBlockerTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/climbBlockerTrigger" ClimbBlocker(x::Integer, y::Integer, width::Integer=16, height::Integer=16, wallJump::Bool=false, climb::Bool=false)

const placements = Ahorn.PlacementDict(
    "Climb Blocker (DJMapHelper)" => Ahorn.EntityPlacement(
        ClimbBlocker,
        "rectangle",
        Dict{String, Any}(
            "wallJump" => false,
            "climb" => false
        )
    )
)


end