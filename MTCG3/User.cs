using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{
    public class User
    {
        public string Username { get; }
        public string Password { get; }
        public List<Card> Deck = new List<Card> { };
        public User(string pUsername, string pPassword)
        {
            Username = pUsername;
            Password = pPassword;
        }

    }
}