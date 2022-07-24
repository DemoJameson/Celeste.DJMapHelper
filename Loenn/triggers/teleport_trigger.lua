local teleportTrigger = {}

teleportTrigger.name = "DJMapHelper/teleportTrigger"
teleportTrigger.fieldOrder = {
    "x", "y", "width", "height", "Dreaming", "bonfire", "KeepKey", "introTypes", "room", "spawnPointX", "spawnPointY"
}
teleportTrigger.placements = {
    name = "normal",
    data = {
        bonfire = true,
        KeepKey = true,
        Dreaming = "NoChange",
        introTypes = "WakeUp",
        room = "",
        spawnPointX = 0,
        spawnPointY = 0,
    }
}
teleportTrigger.fieldInformation = {
    spawnPointX = { fieldType = "integer" },
    spawnPointY = { fieldType = "integer" },
    Dreaming = {
        fieldType = "anything",
        options = { "Dreaming", "Awake", "NoChange" },
        editable = false
    },
    introTypes = {
        fieldType = "anything",
        options = {
            "Respawn",
            "WalkInRight",
            "WalkInLeft",
            "Jump",
            "WakeUp",
            "Fall",
            "TempleMirrorVoid",
            "None",
            "ThinkForABit"
        },
        editable = false
    }
}

return teleportTrigger