local drawableSprite = require("structs.drawable_sprite")

local templeGate = {}

templeGate.name = "DJMapHelper/templeGateReversed"
templeGate.depth = -9000
templeGate.canResize = { false, false }
templeGate.placements = {
    name = "normal",
    data = {
        height = 48,
        sprite = "default",
        ["type"] = "CloseBehindPlayer",
    }
}

local textures = {
    default = "objects/door/TempleDoor00",
    mirror = "objects/door/TempleDoorB00",
    theo = "objects/door/TempleDoorC00"
}

local types = {
    "CloseBehindPlayer",
    "CloseBehindPlayerAlways",
    "HoldingTheo",
    "CloseBehindPlayerAndTheo"
}

templeGate.fieldInformation = {
    sprite = {
        fieldType = "anything",
        options = textures,
        editable = false
    },
    ["type"] = {
        fieldType = "anything",
        options = types,
        editable = false
    },
}

function templeGate.sprite(room, entity)
    local variant = entity.sprite or "default"
    local texture = textures[variant] or textures["default"]
    local sprite = drawableSprite.fromTexture(texture, entity)

    -- Weird offset from the code, justifications are from sprites.xml
    sprite:setJustification(0.5, 0.0)
    sprite:addPosition(4, -8)

    return sprite
end

return templeGate