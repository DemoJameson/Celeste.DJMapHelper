module DestoryAllCrystalSpinnerTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/destoryAllCrystalSpinnerTrigger" DestorySpinner(x::Integer, y::Integer, width::Integer=16, height::Integer=16)

const placements = Ahorn.PlacementDict(
    "DestoryAllCrystalSpinnerTrigger (DJMapHelper)" => Ahorn.EntityPlacement(
        DestorySpinner,
        "rectangle"
    )
)

end