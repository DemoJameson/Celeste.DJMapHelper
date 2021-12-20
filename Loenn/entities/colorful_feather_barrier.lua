local featherBarrier = {}

featherBarrier.name = "DJMapHelper/featherBarrier"
featherBarrier.depth = 0
featherBarrier.placements = {}

for _, color in ipairs({ "Blue", "Red", "Green", "Yellow" }) do
    table.insert(featherBarrier.placements, {
        name = string.lower(color),
        data = {
            width = 8,
            height = 8,
            color = color
        }
    })
end

function featherBarrier.fillColor(room, entity)
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

function featherBarrier.borderColor(room, entity)
    local color = entity.color or "Blue"

    if color == "Red" then
        return { 0.75, 0.25, 0.25, 1 }
    elseif color == "Green" then
        return { 0.25, 0.75, 0.25, 1 }
    elseif color == "Yellow" then
        return { 0.75, 0.75, 0.25, 1 }
    else
        return { 0.25, 0.25, 0.75, 1 }
    end
end

return featherBarrier