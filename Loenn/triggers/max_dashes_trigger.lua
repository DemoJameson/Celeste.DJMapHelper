local maxDashesTrigger = {}

maxDashesTrigger.name = "DJMapHelper/maxDashesTrigger"
maxDashesTrigger.placements = {
    name = "normal",
    data = {
        dashes = "One",
    }
}
maxDashesTrigger.fieldInformation = {
    dashes = {
        fieldType = "anything",
        options = { "Zero", "One", "Two" },
        editable = false
    }
}

return maxDashesTrigger