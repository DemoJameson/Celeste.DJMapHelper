module DJMapHelperOshiroRightTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/oshiroRightTrigger" OshiroRightTrigger(x::Integer, y::Integer, width::Integer=16, height::Integer=16, state::Bool=true)

const placements = Ahorn.PlacementDict(
    "Oshiro Right (Spawn) (DJMapHelper)" => Ahorn.EntityPlacement(
        OshiroRightTrigger,
        "rectangle",
        Dict{String, Any}(
            "state" => true
        )
    ),
    "Oshiro Right (Leave) (DJMapHelper)" => Ahorn.EntityPlacement(
        OshiroRightTrigger,
        "rectangle",
        Dict{String, Any}(
            "state" => false
        )
    )
)

end