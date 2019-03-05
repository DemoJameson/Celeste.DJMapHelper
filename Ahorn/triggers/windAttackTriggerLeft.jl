module WindAttackTriggerLeft

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/windAttackTriggerLeft" WindAttackLeft(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Snowballs (Left) (DJMapHelper)" => Ahorn.EntityPlacement(
        WindAttackLeft,
        "rectangle"
    )
)

end