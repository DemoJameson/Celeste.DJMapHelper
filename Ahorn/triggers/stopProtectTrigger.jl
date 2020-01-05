module StopProtectTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/stopProtectTrigger" StopProtect(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Stop Protect Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        StopProtect,
        "rectangle"
    )
)

end