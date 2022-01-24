using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{

    public enum element
    {
        Fire,
        Water,
        Normal
    }

    public enum type
    {
        Spell,
        Goblin,
        Dragon,
        Wizard,
        Ork,
        Knight,
        Kraken,
        Elf,
        Troll
    }
    public class Card
    {

        public string Id { get; }

        public string Name { get; }
        public double Damage { get; }

        public element Element { get; }
        public type Type { get; }

        public Card(string pName, element pElement, type pType, double pDamage)
        {
            Id = pName;
            Name = pElement.ToString() + pType.ToString();
            Damage = pDamage;
            Element = pElement;
            Type = pType;
        }

        public string PrintFightCard()
        {
            return "Name: " + Name + "\n" +
                   "Element: " + Element.ToString() + "\n" +
                   "Type:" + Type.ToString() + "\n" +
                   "Damage: " + Damage + "\n";
        }
    }
}