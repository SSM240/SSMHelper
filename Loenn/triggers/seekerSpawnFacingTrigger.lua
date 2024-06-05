local enums = require("consts.celeste_enums")

local seekerSpawnFacingTrigger = {}

seekerSpawnFacingTrigger.name = "SSMHelper/SeekerSpawnFacingTrigger"
seekerSpawnFacingTrigger.fieldInformation = {
    facing = {
        options = enums.spawn_facing_trigger_facings,
        editable = false
    }
}
seekerSpawnFacingTrigger.placements = {
    name = "normal",
    data = {
        facing = "Right"
    }
}

return seekerSpawnFacingTrigger
