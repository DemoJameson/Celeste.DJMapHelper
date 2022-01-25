local changeSpinnerColorTrigger = {}

changeSpinnerColorTrigger.name = "DJMapHelper/changeSpinnerColorTrigger"
changeSpinnerColorTrigger.placements = {
    name = "normal",
    data = {
        mode = "OnPlayerEnter",
        color = "Default",
    }
}
changeSpinnerColorTrigger.fieldInformation = {
    mode = {
        fieldType = "anything",
        options = { "OnPlayerEnter", "OnLevelStart" },
        editable = false
    },
    color = {
        fieldType = "anything",
        options = { "Default", "Blue", "Red", "Purple", "Rainbow" },
        editable = false
    }
}

return changeSpinnerColorTrigger