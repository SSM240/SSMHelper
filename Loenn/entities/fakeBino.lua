local fakeBino = {}

fakeBino.name = "SSMHelper/FakeBino"
fakeBino.depth = -8500
fakeBino.justification = {0.5, 1.0}
fakeBino.nodeLineRenderType = "line"
fakeBino.texture = "objects/lookout/lookout05"
fakeBino.nodeLimits = {0, -1}
fakeBino.placements = {
    name = "normal",
    data = {
        summit = false,
        onlyY = false
    }
}

return fakeBino
