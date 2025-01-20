using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.SSMHelper.Entities
{
    [CustomEntity("SSMHelper/FakeBino")]
    public class FakeBino : Lookout
    {
        public FakeBino(EntityData data, Vector2 offset) : base(data, offset)
        {
        }

        public override void Awake(Scene scene)
        {
            Remove(talk);
        }
    }
}
