using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_woodcutter_cl
{
    public class Tree
    {
        int id;
        string name;
        string model;
        Vector3 coords;
        bool isDead;

        int fakeped;
        int objent;

        public Tree(int id, string name, string model, Vector3 coords, bool isDead, int ped, int obj)
        {
            this.id = id;
            this.name = name;
            this.model = model;
            this.coords = coords;
            this.isDead = isDead;
            fakeped = ped;
            objent = obj;
        }

        public int Id { get => id; set => id = value; }
        public string Name { get => name; set => name = value; }
        public string Model { get => model; set => model = value; }
        public Vector3 Coords { get => coords; set => coords = value; }
        public bool IsDead { get => isDead; set => isDead = value; }
        public int Fakeped { get => fakeped; set => fakeped = value; }
        public int Objent { get => objent; set => objent = value; }

        public void setDead()
        {
            isDead = true;
            //Need rework events
        }
    }
}
