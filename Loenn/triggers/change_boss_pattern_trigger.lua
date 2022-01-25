local enums = require("consts.celeste_enums")

local changeBossPatternTrigger = {}

changeBossPatternTrigger.name = "DJMapHelper/changeBossPatternTrigger"
changeBossPatternTrigger.placements = {
    name = "normal",
    data = {
        dashless = false,
        mode = "All",
        patternIndex = 1,
        onlyOnce = false,
    }
}
changeBossPatternTrigger.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
        options = enums.badeline_boss_shooting_patterns,
        editable = false
    },
    mode = {
        fieldType = "anything",
        options = { "All", "Contained" },
        editable = false
    }
}

return changeBossPatternTrigger