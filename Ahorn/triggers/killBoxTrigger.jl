module KillBoxTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/killBoxTrigger" KillBox(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "KillBox (DJMapHelper)" => Ahorn.EntityPlacement(
        KillBox,
        "rectangle"
    )
)

end