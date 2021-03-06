﻿using GTANetworkAPI;
using reallife.Db;
using reallife.Player;

namespace reallife.Events
{
    class LoginRegisterEvent : Script
    {

        [RemoteEvent("OnPlayerCharacterAttempt")]
        public void OnPlayerCharacterAttempt(Client client, string vorname, string nachname)
        {
            int pInfo = client.GetData("ID");
            PlayerInfo playerInfo = Database.GetById<PlayerInfo>(pInfo);


            /*if(Database.GetData<PlayerInfo>("vorname", vorname, "nachname", nachname) != null)
            {
                //client.SendChatMessage("Der Vor/nachname ist in der Kombination schon vorhanden!");
                client.TriggerEvent("CharacterResult", 0);
                return;
            }*/

            playerInfo.vorname = vorname;
            playerInfo.nachname = nachname;


            Database.Upsert(playerInfo);

            NAPI.Player.SetPlayerName(client, playerInfo.vorname + "" + playerInfo.nachname);

            for (int i = 0; i < 99; i++) client.SendChatMessage("~w~");
            client.TriggerEvent("CharacterResult", 1);
        }

        [RemoteEvent("OnPlayerRegisterAttempt")]
        public void OnPlayerRegisterAttempt(Client client, string username, string password, string passwordre)
        {
            int adminrank = 0;
            string socialclub = client.SocialClubName;
            string vorname = "None";
            string nachname = "None";

            PlayerInfo pInfo = Database.GetData<PlayerInfo>("username", username);

            Players players = new Players(username, password, socialclub);
            PlayerInfo playerInfo = new PlayerInfo(adminrank, vorname, nachname);

            if (password != passwordre)
            {
                client.TriggerEvent("RegisterResult", 0);
                return;
            }

            if (Database.GetData<Players>("username", username) != null)
            {
                client.SendChatMessage("Dieser Benutzername existiert bereits.");
                client.TriggerEvent("RegisterResult", 0);
                return;
            }

            playerInfo.Upsert();
            players.Upsert();
            for (int i = 0; i < 99; i++) client.SendChatMessage("~w~");
            client.SendChatMessage("Du hst dich erfolgreich Registriert.");
            client.SendChatMessage("Du kannst dich nun mit deinen Login Daten einloggen!");

            client.TriggerEvent("RegisterResult", 3);
        }

        [RemoteEvent("OnPlayerLoginAttempt")]
        public void OnPlayerLoginAttempt(Client client, string username, string password)
        {
            Players players = Database.GetData<Players>("username", username);
            PlayerVehicles pVeh = PlayerHelper.GetpVehiclesStats(client);

            if (players == null)
            {
                client.SendChatMessage("~r~Daten wurden nicht gefunden!");
                client.TriggerEvent("LoginResult");
                return;
            }

            if (!players.CheckPassword(password))
            {
                client.SendChatMessage("~r~Die angegebenen Daten sind korrekt!");
                client.TriggerEvent("LoginResult", 0);
                return;
            }

            if (client.HasData("ID"))
            {
                client.SendChatMessage("Du bist schon eingeloggt!");
                return;
            }
            client.SetData("ID", players._id);

            //LOGIN ENDE
            PlayerInfo pInfo = PlayerHelper.GetPlayerStats(client);
            Players playerInfo = PlayerHelper.GetPlayer(client);

            client.SetData("AdminRank", pInfo.adminrank);

            if (playerInfo.ban == 0)
            {
                Handler.FinishLogin(client);
                client.TriggerEvent("LoginResult", 1);

                    for (int i = 0; i < 99; i++) client.SendChatMessage("~w~");
                    //GUIDE START
                    if (pInfo.vorname == "None")
                    {
                        client.SendChatMessage("~r~SERVER: ~w~Bitte wähle einen Vor/nachname!");
                        NAPI.ClientEvent.TriggerClientEvent(client, "StartCharBrowser");
                        return;
                    }
                    else
                    {
                        client.SendChatMessage($"Willkommen, {pInfo.vorname} {pInfo.nachname} auf ~b~Reallife-V");
                        client.SendChatMessage("~r~SERVER: ~w~Dies ist eine Entwickler Version von ~b~Reallife-V!");
                        client.SendChatMessage("~r~SERVER: ~w~Also sind Bug`s keine Seltenheit!");
                        client.SendNotification($"~b~Name: {pInfo.vorname} {pInfo.nachname}");
                        return;
                    }
            }
            else
            {
                client.SendChatMessage("Dieser Account wurde gesperrt");
                client.TriggerEvent("LoginResult", 0);
                return;
            }
        }

    }
}
