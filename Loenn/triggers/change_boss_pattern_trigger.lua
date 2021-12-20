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
    patternIndex = { fieldType = "integer" },
}

return changeBossPatternTrigger