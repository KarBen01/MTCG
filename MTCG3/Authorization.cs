using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{
    public class Authorization
    {
        private readonly DB db = new DB();
        private readonly SHA512 hasher = new SHA512Managed();
        private readonly HashSet<string> tokens = new HashSet<string>();
        public string Register(string pUsername, string pPassword)
        {
            if ((pUsername == null) || (pPassword == null))
                return "Missing Argument";


            string lHashedPW = GenerateHash(pPassword);
            User newUser = new User(pUsername, lHashedPW);
            if (db.AddUser(newUser))
            {
                return "Ok";
            }
            return "Error, User already exists";
        }

        public string Login(string pUsername, string pPassword)
        {
            var check = db.GetUser(pUsername);
            if (check == null) return "Falsche Anmeldedaten";
            if (GenerateHash(pPassword) == check.Password)
            {
                tokens.Add(pUsername + "-mtcgToken");
                return "Erfolgreich eingeloggt";
            }
            return "Falsche Anmeldedaten";
        }

        public bool isAuthenticated(string pToken)
        {
            if (tokens.Contains(pToken))
            {
                return true;
            }
            return false;
        }

        private string GenerateHash(string password)
        {
            string hash = string.Empty;
            var hashBuffer = hasher.ComputeHash(Encoding.ASCII.GetBytes(password));
            foreach (var b in hashBuffer) hash += b.ToString("x2");
            return hash;
        }
    }
}