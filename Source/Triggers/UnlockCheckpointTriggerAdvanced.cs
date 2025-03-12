using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.SSMHelper.Triggers
{
    [CustomEntity("SSMHelper/UnlockCheckpointTriggerAdvanced")]
    public class UnlockCheckpointTriggerAdvanced : Trigger
    {
        public bool AutoSave;
        public string[] Checkpoints;

        public UnlockCheckpointTriggerAdvanced(EntityData data, Vector2 offset) : base(data, offset)
        {
            AutoSave = data.Bool("autoSave");
            Checkpoints = data.Attr("checkpoints").Split(',');
        }

        public override void OnEnter(Player player)
        {
            base.OnEnter(player);
            foreach (string checkpoint in Checkpoints)
            {
                string sid = checkpoint.Split(":")[0];
                string room = checkpoint.Split(":")[1];
                AreaMode mode = AreaMode.Normal;
                // stupid hacks
                if (sid.EndsWith("-B"))
                {
                    mode = AreaMode.BSide;
                    sid = sid[..^2];
                }
                else if (sid.EndsWith("-C"))
                {
                    mode = AreaMode.CSide;
                    sid = sid[..^2];
                }

                // just copy this stuff from Commands.loadMapBySID() tbh
                AreaData areaData = AreaData.Get(sid);
                MapData mapData = null;
                if (areaData?.Mode.Length > (int?)mode)
                {
                    mapData = areaData?.Mode[(int)mode]?.MapData;
                }
                if (areaData == null)
                {
                    Logger.Warn(nameof(SSMHelperModule), $"Map {sid} does not exist!");
                    continue;
                }
                else if (mapData == null)
                {
                    Logger.Warn(nameof(SSMHelperModule), $"Map {sid} has no {mode} mode!");
                    continue;
                }
                else if (room != null && (mapData.Levels?.All(level => level.Name != room) ?? false))
                {
                    Logger.Warn(nameof(SSMHelperModule), $"Map {sid} / mode {mode} has no room named {room}!");
                    continue;
                }

                // ok we are gaming i think
                if (CheckpointExists(mapData, room))
                {
                    AreaKey key = new AreaKey(areaData.ID, mode);
                    if (SaveData.Instance.SetCheckpoint(key, room) && AutoSave)
                    {
                        SceneAs<Level>().AutoSave();
                    }
                }
                else
                {
                    Logger.Warn(nameof(SSMHelperModule), $"Checkpoint {checkpoint} does not exist!");
                }
            }

            RemoveSelf();
        }

        private bool CheckpointExists(MapData mapData, string roomName)
        {
            return Enumerable.Any(mapData.Levels, level => level.HasCheckpoint && level.Name == roomName);
        }
    }
}
