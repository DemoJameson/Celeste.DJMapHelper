local badelineBoostTeleport = {}

badelineBoostTeleport.name = "DJMapHelper/badelineBoostTeleport"
badelineBoostTeleport.depth = -1000000
badelineBoostTeleport.nodeLineRenderType = "line"
badelineBoostTeleport.texture = "objects/badelineboost/idle00"
badelineBoostTeleport.nodeLimits = { 0, -1 }
badelineBoostTeleport.placements = {
    name = "normal",
    data = {
        Priority = "Moon -> Golden -> Key",
        DefaultColorGrade = "",
        DefaultRoom = "",
        DefaultSpawnPointX = 0,
        DefaultSpawnPointY = 0,
        MoonColorGrade = "",
        MoonRoom = "",
        MoonSpawnPointX = 0,
        MoonSpawnPointY = 0,
        GoldenColorGrade = "",
        GoldenRoom = "",
        GoldenSpawnPointX = 0,
        GoldenSpawnPointY = 0,
        KeyColorGrade = "",
        KeyRoom = "",
        KeySpawnPointX = 0,
        KeySpawnPointY = 0,
    }
}
badelineBoostTeleport.fieldInformation = {
    DefaultSpawnPointX = { fieldType = "integer" },
    DefaultSpawnPointY = { fieldType = "integer" },
    MoonSpawnPointX = { fieldType = "integer" },
    MoonSpawnPointY = { fieldType = "integer" },
    GoldenSpawnPointX = { fieldType = "integer" },
    GoldenSpawnPointY = { fieldType = "integer" },
    KeySpawnPointX = { fieldType = "integer" },
    KeySpawnPointY = { fieldType = "integer" },
}

return badelineBoostTeleport