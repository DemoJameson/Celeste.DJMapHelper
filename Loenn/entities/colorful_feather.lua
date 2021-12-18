-- TODO 解决贴图不显示的问题

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local colorfulFeather = {}

colorfulFeather.name = "DJMapHelper/colorfulFlyFeather"
colorfulFeather.depth = 0
colorfulFeather.placements = {}

for _, color in ipairs({ "Blue", "Red", "Green" }) do
    table.insert(colorfulFeather.placements, {
        name = string.lower(color),
        data = {
            shielded = false,
            singleUse = false,
            color = color
        }
    })
end

function colorfulFeather.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0

    local shielded = entity.shielded or false
    if shielded then
        love.graphics.circle("line", x, y, 12)
    end

    local color = entity.color or "Blue"
    local texture = ""

    if color == "Red" then
        texture = "objects/DJMapHelper/redFlyFeather/idle00"
    elseif color == "Green" then
        texture = "objects/DJMapHelper/greenFlyFeather/idle00"
    else
        texture = "objects/DJMapHelper/blueFlyFeather/idle00"
    end

    drawableSprite.fromTexture("objects/flyFeather/idle00", entity):draw()
    --drawableSprite.fromTexture(texture, { x = x, y = y }):draw()
end

function colorfulFeather.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return colorfulFeather