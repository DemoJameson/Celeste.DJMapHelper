module DJMapHelperPlaySprite

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/playSprite" playSprite(x::Integer, y::Integer, SpriteName::String="granny", SpriteId::String="laugh", flipX::Bool=false, flipY::Bool=false, depth::Integer=0)

const placements = Ahorn.PlacementDict(
    "Play Sprite (DJMapHelper)" => Ahorn.EntityPlacement(
        playSprite,
        "rectangle"
    ),
)

sprite = "characters/oldlady/idle00"

function Ahorn.selection(entity::playSprite)
    x, y = Ahorn.position(entity)
	scaleX, scaleY = get(entity.data, "flipX", false) ? -1 : 1, get(entity.data, "flipY", false) ? -1 : 1
	return Ahorn.getSpriteRectangle(sprite, x, y, jx=0.5, jy=1, sx=scaleX, sy=scaleY)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::playSprite, room::Maple.Room)
	scaleX, scaleY = get(entity.data, "flipX", false) ? -1 : 1, get(entity.data, "flipY", false) ? -1 : 1
	Ahorn.drawSprite(ctx, sprite, 0, 0, jx=0.5, jy=1, sx=scaleX, sy=scaleY)
end

end