local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local momentumSpring = {}

momentumSpring.name = "DJMapHelper/springGreen"
momentumSpring.depth = -8501
momentumSpring.placements = {}

local placementPresets = {
    up = "Floor",
    left = "WallRight",
    right = "WallLeft",
}

for name, orientation in pairs(placementPresets) do
    table.insert(momentumSpring.placements, {
        name = name,
        data = {
            orientation = orientation,
            sprite = "objects/DJMapHelper/springGreen"
        }
    })
end

function momentumSpring.sprite(room, entity, viewport)
    local sprite = drawableSprite.fromTexture("objects/DJMapHelper/springGreen/00", entity)
    sprite:setJustification(0.5, 1)
    local orientation = entity.orientation or "Floor"
    if orientation == "WallLeft" then
        sprite.rotation = math.pi / 2
    elseif orientation == "WallRight" then
        sprite.rotation = -math.pi / 2
    end

    return sprite
end

function momentumSpring.selection(room, entity)
    local orientation = entity.orientation or "Floor"
    if orientation == "WallLeft" then
        return utils.rectangle(entity.x, entity.y - 6, 5, 12)
    elseif orientation == "WallRight" then
        return utils.rectangle(entity.x - 5, entity.y - 6, 5, 12)
    else
        return utils.rectangle(entity.x - 6, entity.y - 5, 12, 5)
    end
end

return momentumSpring