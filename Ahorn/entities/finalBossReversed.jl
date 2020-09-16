module DJMapHelperFinalBossReversed

using ..Ahorn, Maple

@mapdef Entity "DJMapHelper/finalBossReversed" BadelineBoss(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], patternIndex::Integer=1, startHit::Bool=false, canChangeMusic::Bool=true)

const placements = Ahorn.PlacementDict(
    "Badeline Boss Reversed (DJMapHelper)" => Ahorn.EntityPlacement(
        BadelineBoss,
    )
)

Ahorn.editingOptions(entity::BadelineBoss) = Dict{String, Any}(
    "patternIndex" => Maple.badeline_boss_shooting_patterns
)

Ahorn.nodeLimits(entity::BadelineBoss) = 0, -1

sprite = "characters/badelineBoss/charge00.png"

function Ahorn.selection(entity::BadelineBoss)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BadelineBoss)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BadelineBoss, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x, y, sx=-1)
end

end