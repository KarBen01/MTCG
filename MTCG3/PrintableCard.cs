using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{
    public class PrintableCard
    {
        public string Id { get; }
        public double Damage { get; }

        public element Element { get; }
        public type Type { get; }

        public double PackageId { get; }
        public bool Deck { get; }

        public string Username { get; }

        public PrintableCard(string pName, element pElement, type pType, double pDamage, double pPackageId, string pUsername, bool pDeck)
        {
            Id = pName;
            Damage = pDamage;
            Element = pElement;
            Type = pType;
            PackageId = pPackageId;
            Username = pUsername;
            Deck = pDeck;
        }

        public string PrintCardAllStats()
        {
            string lRetVal = "ID: " + Id + "\n" +
                             "Name: " + Element.ToString() + Type.ToString() + "\n" +
                             "Damage: " + Damage + "\n" +
                             "Contained in Package Number: " + PackageId + "\n" +
                             "Owned by: " + Username + "\n";
            if (Deck)
            {
                lRetVal += "Currently in Deck\n";
            }
            else
            {
                lRetVal += "Currently not in Deck\n";
            }

            return lRetVal;
        }

        public string PrintCardDeckStats(string pFormat)
        {
            string lRetVal = String.Empty;
            if (pFormat == "plain")
            {
                lRetVal += "ID: " + Id + "\n" +
                                 "Name: " + Element.ToString() + Type.ToString() + "\n" +
                                 "Damage: " + Damage + "\n";
                return lRetVal;
            }
            lRetVal += Id + ": " + Element.ToString() + Type.ToString() + "\n";
            return lRetVal;
        }
    }
}
