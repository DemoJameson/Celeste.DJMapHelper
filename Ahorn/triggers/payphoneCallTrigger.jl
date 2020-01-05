module PayphoneCallTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/payphoneCallTrigger" PayphoneDialog(x::Integer, y::Integer, width::Integer=16, height::Integer=16, endLevel::Bool=false, answer::Bool=false, dialogId::String="")

const placements = Ahorn.PlacementDict(
    "Payphone Call (DJMapHelper)" => Ahorn.EntityPlacement(
        PayphoneDialog,
        "rectangle"
    )
)

end