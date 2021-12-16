local climbBlockerTrigger = {}

climbBlockerTrigger.name = "DJMapHelper/climbBlockerTrigger"
climbBlockerTrigger.placements = {
    name = "normal",
    data = {
        climb = false,
        wallJump = false,
        mode = "Contained",
    }
}

return climbBlockerTrigger