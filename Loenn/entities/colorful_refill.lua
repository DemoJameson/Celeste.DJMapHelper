-- TODO 解决贴图不显示的问题

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")
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

function colorfulRefill.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    local color = entity.color or "Blue"
    local texture = ""

    if color == "Red" then
        texture = "objects/DJMapHelper/redRefill/idle00"
    elseif color == "Black" then
        texture = "objects/DJMapHelper/blackRefill/idle00"
    else
        texture = "objects/DJMapHelper/blueRefill/idle00"
    end

    --drawableSprite.fromTexture(texture, { x = x, y = y }):draw()
    drawableSprite.fromTexture("objects/refill/idle00", entity):draw()
end

function colorfulRefill.selection(room, entity)
    return utils.rectangle(entity.x - 5, entity.y - 5, 10, 10)
end

return colorfulRefill