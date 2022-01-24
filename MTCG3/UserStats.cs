using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{
    public class UserStats
    {
        public string Username { get; set; }
        public int Elo { get; set; }
        public int Wins { get; set; }
        public int Looses { get; set; }
        public int Draws { get; set; }
        public int Coins { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public string Image { get; set; }

        public UserStats(string username)
        {
            Username = username;
            Elo = 1000;
            Wins = 0;
            Looses = 0;
            Draws = 0;
            Coins = 20;
            Name = "No Data";
            Bio = "No Data";
            Image = "";
        }


        public UserStats(string pUsername, int pElo, int pWins, int pLooses, int pDraws)
        {
            Username = pUsername;
            Elo = pElo;
            Wins = pWins;
            Looses = pLooses;
            Draws = pDraws;
            Name = string.Empty;
            Bio = string.Empty;
            Image = string.Empty;
        }

        public UserStats(
            string pUsername, int pElo, int pWins, int pLooses, int pDraws, int pCoins, string pName, string pBio, string pImage
        )
        {
            Username = pUsername;
            Elo = pElo;
            Wins = pWins;
            Looses = pLooses;
            Draws = pDraws;
            Coins = pCoins;
            Name = pName;
            Bio = pBio;
            Image = pImage;
        }

        public string PrintUserData()
        {
            string lRetVal = "Username: " + Username + "    " + Image + "\n" +
                             "Name: " + Name + "\n" +
                             "Bio: " + Bio + "\n" +
                             "Available Coins: " + Coins + "\n";
            return lRetVal;
        }

        public string PrintUserStats()
        {
            string lRetVal = "User: " + Username + "\n" +
                             "Current Elo: " + Elo + "\n" +
                             "Wins/Looses/Draws: " + Wins + "/" + Looses + "/" + Draws + "\n";
            return lRetVal;
        }

        public string PrintElo()
        {
            string lRetVal = Username + ", Elo: " + Elo + "\n";
            return lRetVal;
        }
    }
}

