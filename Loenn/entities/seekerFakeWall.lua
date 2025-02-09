local fakeTilesHelper = require("helpers.fake_tiles")

local seekerFakeWall = {}

seekerFakeWall.name = "SSMHelper/SeekerFakeWall"
seekerFakeWall.depth = -13000

function seekerFakeWall.placements()
    return {
        name = "normal",
        data = {
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            width = 8,
            height = 8
        }
    }
end

seekerFakeWall.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", true, "tilesFg", {1.0, 1.0, 1.0, 0.7})
seekerFakeWall.fieldInformation = fakeTilesHelper.getFieldInformation("tiletype")

return seekerFakeWall