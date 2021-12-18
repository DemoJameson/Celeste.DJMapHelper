local featherBarrier = {}

featherBarrier.name = "DJMapHelper/featherBarrier"
featherBarrier.depth = 0
featherBarrier.placements = {
    {
        name = "blue",
        data = {
            width = 16,
            height = 16,
            color = "Blue"
        }
    },
    {
        name = "red",
        data = {
            width = 16,
            height = 16,
            color = "Red"
        }
    },
    {
        name = "green",
        data = {
            width = 16,
            height = 16,
            color = "Green"
        }
    },
    {
        name = "yellow",
        data = {
            width = 16,
            height = 16,
            color = "Yellow"
        }
    }
}

function featherBarrier.color(room, entity)
    local color = entity.color or "Blue"

    if color == "Red" then
        return { 0.75, 0.25, 0.25, 0.5 }
    elseif color == "Green" then
        return { 0.25, 0.75, 0.25, 0.5 }
    elseif color == "Yellow" then
        return { 0.75, 0.75, 0.25, 0.5 }
    else
        return { 0.25, 0.25, 0.75, 0.5 }
    end
end

return featherBarrier