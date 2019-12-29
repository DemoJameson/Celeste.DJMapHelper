module DreamBlockYellow

using ..Ahorn, Maple
@mapdef Entity "DJMapHelper/dreamBlockYellow" YellowDreamBlock(x::Integer, y::Integer, width::Integer=16, height::Integer=16, fastMoving::Bool=false, oneUse::Bool=false)
const placements = Ahorn.PlacementDict(
    "Space Jam Yellow (DJMapHelper)" => Ahorn.EntityPlacement(
        YellowDreamBlock,
        "rectangle"
    )
)


Ahorn.nodeLimits(entity::YellowDreamBlock) = 0, 1

Ahorn.minimumSize(entity::YellowDreamBlock) = 8, 8
Ahorn.resizable(entity::YellowDreamBlock) = true, true

function Ahorn.selection(entity::YellowDreamBlock)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    nodes = get(entity.data, "nodes", ())
    if isempty(nodes)
        return Ahorn.Rectangle(x, y, width, height)

    else
        nx, ny = Int.(nodes[1])
        return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(nx, ny, width, height)]
    end
end

function renderYellowDreamBlock(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number)
    Ahorn.Cairo.save(ctx)

    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1)

    Ahorn.drawRectangle(ctx, x, y, width, height, (0.0, 0.0, 0.0, 0.4), (1.0, 1.0, 1.0, 1.0))

    Ahorn.restore(ctx)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::YellowDreamBlock)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))
    
    if !isempty(nodes)
        nx, ny = Int.(nodes[1])

        cox, coy = floor(Int, width / 2), floor(Int, height / 2)

        renderYellowDreamBlock(ctx, nx, ny, width, height)
        Ahorn.drawArrow(ctx, x + cox, y + coy, nx + cox, ny + coy, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::YellowDreamBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderYellowDreamBlock(ctx, 0, 0, width, height)
end

end