using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobilePhone
{
    public class MobilePhone : BaseScript
    {
        public static Dictionary<int, bool> MobilePhoneVehToggles = new Dictionary<int, bool>();
        private List<Player> spottedPlayers = new List<Player>();
        Prop MobilePhoneProp;
        bool toggle = false;
        private DateTime lastSpottedDateTime = DateTime.MinValue;
        public MobilePhone()
        {
            EventHandlers["MobilePhone:SyncInVeh"] += new Action<dynamic>((dynamic phones) =>
            {
                MobilePhoneVehToggles.Clear();
                if (phones != null)
                {
                    foreach (KeyValuePair<string, object> item in phones)
                    {
                        MobilePhoneVehToggles[Int32.Parse(item.Key)] = bool.Parse(item.Value.ToString());
                    }
                }
            });

            EventHandlers["MobilePhone:Call"] += new Action<dynamic>((dynamic) =>
            {
                TogglePhone();
            });
            EventHandlers["MobilePhone:Text"] += new Action<dynamic>((dynamic) =>
            {
                TogglePhoneText();
            });

            Observations();
        }

        private async Task Observations()
        {
            while (true)
            {
                await Delay(100);
                if (spottedPlayers.Count > 0 && (DateTime.Now - lastSpottedDateTime).Seconds > 10)
                {
                    spottedPlayers.Clear();
                    lastSpottedDateTime = DateTime.MinValue;
                }

                foreach (KeyValuePair<int, bool> toggles in MobilePhoneVehToggles)
                {
                    Player p = Players[toggles.Key];
                    if (toggles.Value && p != null && LocalPlayer != p && !spottedPlayers.Contains(p) && p.Character != null && p.Character.Exists() && Vector3.Distance(LocalPlayer.Character.Position, p.Character.Position) < 19
                        && p.Character.IsInVehicle() && p.Character.CurrentVehicle.IsEngineRunning)
                    {
                        string desc = "passenger";
                        if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Driver) == p.Character)
                        {
                            desc = "driver";
                        }
                        else if (p.Character.CurrentVehicle.GetPedOnSeat(VehicleSeat.Passenger) == p.Character)
                        {
                            desc = "front seat passenger";
                        }
                        else
                        {
                            continue;
                        }
                        string modelName = API.GetDisplayNameFromVehicleModel((uint)API.GetEntityModel(p.Character.CurrentVehicle.Handle));
                        string plate = API.GetVehicleNumberPlateText(p.Character.CurrentVehicle.Handle).Trim();
                        Screen.ShowNotification("Using handheld phone: " + desc + " of the " + modelName + ".");
                        TriggerEvent("chatMessage", "Observations", new int[] { 255, 165, 0 }, "You notice the " + desc + " of the " + modelName + " (" + plate + ") is using a handheld phone.");
                        lastSpottedDateTime = DateTime.Now;
                        spottedPlayers.Add(p);
                    }
                }
            }
        }

        private async void TogglePhone()
        {
            toggle = !toggle;
            if (!MobilePhoneVehToggles.ContainsKey(LocalPlayer.ServerId))
            {                
                MobilePhoneVehToggles[LocalPlayer.ServerId] = toggle;
            }
            TriggerServerEvent("MobilePhone:UpdateToggle", LocalPlayer.ServerId, toggle);
            if (toggle)
            {
                if (MobilePhoneProp != null && MobilePhoneProp.Exists()) { MobilePhoneProp.Delete(); }
                LocalPlayer.Character.CanSwitchWeapons = false;
                LocalPlayer.Character.Weapons.Give(WeaponHash.Unarmed, -1, true, true);
                MobilePhoneProp = await World.CreateProp(new Model("prop_player_phone_02"), Vector3.Zero, true, true);

                int boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, LocalPlayer.Character, 28422);
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, MobilePhoneProp, LocalPlayer.Character, boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);
                LocalPlayer.Character.Task.PlayAnimation("cellphone@", "cellphone_call_listen_base", 1.3f, -1, (AnimationFlags)49);
            }
            else
            {
                LocalPlayer.Character.CanSwitchWeapons = true;
                LocalPlayer.Character.Task.ClearSecondary();
                await Delay(800);
                if (MobilePhoneProp != null && MobilePhoneProp.Exists() && !toggle) { MobilePhoneProp.Delete(); }
            }
        }

        private async void TogglePhoneText()
        {
            toggle = !toggle;
            if (!MobilePhoneVehToggles.ContainsKey(LocalPlayer.ServerId))
            {
                MobilePhoneVehToggles[LocalPlayer.ServerId] = toggle;
            }
            TriggerServerEvent("MobilePhone:UpdateToggle", LocalPlayer.ServerId, toggle);

            if (toggle)
            {
                if (MobilePhoneProp != null && MobilePhoneProp.Exists()) { MobilePhoneProp.Delete(); }
                LocalPlayer.Character.CanSwitchWeapons = false;
                LocalPlayer.Character.Weapons.Give(WeaponHash.Unarmed, -1, true, true);
                MobilePhoneProp = await World.CreateProp(new Model("prop_player_phone_02"), Vector3.Zero, true, true);

                int boneIndex = Function.Call<int>(Hash.GET_PED_BONE_INDEX, LocalPlayer.Character, 28422);
                Function.Call(Hash.ATTACH_ENTITY_TO_ENTITY, MobilePhoneProp, LocalPlayer.Character, boneIndex, 0f, 0f, 0f, 0f, 0f, 0f, true, true, false, false, 2, 1);
                LocalPlayer.Character.Task.PlayAnimation("cellphone@", "cellphone_text_read_base", 1.3f, -1, (AnimationFlags)49);
            }
            else
            {
                LocalPlayer.Character.CanSwitchWeapons = true;
                LocalPlayer.Character.Task.ClearSecondary();
                await Delay(800);
                if (MobilePhoneProp != null && MobilePhoneProp.Exists() && !toggle) { MobilePhoneProp.Delete(); }
            }
        }
    }
}
