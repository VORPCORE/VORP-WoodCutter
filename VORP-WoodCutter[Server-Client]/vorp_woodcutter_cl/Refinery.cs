using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_woodcutter_cl
{
    class Refinery : BaseScript
    {
        static bool isOnScenario = false;

        [Tick]
        public async Task onRefinery()
        {
            if (!GetConfig.isLoaded)
            {
                return;
            }

            int pid = API.PlayerPedId();
            Vector3 pCoords = API.GetEntityCoords(pid, true, true);

            for (int i = 0; i < GetConfig.Config["RefineryZones"].Count(); i++)
            {
                float x = float.Parse(GetConfig.Config["RefineryZones"][i]["Coords"][0].ToString());
                float y = float.Parse(GetConfig.Config["RefineryZones"][i]["Coords"][1].ToString());
                float z = float.Parse(GetConfig.Config["RefineryZones"][i]["Coords"][2].ToString());

                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, x, y, z, true) <= 1.0f)
                {
                    if (!isOnScenario) //API.IsPedUsingAnyScenario(pid)
                    {
                        await DrawTxt(GetConfig.Langs["PressToStart"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                        if (API.IsControlJustPressed(2, 0xD9D0E1C0))
                        {
                            API.ClearPedTasks(API.PlayerPedId(), 1, 1);
                            await Delay(100);
                            Function.Call((Hash)0x322BFDEA666E2B0E, API.PlayerPedId(), x, y, z, 2.0, -1, 1, 1, 1, 1);
                            isOnScenario = true;
                            await Delay(1000);
                        }
                    }
                    else
                    {
                        await DrawTxt(GetConfig.Langs["PressToStop"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                        if (API.IsControlJustPressed(2, 0xD9D0E1C0))
                        {
                            API.ClearPedTasks(API.PlayerPedId(), 1, 1);
                            isOnScenario = false;
                        }
                        if (API.IsControlJustPressed(0, 0x07CE1E61))
                        {
                            await Delay(5000);
                            TriggerServerEvent($"{API.GetCurrentResourceName()}:refineWood");
                            Debug.WriteLine("Refinando");
                        }
                    }
                }

            }
        }


        public async Task DrawTxt(string text, float x, float y, float fontscale, float fontsize, int r, int g, int b, int alpha, bool textcentred, bool shadow)
        {
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            Function.Call(Hash.SET_TEXT_SCALE, fontscale, fontsize);
            Function.Call(Hash._SET_TEXT_COLOR, r, g, b, alpha);
            Function.Call(Hash.SET_TEXT_CENTRE, textcentred);
            if (shadow) { Function.Call(Hash.SET_TEXT_DROPSHADOW, 1, 0, 0, 255); }
            Function.Call(Hash.SET_TEXT_FONT_FOR_CURRENT_COMMAND, 1);
            Function.Call(Hash._DISPLAY_TEXT, str, x, y);
        }
    }
}
