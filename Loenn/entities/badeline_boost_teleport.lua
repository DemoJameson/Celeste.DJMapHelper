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
badelineBoostTeleport.fieldOrder = {
    "x", "y", "width", "height",
    "DefaultRoom", "DefaultColorGrade", "DefaultSpawnPointX", "DefaultSpawnPointY",
    "MoonRoom", "MoonColorGrade", "MoonSpawnPointX", "MoonSpawnPointY",
    "GoldenRoom", "GoldenColorGrade", "GoldenSpawnPointX", "GoldenSpawnPointY",
    "KeyRoom", "KeyColorGrade", "KeySpawnPointX", "KeySpawnPointY",
    "Priority"
}

return badelineBoostTeleport