using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_woodcutter_sv
{
    public class Tree
    {
        int id;
        string name;
        string model;
        Vector3 coords;
        bool isDead;
        int time;

        public Tree(int id, string name, string model, Vector3 coords, bool isDead)
        {
            this.id = id;
            this.name = name;
            this.model = model;
            this.coords = coords;
            this.isDead = isDead;
            time = 0;
        }

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Model { get => model; set => model = value; }
        public Vector3 Coords { get => coords; set => coords = value; }
        public bool IsDead { get => isDead; set => isDead = value; }
        public int Time { get => time; set => time = value; }

        public void setDead()
        {
            time = LoadConfig.Config["TimeToRespawn"].ToObject<int>();
            isDead = true;
            //Need rework events
        }
    }
}
