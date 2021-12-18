local flingBirdReversed = {}

flingBirdReversed.name = "DJMapHelper/flingBirdReversed"
flingBirdReversed.depth = 0
flingBirdReversed.nodeLineRenderType = "line"
flingBirdReversed.texture = "characters/bird/Hover04"
flingBirdReversed.nodeScale = { -1, 1 }
flingBirdReversed.nodeLimits = { 0, -1 }
flingBirdReversed.placements = {
    name = "normal",
    data = {
        waiting = false
    }
}

return flingBirdReversed