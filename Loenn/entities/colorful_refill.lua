local drawableSprite = require("structs.drawable_sprite")

local colorfulRefill = {}

colorfulRefill.name = "DJMapHelper/colorfulRefill"
colorfulRefill.depth = 0
colorfulRefill.placements = {}

for _, color in ipairs({ "Blue", "Red", "Black" }) do
    table.insert(colorfulRefill.placements, {
        name = string.lower(color),
        data = { color = color }
    })
end

function colorfulRefill.sprite(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local color = entity.color or "Blue"
    local texture

    if color == "Red" then
        texture = "objects/DJMapHelper/redRefill/idle00"
    elseif color == "Black" then
        texture = "objects/DJMapHelper/blackRefill/idle00"
    else
        texture = "objects/DJMapHelper/blueRefill/idle00"
    end

    return drawableSprite.fromTexture(texture, { x = x, y = y })
end

return colorfulRefill