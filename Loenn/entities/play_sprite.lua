local playSprite = {}

playSprite.name = "DJMapHelper/playSprite"
playSprite.depth = 100
playSprite.texture = "characters/oldlady/idle00"
playSprite.justification = { 0.5, 1.0 }
playSprite.placements = {
    name = "normal",
    data = {
        flipX = false,
        flipY = false,
        SpriteName = "granny",
        SpriteId = "laugh",
    }
}

function playSprite.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1
    local scaleY = entity.flipY and -1 or 1

    return scaleX, scaleY
end

return playSprite