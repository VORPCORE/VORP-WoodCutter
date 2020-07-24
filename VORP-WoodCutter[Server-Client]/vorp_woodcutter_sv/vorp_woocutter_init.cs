using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_woodcutter_sv
{
    public class vorp_woocutter_init : BaseScript
    {
        public static List<Tree> TreesSync = new List<Tree>();

        public vorp_woocutter_init()
        {
            TriggerEvent("vorp:addNewCallBack", "getTrees", new Action<int, CallbackDelegate, dynamic>((source, cb, anything) =>
            {
                string trees = JsonConvert.SerializeObject(TreesSync);
                cb(trees);
            }));

            EventHandlers[$"{API.GetCurrentResourceName()}:SetDeadTree"] += new Action<Player, int>(setDeadTree);
            EventHandlers[$"{API.GetCurrentResourceName()}:refineWood"] += new Action<Player>(refineWood);
        }

        private void refineWood([FromSource]Player player)
        {
            int _source = int.Parse(player.Handle);
            Random rnd = new Random();
            TriggerEvent("vorpCore:getItemCount", _source, new Action<int>((count) => {
                if (count >= LoadConfig.Config["ItemNeededOnRefine"].ToObject<int>())
                {
                    int reward = rnd.Next(LoadConfig.Config["ItemsGivedOnRefine"][0].ToObject<int>(), LoadConfig.Config["ItemsGivedOnRefine"][1].ToObject<int>());
                    TriggerEvent("vorpCore:canCarryItems", _source, reward, new Action<bool>((can) => {
                        if (can)
                        {
                            TriggerEvent("vorpCore:subItem", _source, LoadConfig.Config["ItemGivedOnChop"].ToString(), LoadConfig.Config["ItemNeededOnRefine"].ToObject<int>());
                            TriggerEvent("vorpCore:addItem", _source, LoadConfig.Config["ItemGivedOnRefine"].ToString(), reward);
                            player.TriggerEvent("vorp:TipRight", string.Format(LoadConfig.Langs["RefineReward"], reward.ToString()), 4000);
                        }
                        else
                        {
                            player.TriggerEvent("vorp:TipRight", LoadConfig.Langs["CantCarryMore"], 4000);
                        }
                    }));
                }
                else
                {
                    player.TriggerEvent("vorp:TipRight", LoadConfig.Langs["InsufficientMaterials"], 4000);
                }
            }), LoadConfig.Config["ItemGivedOnChop"].ToString());
        }

        private void setDeadTree([FromSource]Player player, int id)
        {
            TreesSync[id].setDead();
            int _source = int.Parse(player.Handle);
            Random rnd = new Random();
            Debug.WriteLine(id.ToString());
            TriggerEvent("vorpCore:addItem", _source, LoadConfig.Config["ItemGivedOnChop"].ToString(), rnd.Next(LoadConfig.Config["SyncTrees"][id]["ItemGivedOnChopRandom"][0].ToObject<int>(), LoadConfig.Config["SyncTrees"][id]["ItemGivedOnChopRandom"][1].ToObject<int>() + 1));
            TriggerEvent("vorp:addXp", _source, LoadConfig.Config["SyncTrees"][id]["XPGivedOnChop"].ToObject<int>());

            TriggerClientEvent($"{API.GetCurrentResourceName()}:SetDeadTree", id);
        }

        public static async Task vorpwoocutter_init()
        {
            for (int i = 0; i < LoadConfig.Config["SyncTrees"].Count(); i++)
            {
                Vector3 pos = new Vector3(LoadConfig.Config["SyncTrees"][i]["Coords"][0].ToObject<float>(), LoadConfig.Config["SyncTrees"][i]["Coords"][1].ToObject<float>(), LoadConfig.Config["SyncTrees"][i]["Coords"][2].ToObject<float>());
                TreesSync.Add(new Tree(i, LoadConfig.Config["SyncTrees"][i]["NameOnTarget"].ToString(), LoadConfig.Config["SyncTrees"][i]["Model"].ToString(), pos, false));
            }
        }

        [Tick]
        public static async Task RespawnTrees()
        {
            //$"{API.GetCurrentResourceName()}:SetAliveTree
            await Delay(1000);

            if (TreesSync.Count() == 0 || !LoadConfig.isConfigLoaded)
            {
                return;
            }

            for (int i = 0; i < TreesSync.Count(); i++)
            {
                if (TreesSync[i].IsDead)
                {
                    TreesSync[i].Time -= 1;

                    if (TreesSync[i].Time == 0)
                    {
                        TreesSync[i].IsDead = false;
                        TriggerClientEvent($"{API.GetCurrentResourceName()}:SetAliveTree", i);
                    }
                }
            }

        }

    }
}
