local utils = require("utils")
local shield = {}

shield.name = "DJMapHelper/shield"
shield.depth = 0
shield.placements = {
    name = "normal",
}

function shield.draw(room, entity, viewport)
    local x, y = entity.x or 0, entity.y or 0
    love.graphics.circle("line", x, y, 12)
end

function shield.selection(room, entity)
    return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
end

return shield