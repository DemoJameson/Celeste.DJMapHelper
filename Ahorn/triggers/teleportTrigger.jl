module TeleportTrigger

using ..Ahorn, Maple

@mapdef Trigger "DJMapHelper/teleportTrigger" Teleport(x::Integer, y::Integer, width::Integer=16, height::Integer=16, bonfire::Bool=false, room::String="", spawnPointX::Int=0, spawnPointY::Int=0, Dreaming::String="NoChange")
const dreams = String[
    "Dreaming",
    "Awake",
    "NoChange"
]

const placements = Ahorn.PlacementDict(
    "Teleport Trigger (DJMapHelper)" => Ahorn.EntityPlacement(
        Teleport,
        "rectangle"
    )
)

Ahorn.editingOptions(Trigger::Teleport) = Dict{String, Any}(
    "Dreaming" => dreams
)


end