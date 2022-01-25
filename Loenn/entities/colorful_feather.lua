local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local colorfulFeather = {}
local colors = { "Blue", "Red", "Green" }

colorfulFeather.name = "DJMapHelper/colorfulFlyFeather"
colorfulFeather.depth = 0
colorfulFeather.placements = {}

for _, color in ipairs(colors) do
    table.insert(colorfulFeather.placements, {
        name = string.lower(color),
        data = {
            shielded = false,
            singleUse = false,
            color = color
        }
    })
end

colorfulFeather.fieldInformation = {
    color = {
        fieldType = "anything",
        options = colors,
        editable = false
    }
}

function colorfulFeather.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local color = entity.color or "Blue"
    local texture

    if color == "Red" then
        texture = "objects/DJMapHelper/redFlyFeather/idle00"
    elseif color == "Green" then
        texture = "objects/DJMapHelper/greenFlyFeather/idle00"
    else
        texture = "objects/DJMapHelper/blueFlyFeather/idle00"
    end

    local sprites = { drawableSprite.fromTexture(texture, { x = x, y = y }) }

    local shielded = entity.shielded or false
    if shielded then
        table.insert(sprites, drawableSprite.fromTexture("objects/DJMapHelper/shield/shield", { x = x, y = y }))
    end

    return sprites
end

function colorfulFeather.selection(room, entity)
    return utils.rectangle(entity.x - 10, entity.y - 10, 20, 20)
end

return colorfulFeather