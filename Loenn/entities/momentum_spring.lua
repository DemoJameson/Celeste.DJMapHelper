local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local momentumSpring = {}

momentumSpring.name = "DJMapHelper/springGreen"
momentumSpring.depth = -8501
momentumSpring.color = { 0, 1, 0, 1 }
momentumSpring.justification = { 0.5, 1.0 }
momentumSpring.placements = {
    {
        name = "up",
        data = {
            orientation = "Floor",
            sprite = "objects/DJMapHelper/springGreen"
        }
    },
    {
        name = "left",
        data = {
            orientation = "WallRight",
            sprite = "objects/DJMapHelper/springGreen"
        }
    },
    {
        name = "right",
        data = {
            orientation = "WallLeft",
            sprite = "objects/DJMapHelper/springGreen"
        }
    },
}

function momentumSpring.sprite(room, entity, viewport)
    local sprite = drawableSprite.fromTexture("objects/DJMapHelper/springGreen/00", entity)

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
        return utils.rectangle(entity.x - 8, entity.y - 6, 5, 12)
    elseif orientation == "WallRight" then
        return utils.rectangle(entity.x + 3, entity.y - 6, 5, 12)
    else
        return utils.rectangle(entity.x - 6, entity.y + 3, 12, 5)
    end
end

return momentumSpring