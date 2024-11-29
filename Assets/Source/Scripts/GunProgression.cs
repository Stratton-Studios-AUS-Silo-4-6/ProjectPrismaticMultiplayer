using System.Collections.Generic;
using System.Linq;
using MultiFPS;
using UnityEngine;
using MultiFPS.Gameplay;
using MultiFPS.Gameplay.Gamemodes;

namespace StrattonStudioGames.PrisMulti
{
    [AddComponentMenu("MultiFPS/Gamemodes/GunProgression")]
    public class GunProgression : Gamemode
    {
        public GunProgressionConfig config;

        public GunProgression()
        {
            Indicator = Gamemodes.GunProgression;
            LetPlayersSpawnOnTheirOwn = true;
            FFA = true;
            FriendyFire = true; //friendly fire must be true because in free for all Deathmatch everyone are in the same team, 
            //so i they want to fight each other, friendy fire must be true
        }
        
        public override void PlayerSpawnCharacterRequest(PlayerInstance playerInstance)
        {
            if (LetPlayersSpawnOnTheirOwn)
            {
                playerInstance.Server_SpawnCharacter(defaultSpawnPoints.GetNextSpawnPoint());
            }
        }

        public override void Server_OnPlayerInstanceAdded(PlayerInstance player)
        {
            if (!isServer) return;
            //in FFA we want all players to be in the same team, so we dont let team choose and we choose default team for them instead

            AssignPlayerToTeam(player, 0);
            ResetPlayerInventory(player);

            var characterItemManager = player.MyCharacter.CharacterItemManager;
            characterItemManager.CanGrabItem = false;
            characterItemManager.CanDropItem = false;
        }

        public override void Server_OnPlayerKilled(Health victimID, Health killerID)
        {
            if (!isServer)
                return;
            
            var killer = GameManager.FindPlayerInstanceByCharacter(killerID.GetComponent<CharacterInstance>());
            var killCount = killer.Kills;

            if (killer.Kills >= config.GetMax())
            {
                SwitchGamemodeState(GamemodeState.Finish);
            }
            else
            {
                var nextItem = config.GetItem(killCount);
                var itemManager = killer.MyCharacter.CharacterItemManager;
                itemManager.Server_DespawnItem(0);
                itemManager.Server_SpawnInventory(nextItem);
            }
        }

        public override void Server_OnPlayerRespawn(PlayerInstance playerInstance)
        {
            if (!isServer)
                return;

            var kills = playerInstance.Kills;

            if (kills >= config.GetMax())
            {
                return;
            }
            
            var item = config.GetItem(kills);
            playerInstance.MyCharacter.CharacterItemManager.Server_SpawnInventory(item);
        }

        public override void Server_OnPlayerDied(PlayerInstance playerInstance)
        {
            if (!isServer)
                return;
            
            playerInstance.MyCharacter.CharacterItemManager.Server_DespawnItem(0);
        }

        protected override void TimerEnded()
        {
            base.TimerEnded();

            switch (State)
            {
                case GamemodeState.Warmup:
                    SwitchGamemodeState(GamemodeState.Inprogress);
                    break;

                case GamemodeState.Inprogress:
                    SwitchGamemodeState(GamemodeState.Finish);
                    break;
            }
        }

        protected override void CheckTeamStates()
        {
            base.CheckTeamStates();

            if (!isServer) return;

            //start the game if there is more than 1 player
            if (_teams[0].PlayerInstances.Count > 1)
            {
                if (State == GamemodeState.WaitingForPlayers)
                    SwitchGamemodeState(GamemodeState.Warmup);
            }
            else
            {
                SwitchGamemodeState(GamemodeState.WaitingForPlayers);
            }
        }

        protected override void MatchEvent_StartMatch()
        {
            base.MatchEvent_StartMatch();

            ResetPlayersStats();
            ResetAllPlayerInventories();
            RespawnAllPlayers(defaultSpawnPoints);
            CountTimer(GameDuration);
            
            GamemodeMessage("Match started!", 3f);

            LetPlayersSpawnOnTheirOwn = true;
        }

        protected override void MatchEvent_EndMatch()
        {
            base.MatchEvent_EndMatch();
            StopTimer();

            BlockAllPlayers(true);

            LetPlayersSpawnOnTheirOwn = false;

            //find the winner
            List<PlayerInstance> players = GameManager.Players;

            players = players.OrderByDescending(x => x.Kills).ToList();

            //display message who won
            GamemodeMessage(players[0].PlayerInfo.Username + " won!", 5f);

            //set timer for next round
            DelaySetGamemodeState(GamemodeState.Warmup, 5f);
        }

        protected override void MatchEvent_StartWarmup()
        {
            base.MatchEvent_StartWarmup();
            LetPlayersSpawnOnTheirOwn = false;
        }
        protected override void OnPlayerAddedToTeam(PlayerInstance player, int team)
        {
            player.Server_SpawnCharacter(defaultSpawnPoints.GetNextSpawnPoint());
        }

        protected override int PlayerRequestToJoinTeamPermission(PlayerInstance player, int requestedTeam)
        {
            return -1;
        }

        private void ResetAllPlayerInventories()
        {
            foreach (var playerInstance in GameManager.Players)
            {
                ResetPlayerInventory(playerInstance);
            }
        }

        private void ResetPlayerInventory(PlayerInstance playerInstance)
        {
            var kills = playerInstance.Kills;
            var item = config.GetItem(kills);
            var characterItemManager = playerInstance.MyCharacter.CharacterItemManager;
            characterItemManager.Server_DespawnAllItems();
            characterItemManager.Server_SpawnInventory(item);
        }
    }
}