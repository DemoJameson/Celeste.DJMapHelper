module DJMapHelperBadelineProtectTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/badelineProtectTrigger" BadelineProtect(x::Integer, y::Integer, width::Integer=16, height::Integer=16, maxQuantity::Integer=3, radius::Integer=24, respwanTime::Number=8.0, rotationTime::Number=1.8, clockwise::Bool=true)

const placements = Ahorn.PlacementDict(
    "Badeline Protect Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        BadelineProtect,
        "rectangle"
    )
)

end