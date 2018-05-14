using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using HouseOfCode.Models;

namespace HouseOfCode.Api
{
    public class ActionController : UmbracoApiController
    {

        [HttpPost]
        public object managerLogin(string username, string password)
        {
            var mh = new MembershipHelper(UmbracoContext.Current);
            var login = mh.Login(username, password);
            login manager = new login();

            if (login == true)
            {
                var cManager = mh.GetByUsername(username);

                var cs = Services.ContentService;
                var clubs = cs.GetRootContent();


                foreach (var club in clubs)
                {
                    if (club.ContentTypeId == 1073)
                    {
                        var managerId = this.GetManager((club.Properties["clubManager"].Value != null) ? club.Properties["clubManager"].Value.ToString() : null);
                        if (managerId == cManager.Id)
                        {
                            manager.clubId = club.Id;
                        }
                    }
                }
                manager.managerType = cManager.GetType().Name;
            }

            return manager;
        }

        private int GetManager(string managerId)
        {
            var ms = Services.MemberService;
            var mId = new int();

            if (managerId != null)
            {
                var manager = ms.GetByKey(Guid.Parse(managerId.Substring(13)));
                mId = manager.Id;
            }

            return mId;
        }

        //Handles club mananger create team
        [HttpPut]
        public int managerCreateTeam([FromBody] newTeam nTeam, int clubId)
        {
            var username = nTeam.managerName.Replace(" ", "").ToLower();
            var ms = Services.MemberService;
            var cs = Services.ContentService;
            int teamId = new int();

            try
            {
                var memberCheck = ms.GetByUsername(username);
                if(memberCheck == null)
                {
                    var newManager = ms.CreateMember(username, nTeam.managerEmail, nTeam.managerName, "teamManager");
                    ms.Save(newManager);
                    ms.SavePassword(newManager, nTeam.managerPass);

                    var newTeam = cs.CreateContent(nTeam.teamName, clubId, "teams");
                    
                    var sportNode = cs.GetById(nTeam.sportId);
                    var slocaUdi = Udi.Create(Constants.UdiEntityType.Document, sportNode.Key);
                    newTeam.SetValue("typeOfSport", slocaUdi.ToString());

                    var mlocaUdi = Udi.Create(Constants.UdiEntityType.Member, newManager.Key);
                    newTeam.SetValue("teamManager", mlocaUdi.ToString());

                    cs.SaveAndPublishWithStatus(newTeam);

                    teamId = newTeam.Id;
                }
            }
            catch
            {
                teamId = 0;
            }

            return teamId;
        }

        //Handles club manager and team manager create player
        [HttpPut]
        public object managerCreatePlayer([FromBody] player nPlayer, int clubId, int sportId)
        {
            var cs = Services.ContentService;
            bool status = false;

            var club = cs.GetChildren(clubId);

            try
            {
                int createId = new int();
                foreach (var child in club)
                {
                    if(child.ContentTypeId == 1127)
                    {
                        createId = child.Id;
                    }
                }

                var newPlayer = cs.CreateContent(nPlayer.playerName, createId, "player");

                newPlayer.SetValue("playerName", nPlayer.playerName);
                newPlayer.SetValue("playerNumber", nPlayer.playerNumber);
                newPlayer.SetValue("playerAge", nPlayer.playerAge);
                newPlayer.SetValue("playerPosition", nPlayer.playerPosition);

                var node = cs.GetById(sportId);
                var locaUdi = Udi.Create(Constants.UdiEntityType.Document, node.Key);
                newPlayer.SetValue("sport", locaUdi.ToString());

                cs.SaveAndPublishWithStatus(newPlayer);

                status = true;
            }
            catch
            {
                status = false;
            }

            return status;
        }

        [HttpPut]
        //Handles team manager create match
        public int managerCreateMatch([FromBody] match nMatch, int teamId)
        {
            var cs = Services.ContentService;
            int matchId = new int();

            var matchName = "Kamp mod: " + nMatch.enemyTeam;

            var match = cs.GetChildren(teamId);

            try
            {
                var newMatch = cs.CreateContent(matchName, teamId, "match");

                newMatch.SetValue("enemyTeam", nMatch.enemyTeam);
                newMatch.SetValue("matchLocation", nMatch.matchLocation);
                newMatch.SetValue("matchDate", nMatch.matchTime);

                cs.SaveAndPublishWithStatus(newMatch);
                matchId = newMatch.Id;
            }
            catch
            {
                matchId = 0;
            }
            
            return matchId;
        }
        
        //Handles team manager choose player
        [HttpPut]
        public object managerChoosePlayer([FromBody] List<player> AllPlayers, int teamId, int matchId = 0)
        {
            bool status = false;
            var cs = Services.ContentService;
            var createContext = cs.GetById(teamId);

            if (matchId != 0)
            {
                createContext = cs.GetById(matchId);
            }

            try
            {

                string nodelist = "";

                foreach (var player in AllPlayers)
                {
                    var playerNode = cs.GetById(player.id);
                    var slocaUdi = Udi.Create(Constants.UdiEntityType.Document, playerNode.Key);
                    nodelist += slocaUdi + ",";
                }

                string trimmedNodeList = nodelist.TrimEnd(",");
                
                createContext.SetValue("players", trimmedNodeList);
                cs.SaveAndPublishWithStatus(createContext);

                status = true;
            }
            catch
            {
                status = false;
            }




            return status;
        }

        //Handles team manager end match
        public object managerEndMatch()
        {
            

            return null;
        }

        [HttpPut]
        public bool votingHandler([FromBody]vote newVote)
        {
            var cs = Services.ContentService;

            var match = cs.GetById(newVote.matchId);
            var votes = cs.GetChildren(newVote.matchId);
            bool status = false;

            foreach (var vote in votes)
            {
                if (votes.All(x => x.Properties["deviceId"].Value.ToString() != newVote.deviceId))
                {
                    status = true;
                }
            }

            if (status == true || votes.Count() == 0)
            {
                status = true;
                var nVote = cs.CreateContent(newVote.playerId.ToString(), newVote.matchId, "voting");
                nVote.SetValue("playerId", newVote.playerId);
                nVote.SetValue("deviceId", newVote.deviceId);
                cs.SaveAndPublishWithStatus(nVote);
            }

            return status;
        }
    }
}