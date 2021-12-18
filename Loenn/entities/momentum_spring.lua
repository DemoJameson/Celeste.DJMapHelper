-- TODO 使用绿色贴图，并实时更新设置的贴图

local momentumSpring = {}
momentumSpring.name = "DJMapHelper/springGreen"
momentumSpring.depth = -8501
momentumSpring.texture = "objects/spring/00"
momentumSpring.color = {0, 1, 0, 1}
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

function momentumSpring.rotation(room, entity)
    local orientation = entity.orientation or "Floor"
    if orientation == "WallLeft" then
        return math.pi / 2
    elseif orientation == "WallRight" then
        return -math.pi / 2
    else
        return 0
    end
end

return momentumSpring