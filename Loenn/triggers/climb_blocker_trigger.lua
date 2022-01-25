local climbBlockerTrigger = {}

climbBlockerTrigger.name = "DJMapHelper/climbBlockerTrigger"
climbBlockerTrigger.placements = {
    name = "normal",
    data = {
        climb = false,
        wallJump = false,
        mode = "Contained",
    }
}
climbBlockerTrigger.fieldInformation = {
    mode = {
        fieldType = "anything",
        options = { "Contained", "Persistent" },
        editable = false
    }
}

return climbBlockerTrigger