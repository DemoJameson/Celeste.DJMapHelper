local teleportTrigger = {}

teleportTrigger.name = "DJMapHelper/teleportTrigger"
teleportTrigger.fieldOrder = {
    "x", "y", "width", "height", "room", "Dreaming", "bonfire", "KeepKey", "spawnPointX", "spawnPointY"
}
teleportTrigger.placements = {
    name = "normal",
    data = {
        bonfire = true,
        KeepKey = true,
        room = "",
        Dreaming = "NoChange",
        spawnPointX = 0,
        spawnPointY = 0,
    }
}

return teleportTrigger