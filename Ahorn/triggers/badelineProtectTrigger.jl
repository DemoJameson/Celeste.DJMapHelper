module BadelineProtectTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/badelineProtectTrigger" BadelineProtect(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "Badeline Protect Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        BadelineProtect,
        "rectangle"
    )
)

end