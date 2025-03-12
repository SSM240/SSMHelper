local unlockCheckpointTriggerAdv = {}

unlockCheckpointTriggerAdv.name = "SSMHelper/UnlockCheckpointTriggerAdvanced"
unlockCheckpointTriggerAdv.placements = {
    name = "normal",
    data = {
        autoSave = false,
        checkpoints = "",
    }
}

unlockCheckpointTriggerAdv.fieldInformation = {
    checkpoints = {
        fieldType = "list"
    }
}

return unlockCheckpointTriggerAdv
