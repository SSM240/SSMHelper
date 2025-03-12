using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.SSMHelper.Triggers
{
    [CustomEntity("SSMHelper/UnlockCheckpointTrigger")]
    public class UnlockCheckpointTrigger : Trigger
    {
        public bool AutoSave;
        public string Room;

        public UnlockCheckpointTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            AutoSave = data.Bool("autoSave");
            Room = data.Attr("room");
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            Level level = SceneAs<Level>();
            if (CheckpointExists(Room))
            {
                if (SaveData.Instance.SetCheckpoint(level.Session.Area, Room) && AutoSave)
                {
                    level.AutoSave();
                }
            }
            else
            {
                Logger.Warn(nameof(SSMHelperModule), $"Room \"{Room}\" does not exist or does not have a checkpoint!");
            }
            RemoveSelf();
        }

        private bool CheckpointExists(string roomName)
        {
            MapData mapData = SceneAs<Level>().Session.MapData;
            return Enumerable.Any(mapData.Levels, level => level.HasCheckpoint && level.Name == roomName);
        }
    }
}
