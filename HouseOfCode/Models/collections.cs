using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseOfCode.Models
{
 
    public class login
    {
        public int clubId { get; set; }
        public object managerType { get; set; }
    }

    public class vote
    {
        public int matchId { get; set; }
        public int playerId { get; set; }
        public string deviceId { get; set; }
    }

    public class motm
    {
        public int playerId { get; set; }
        public int increment { get; set; }
    }

    public class motmWinner
    {
        public int increment { get; set; }
        public string name { get; set; }
        public object image { get; set; }
    }

    public class newTeam
    {
        public string managerName { get; set; }
        public string managerEmail { get; set; }
        public string managerPass { get; set; }
        public int sportId { get; set; }
        public string teamName { get; set; }

    }

    public class club
    {
        public int id { get; set; }
        public string name { get; set; }
        public object clubImage { get; set; }
    }

    public class singleClub
    {
        public int clubId { get; set; }
        public string clubName { get; set; }
        public object clubImage { get; set; }
        public List<teams> clubTeams { get; set; }
    }

    public class teams
    {
        public int id { get; set; }
        public string name { get; set; }
        public string clubName { get; set; }
        public sport sport { get; set; }
        public string gender { get; set; }
        public tManager coach { get; set; }
    }

    public class team
    {
        public string name { get; set; }
        //public List<player> players { get; set; }
        public List<match> matches { get; set; }
        public int managerId { get; set; }
        public sport sport { get; set; }
    }

    public class player
    {
        public int id { get; set; }
        public string playerName { get; set; }
        public int playerNumber { get; set; }
        public string playerAge { get; set; }
        public object playerPicture { get; set; }
        public string playerPosition { get; set; }
    }

    public class match
    {
        public int id { get; set; }
        public string matchLocation { get; set; }
        public string matchTime { get; set; }
        public List<player> matchPlayers { get; set; }
        public string enemyTeam { get; set; }
        public int homeGoal { get; set; }
        public int enemyGoal { get; set; }
        public string status { get; set; }
    }

    public class sport
    {
        public int id { get; set; }
        public string sportName { get; set; }
        public object sportImage { get; set; }
    }

    public class tManager
    {
        public int id { get; set; }
        public string name { get; set; }
    }

    //Manager collections

    public class managerTeams
    {
        public int clubManagerId { get; set; }
        public List<managerTeamI> allTeams { get; set; }
    }

    public class managerTeamI
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}