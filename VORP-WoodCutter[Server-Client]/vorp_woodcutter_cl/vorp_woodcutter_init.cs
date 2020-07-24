using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_woodcutter_cl
{
    public class vorp_woodcutter_init : BaseScript
    {
        public static JArray Tress;
        public static List<Tree> TreeFakePeds = new List<Tree>();


        public vorp_woodcutter_init()
        {
            API.RegisterCommand("start", new Action<int, List<object>, string>(async (source, args, raw) =>
            {
                Vector3 playerCoords = API.GetEntityCoords(API.PlayerPedId(), true, true);

                Function.Call((Hash)0x322BFDEA666E2B0E, API.PlayerPedId(), playerCoords.X, playerCoords.Y, playerCoords.Z, 5.0, -1, 1, 1, 1, 1);

                Debug.WriteLine("Scenario");

                await Delay(5000);

                int scenario = Function.Call<int>((Hash)0x569F1E1237508DEB, API.PlayerPedId());

                Debug.WriteLine(scenario.ToString());

            }), false);

            API.RegisterCommand("stop", new Action<int, List<object>, string>((source, args, raw) =>
            {
                API.ClearPedTasks(API.PlayerPedId(), 1, 1);

            }), false);

            EventHandlers[$"{API.GetCurrentResourceName()}:SetDeadTree"] += new Action<int>(setDeadTree);
            EventHandlers[$"{API.GetCurrentResourceName()}:SetAliveTree"] += new Action<int>(setAliveTree);
        }

        private async void setAliveTree(int id)
        {
            TreeFakePeds[id].IsDead = false;

            uint objHash = (uint)API.GetHashKey(TreeFakePeds[id].Model);
            await LoadModel(objHash);

            TreeFakePeds[id].Objent = API.CreateObject(objHash, TreeFakePeds[id].Coords.X, TreeFakePeds[id].Coords.Y, TreeFakePeds[id].Coords.Z, false, true, true, true, true);

            //API.PlaceObjectOnGroundProperly(entTree, 0);

            //Create Fake Ped
            Vector3 objCoords = API.GetEntityCoords(TreeFakePeds[id].Objent, true, true);
            uint pedHash = (uint)API.GetHashKey("mp_male");
            await LoadModel(pedHash);
            TreeFakePeds[id].Fakeped = API.CreatePed(pedHash, objCoords.X, objCoords.Y, objCoords.Z - 0.2f, 360.0f, false, true, true, true);
            API.SetEntityCollision(TreeFakePeds[id].Objent, false, true);
            API.FreezeEntityPosition(TreeFakePeds[id].Fakeped, true);
            API.SetEntityCanBeDamaged(TreeFakePeds[id].Fakeped, true);
            API.SetPedScale(TreeFakePeds[id].Fakeped, 0.6f);
            API.SetBlockingOfNonTemporaryEvents(TreeFakePeds[id].Fakeped, true);
            API.SetEntityMaxHealth(TreeFakePeds[id].Fakeped, GetConfig.Config["SyncTrees"][id]["Health"].ToObject<int>());
            API.SetEntityHealth(TreeFakePeds[id].Fakeped, GetConfig.Config["SyncTrees"][id]["Health"].ToObject<int>(), 0);
            API.SetPedPromptName(TreeFakePeds[id].Fakeped, TreeFakePeds[id].Name);
            API.SetEntityOnlyDamagedByPlayer(TreeFakePeds[id].Fakeped, true);
            await Delay(1000);
        }

        private async void setDeadTree(int id)
        {
            TreeFakePeds[id].setDead();

            int tree = TreeFakePeds[id].Objent;
            int ped = TreeFakePeds[id].Fakeped;

            Debug.WriteLine(API.GetPedSourceOfDeath(ped).ToString());
            Debug.WriteLine("ha muerto!");
            API.SetEntityHasGravity(tree, true);
            API.FreezeEntityPosition(tree, false);
            API.DeletePed(ref ped);
            Vector3 rotArbol = API.GetEntityRotation(tree, 0);
            while (true)
            {
                await Delay(10);
                rotArbol.X += 1.0f;
                API.SetEntityRotation(tree, rotArbol.X, rotArbol.Y, rotArbol.Z, 0, true);
                if (rotArbol.X >= 95)
                {
                    break;
                }
            }
            await Delay(5000);
            API.DeleteObject(ref tree);
        }

        public static async Task CreateTrees()
        {
            await Delay(5000);

            TriggerEvent("vorp:ExecuteServerCallBack", "getTrees", new Action<string>(async (json) => {

                Tress = JArray.Parse(json);
                int index = 0;
                foreach (var tree in Tress)
                {
                    if (!tree["IsDead"].ToObject<bool>())
                    {
                        uint objHash = (uint)API.GetHashKey(tree["Model"].ToString());
                        await LoadModel(objHash);

                        int entTree = API.CreateObject(objHash, tree["Coords"]["X"].ToObject<float>(), tree["Coords"]["Y"].ToObject<float>(), tree["Coords"]["Z"].ToObject<float>(), false, true, true, true, true);

                        //API.PlaceObjectOnGroundProperly(entTree, 0);

                        //Create Fake Ped
                        Vector3 objCoords = API.GetEntityCoords(entTree, true, true);
                        uint pedHash = (uint)API.GetHashKey("mp_male");
                        await LoadModel(pedHash);
                        int entPed = API.CreatePed(pedHash, objCoords.X, objCoords.Y, objCoords.Z - 0.2f, 360.0f, false, true, true, true);
                        API.SetEntityCollision(entTree, false, true);
                        API.FreezeEntityPosition(entPed, true);
                        API.SetEntityCanBeDamaged(entPed, true);
                        API.SetPedScale(entPed, 0.6f);
                        API.SetBlockingOfNonTemporaryEvents(entPed, true);
                        API.SetEntityMaxHealth(entPed, GetConfig.Config["SyncTrees"][index]["Health"].ToObject<int>());
                        API.SetEntityHealth(entPed, GetConfig.Config["SyncTrees"][index]["Health"].ToObject<int>(), 0);                        
                        API.SetPedPromptName(entPed, tree["Name"].ToString());
                        API.SetEntityOnlyDamagedByPlayer(entPed, true);
                        Vector3 objpos = new Vector3(tree["Coords"]["X"].ToObject<float>(), tree["Coords"]["Y"].ToObject<float>(), tree["Coords"]["Z"].ToObject<float>());
                        TreeFakePeds.Add(new Tree(tree["Id"].ToObject<int>(), tree["Name"].ToString(), tree["Model"].ToString(), objpos, false, entPed, entTree));
                        Debug.WriteLine("Arbol creado");
                        await Delay(100);
                    }
                    index += 1;
                }
            }), "null");
        }


        [Tick]
        public async Task checkHealth()
        {
            await Delay(500);
            if (TreeFakePeds.Count == 0)
            {
                return;
            }

            for (int i = 0; i < TreeFakePeds.Count(); i++)
            {
                int tree = TreeFakePeds[i].Objent;
                int ped = TreeFakePeds[i].Fakeped;
                int health = API.GetEntityHealth(ped);
          
                if (health <= 0 && !TreeFakePeds[i].IsDead)
                {
                    TreeFakePeds[i].setDead();
                    TriggerServerEvent($"{API.GetCurrentResourceName()}:SetDeadTree", i);
                }
            }
        }


        public static async Task<bool> LoadModel(uint hash)
        {
            if (Function.Call<bool>(Hash.IS_MODEL_VALID, hash))
            {
                Function.Call(Hash.REQUEST_MODEL, hash);
                while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash))
                {
                    Debug.WriteLine($"Waiting for model {hash} load!");
                    await Delay(100);
                }
                return true;
            }
            else
            {
                Debug.WriteLine($"Model {hash} is not valid!");
                return false;
            }
        }
    }
}
