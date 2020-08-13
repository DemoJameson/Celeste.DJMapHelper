module DJMapHelperTalkToBadelineTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/talkToBadelineTrigger" BadelineDialog(x::Integer, y::Integer, width::Integer=16, height::Integer=16, endLevel::Bool=false, rejoin::Bool=false, dialogId::String="")

const placements = Ahorn.PlacementDict(
    "Talk To Badeline Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        BadelineDialog,
        "rectangle"
    )
)

end