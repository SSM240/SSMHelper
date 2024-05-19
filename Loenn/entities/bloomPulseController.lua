local bloomPulseController = {}

local easers = {
    "BackIn",
    "BackInOut",
    "BackOut",
    "BounceIn",
    "BounceInOut",
    "BounceOut",
    "CubeIn",
    "CubeInOut",
    "CubeOut",
    "ElasticIn",
    "ElasticInOut",
    "ElasticOut",
    "ExpoIn",
    "ExpoInOut",
    "ExpoOut",
    "Linear",
    "QuadIn",
    "QuadInOut",
    "QuadOut",
    "SineIn",
    "SineInOut",
    "SineOut"
}

bloomPulseController.name = "SSMHelper/BloomPulseController"
bloomPulseController.depth = 0
bloomPulseController.texture = "loenn/SSMHelper/bloompulsecontroller"
bloomPulseController.placements = {
    name = "normal",
    data = {
        bloomStrengthFrom = 0.0,
        bloomStrengthTo = 1.0,
        bloomBaseFrom = 0.0,
        bloomBaseTo = 1.0,
        duration = 2.0,
        modifyBloomBase = false,
        modifyBloomStrength = true,
        tweenMode = "YoyoLooping",
        easer = "Linear",
        persistent = true,
    }
}
bloomPulseController.fieldInformation = {
    tweenMode = {
        options = {"Looping", "YoyoLooping"},
        editable = false
    },
    easer = {
        options = easers,
        editable = false
    }
}

return bloomPulseController
