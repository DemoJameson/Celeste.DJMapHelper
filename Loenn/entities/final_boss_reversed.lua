local enums = require("consts.celeste_enums")

local finalBossReversed = {}

finalBossReversed.name = "DJMapHelper/finalBossReversed"
finalBossReversed.depth = 0
finalBossReversed.nodeLineRenderType = "line"
finalBossReversed.texture = "characters/badelineBoss/charge00"
finalBossReversed.nodeLimits = { 0, -1 }
finalBossReversed.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
    }
}
finalBossReversed.placements = {
    name = "normal",
    data = {
        patternIndex = 1,
        startHit = false,
        canChangeMusic = true
    }
}
finalBossReversed.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = enums.badeline_boss_shooting_patterns,
        editable = false
    }
}

return finalBossReversed