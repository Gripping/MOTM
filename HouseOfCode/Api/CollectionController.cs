using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using HouseOfCode.Models;
using Umbraco.Web.WebApi;

namespace HouseOfCode.Api
{
    public class CollectionController : UmbracoApiController
    {
        //Gets a filtered club collection
        [HttpGet]
        public List<club> GetFilteredClubs(int sportsId)
        {
            var cs = Services.ContentService;
            var clubs = cs.GetRootContent();

            List<club> filteredClubs = new List<club>();
            bool status = false;

            foreach ( var club in clubs)
            {   
                if (club.ContentTypeId == 1073)
                {
                    var teams = cs.GetChildren(club.Id);

                    foreach (var team in teams)
                    {

                        status = false;

                        if (team.ContentTypeId == 1074)
                        {
                            var s = (team.Properties["typeOfSport"].Value.ToString() != null) ? this.GetSport(team.Properties["typeOfSport"].Value.ToString()) : null;
                        
                            if (s.id == sportsId)
                            {
                                status = true;
                                break;
                            }
                        }
                    }

                    if (status == true)
                    {
                        var c = new club();
                        c.id = club.Id;
                        c.name = club.Name;
                        try
                        {
                            c.clubImage = this.GetImg(club.Properties["clubLogo"].Value.ToString());
                        }
                        catch
                        {
                            c.clubImage = "";
                        };
                        filteredClubs.Add(c);
                    }
                }
            }

            return filteredClubs;
        }

        //Gets single club based on id
        [HttpGet]
        public object GetClub(int clubId, int sportId)
        {
            var cs = Services.ContentService;

            var cClub = cs.GetById(clubId);

            var sId = sportId;

            var c = new singleClub();

            c.clubId = cClub.Id;
            c.clubName = cClub.Name;
            try
            {
                c.clubImage = this.GetImg(cClub.Properties["clubLogo"].Value.ToString());
            }
            catch
            {
                c.clubImage = "";
            };

            List<teams> clubTeams = new List<teams>();
            var cTeams = this.GetTeams(clubId);

            foreach (var cTeam in cTeams)
            {
                if (cTeam.sport.id == sportId)
                {
                    clubTeams.Add(cTeam);
                }
            }

            c.clubTeams = clubTeams;

            return c;
        }

        //Gets team collection for single club
        [HttpGet]
        public List<teams> GetTeams(int clubId)
        {
            var cs = Services.ContentService;
            var teams = cs.GetChildren(clubId);
            var c = cs.GetById(clubId);

            var AT = new teams();

            List<teams> AllTeams = new List<teams>();

            foreach (var team in teams)
            {
                if (team.ContentTypeId == 1074)
                {
                    var t = new teams();
                    t.id = team.Id;
                    t.name = team.Name;
                    t.clubName = c.Name;
                    var sport = (team.Properties["typeOfSport"].Value.ToString() != null) ? this.GetSport(team.Properties["typeOfSport"].Value.ToString()) : null;
                    t.sport = sport;
                    var gender = (team.Properties["typeOfGender"].Value != null) ? team.Properties["typeOfGender"].Value.ToString() : null;
                    try
                    {
                        t.gender = Umbraco.GetPreValueAsString(int.Parse(gender));
                    }
                    catch
                    {
                        t.gender = "";
                    }

                    t.coach = (team.Properties["teamManager"].Value != null) ? this.GetManager(team.Properties["teamManager"].Value.ToString()) : null;

                    AllTeams.Add(t);
                }
            }

            return AllTeams;
        }

        //Get single team from team id
        [HttpGet]
        public team GetTeam(int teamId)
        {
            var cs = Services.ContentService;
            var team = cs.GetById(teamId);
            var teamMatches = cs.GetChildren(teamId);

            var t = new team();
            t.name = team.Name;
            t.matches = new List<match>();

            foreach (var tMatch in teamMatches)
            {
                var m = new match();

                m.id = tMatch.Id;
                m.matchLocation = (tMatch.Properties["matchLocation"].Value != null) ? tMatch.Properties["matchLocation"].Value.ToString() : null;
                m.matchTime = (tMatch.Properties["matchDate"].Value != null) ? tMatch.Properties["matchDate"].Value.ToString() : null;
                m.enemyTeam = (tMatch.Properties["enemyTeam"].Value != null) ? tMatch.Properties["enemyTeam"].Value.ToString() : null;
                m.matchPlayers = (tMatch.Properties["players"].Value != null) ? this.GetPlayers(tMatch.Properties["players"].Value.ToString()) : null;

                m.enemyGoal = (tMatch.Properties["enemyGoal"].Value != null) ? int.Parse(tMatch.Properties["enemyGoal"].Value.ToString()) : 0;
                m.homeGoal = (tMatch.Properties["homeGoal"].Value != null) ? int.Parse(tMatch.Properties["homeGoal"].Value.ToString()) : 0;
                m.status = (tMatch.Properties["status"].Value != null) ? tMatch.Properties["status"].Value.ToString() : "0";

                t.matches.Add(m);
            }

            t.managerId = (team.Properties["teamManager"].Value != null) ? team.Properties["teamManager"].Id : 0;

            //t.players = (team.Properties["players"].Value != null) ? this.GetPlayers(team.Properties["players"].Value.ToString()) : null;

            t.sport = (team.Properties["typeOfSport"].Value != null) ? this.GetSport(team.Properties["typeOfSport"].Value.ToString()) : null;

            return t;
        }

        public List<player> GetAllPlayers(int clubId, int teamId = 0)
        {
            var cs = Services.ContentService;
            List<player> AllPlayers = new List<player>();
            if(teamId == 0)
            {
                var children = cs.GetChildren(clubId);
                int fetchId = new int();

                foreach (var child in children)
                {
                    if(child.ContentTypeId == 1127)
                    {
                        fetchId = child.Id;
                    }
                }

                var players = cs.GetChildren(fetchId);

                foreach (var player in players)
                {
                    player p = new Models.player();

                    p.id = player.Id;
                    p.playerName = (player.Properties["playerName"].Value != null) ? player.Properties["playerName"].Value.ToString() : null;
                    p.playerNumber = (player.Properties["playerNumber"].Value != null) ? int.Parse(player.Properties["playerNumber"].Value.ToString()) : 0;
                    p.playerPosition = (player.Properties["playerPosition"].Value != null) ? player.Properties["playerPosition"].Value.ToString() : null;
                    p.playerAge = (player.Properties["playerAge"].Value != null) ? player.Properties["playerAge"].Value.ToString() : null;
                    p.playerPicture = (player.Properties["playerPicture"].Value != null) ? this.GetImg(player.Properties["playerPicture"].Value.ToString()) : null;

                    AllPlayers.Add(p);

                }
            }
            else
            {
                var team = cs.GetById(teamId);
                AllPlayers = (team.Properties["players"].Value != null) ? this.GetPlayers(team.Properties["players"].Value.ToString()) : null;
            }

            return AllPlayers;
        }

        //Gets list players for team
        [HttpGet]
        private List<player> GetPlayers(string players)
        {
            var cs = Services.ContentService;

            string[] AllPlayers = players.Split(',');

            List<player> rPlayers = new List<player>();
            foreach (var player in AllPlayers)
            {
                var cP = cs.GetById(Guid.Parse(player.Substring(15)));
                var p = new player();

                p.id = cP.Id;
                p.playerName = (cP.Properties["playerName"].Value != null) ? cP.Properties["playerName"].Value.ToString() : null;
                p.playerNumber = (cP.Properties["playerNumber"].Value != null) ? int.Parse(cP.Properties["playerNumber"].Value.ToString()) : 0;
                p.playerAge = (cP.Properties["playerAge"].Value != null) ? cP.Properties["playerAge"].Value.ToString() : null;
                p.playerPosition = (cP.Properties["playerPosition"].Value != null) ? cP.Properties["playerPosition"].Value.ToString() : null;

                try
                {
                    p.playerPicture = this.GetImg(cP.Properties["playerPicture"].Value.ToString());
                }
                catch
                {
                    p.playerPicture = "";
                };

                rPlayers.Add(p);
            }

            return rPlayers;
        }

        //gets sports for filter
        public List<sport> GetAllSports()
        {
            var cs = Services.ContentService;
            var sports = cs.GetChildren(1136);

            List<sport> allSports = new List<sport>();

            foreach (var sport in sports)
            {
                var s = new sport();

                s.id = sport.Id;
                s.sportName = sport.Name;
                try
                {
                    s.sportImage = this.GetImg(sport.Properties["sportImage"].Value.ToString());
                }
                catch
                {
                    s.sportImage = "";
                };

                allSports.Add(s);

            }

            return allSports;
        }

        //Gets image
        [HttpGet]
        private object GetImg(string pictureId)
        {
            var ms = Services.MediaService;

            var imgGuid = Guid.Parse(pictureId.Substring(12));

            var img = ms.GetById(imgGuid);

            return "https://potm.bootsmann.dk" + Umbraco.Media(img.Id).Url;
        }

        private tManager GetManager(string ManagerId)
        {
            var cs = Services.MemberService;

            var id = Guid.Parse(ManagerId.Substring(13));

            var m = cs.GetByKey(id);

            var nm = new tManager();

            nm.id = m.Id;
            nm.name = m.Name;

            return nm;
        }

        //gets sport
        [HttpGet]
        private sport GetSport(string sportId)
        {
            var cs = Services.ContentService;

            var sport = cs.GetById(Guid.Parse(sportId.Substring(15)));
            var s = new sport();

            s.id = sport.Id;
            s.sportName = sport.Properties["sportName"].Value.ToString();
            
            return s;
        }
    }
}