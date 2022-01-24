using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MTCG3
{
    public class Game
    {
        private readonly DB db = new DB();

        public string InitiateFight(User pPlayerOne, User pPlayerTwo)
        {
            string lRetVal = String.Empty;
            pPlayerOne.Deck = db.GetDeck(pPlayerOne.Username);
            pPlayerTwo.Deck = db.GetDeck(pPlayerTwo.Username);

            UserStats lPlayerOneStats = db.GetUserStats(pPlayerOne.Username);
            UserStats lPlayerTwoStats = db.GetUserStats(pPlayerTwo.Username);

            Card lPlayerOnePrev = null, lPlayerTwoPrev = null, lPlayerOneCard = null, lPlayerTwoCard = null;

            int lPlayerOneCardNum, lPlayerTwoCardNum, rounds = 1, sameElementPlayerOne = 0, sameElementPlayerTwo = 0;
            bool cardOneHasEffect = true, cardTwoHasEffect = true;
            Random randomNumber = new Random();
            while (pPlayerOne.Deck.Count > 0 && pPlayerTwo.Deck.Count > 0 && rounds <= 100)
            {

                lPlayerOneCardNum = randomNumber.Next(pPlayerOne.Deck.Count);
                lPlayerTwoCardNum = randomNumber.Next(pPlayerTwo.Deck.Count);
                lPlayerOneCard = pPlayerOne.Deck[lPlayerOneCardNum];
                lPlayerTwoCard = pPlayerTwo.Deck[lPlayerTwoCardNum];

                Console.WriteLine("\n--- Round: " + rounds + " ---");
                Console.WriteLine(pPlayerOne.Username + " battles his " + lPlayerOneCard.Name + " against " +
                                  pPlayerTwo.Username + "'s " + lPlayerTwoCard.Name + "\n");

                if (lPlayerOnePrev != null && string.Equals(lPlayerOneCard.Element, lPlayerOnePrev.Element))
                {
                    sameElementPlayerOne++;
                }
                if (lPlayerTwoPrev != null && string.Equals(lPlayerTwoCard.Element, lPlayerTwoPrev.Element))
                {
                    sameElementPlayerTwo++;
                }

                if (sameElementPlayerOne == 3)
                {
                    Console.WriteLine("\nOH NO! " + pPlayerOne.Username + " played the same Element three times in a ROW. Element has no positive effect this round\n");
                    cardOneHasEffect = false;
                    sameElementPlayerOne = 0;
                }
                if (sameElementPlayerTwo == 3)
                {
                    Console.WriteLine("\nOH NO! " + pPlayerTwo.Username + " played the same Element three times in a ROW. Element has no positive effect this round\n");

                    cardTwoHasEffect = false;
                    sameElementPlayerTwo = 0;
                }

                switch (BattleCards(lPlayerOneCard, lPlayerTwoCard, cardOneHasEffect, cardTwoHasEffect))
                {
                    case 0: //Draw
                        Console.WriteLine("Draw");
                        break;

                    case 1: //PlayerOne won
                        Console.WriteLine(pPlayerOne.Username + " wins with his " + lPlayerOneCard.Name);
                        pPlayerOne.Deck.Add(lPlayerTwoCard);
                        pPlayerTwo.Deck.RemoveAt(lPlayerTwoCardNum);
                        break;

                    case 2: //PlayerTwo won
                        Console.WriteLine(pPlayerTwo.Username + " wins with his " + lPlayerTwoCard.Name);
                        pPlayerTwo.Deck.Add(lPlayerOneCard);
                        pPlayerOne.Deck.RemoveAt(lPlayerOneCardNum);
                        break;

                    case -1: //Error
                        Console.WriteLine("Error with battle");
                        break;
                }
                rounds++;
                lPlayerOnePrev = lPlayerOneCard;
                lPlayerTwoPrev = lPlayerTwoCard;
            }

            if (pPlayerOne.Deck.Count == 0)
            {
                lRetVal = pPlayerTwo.Username + " WON";
                lPlayerOneStats.Elo -= 5;
                lPlayerTwoStats.Elo += 3;
                lPlayerOneStats.Looses += 1;
                lPlayerTwoStats.Wins += 1;
            }
            else if (pPlayerTwo.Deck.Count == 0)
            {
                lRetVal = pPlayerOne.Username + " WON";
                lPlayerOneStats.Elo += 3;
                lPlayerTwoStats.Elo -= 5;
                lPlayerOneStats.Wins += 1;
                lPlayerTwoStats.Looses += 1;
            }
            else
            {
                lRetVal = "It's a DRAW";
                lPlayerOneStats.Draws += 1;
                lPlayerTwoStats.Draws += 1;
            }


            if (db.UpdateUserStats(lPlayerOneStats) && db.UpdateUserStats(lPlayerTwoStats))
            {
                return lRetVal;
            }

            return "Could not Update user Stats";

        }



        public int BattleCards(Card pCardOne, Card pCardTwo, bool pCardOneHasEffect, bool pCardTwoHasEffect)
        {
            Console.WriteLine(pCardOne.PrintFightCard());
            Console.WriteLine(pCardTwo.PrintFightCard());
            if (pCardOne.Type == type.Spell && pCardTwo.Type == type.Spell) //PlayerA and PlayerB played Spells
            {
                return GetCardWinner(pCardOne, pCardTwo, pCardOneHasEffect, pCardTwoHasEffect);
            }
            if (pCardOne.Type != type.Spell && pCardTwo.Type != type.Spell)
            {
                switch (pCardOne.Type)
                {
                    case type.Elf when pCardOne.Element == element.Fire && pCardTwo.Type == type.Dragon:
                        Console.WriteLine(pCardOne.Name + " evade attacks from " + pCardTwo.Name);
                        return 1;
                    case type.Dragon when pCardTwo.Type == type.Goblin:
                        Console.WriteLine(pCardTwo.Name + " is afraid of " + pCardOne.Name);
                        return 1;
                    case type.Wizard when pCardTwo.Type == type.Ork:
                        Console.WriteLine(pCardOne.Name + " controls " + pCardTwo.Name);
                        return 1;

                    case type.Dragon when pCardTwo.Element == element.Fire && pCardTwo.Type == type.Elf:
                        Console.WriteLine(pCardTwo.Name + " evade attacks from " + pCardOne.Name);
                        return 2;
                    case type.Goblin when pCardTwo.Type == type.Dragon:
                        Console.WriteLine(pCardOne.Name + " is afraid of " + pCardTwo.Name);
                        return 2;
                    case type.Ork when pCardTwo.Type == type.Wizard:
                        Console.WriteLine(pCardTwo.Name + " controls " + pCardOne.Name);
                        return 2;
                    default:
                        return GetCardWinnerPureMonster(pCardOne, pCardTwo);
                }
            }
            if (pCardOne.Type != type.Spell && pCardTwo.Type == type.Spell)
            {
                switch (pCardOne.Type)
                {
                    case type.Kraken:
                        Console.WriteLine(pCardOne.Name + " is immune against Spells. " + pCardTwo.Name + " was not effective");
                        return 1;
                    case type.Knight when pCardTwo.Element == element.Water:
                        Console.WriteLine(pCardOne.Name + " drowns instantly from " + pCardTwo.Name);
                        return 2;
                    default:
                        return GetCardWinner(pCardOne, pCardTwo, pCardOneHasEffect, pCardTwoHasEffect);
                }
            }
            if (pCardOne.Type == type.Spell && pCardTwo.Type != type.Spell)
            {
                switch (pCardTwo.Type)
                {
                    case type.Knight when pCardOne.Element == element.Water:
                        Console.WriteLine(pCardTwo.Name + " drowns instantly from " + pCardOne.Name);
                        return 1;

                    case type.Kraken:
                        Console.WriteLine(pCardTwo.Name + " is immune against Spells. " + pCardOne.Name + " was not effective");
                        return 2;

                    default:
                        return GetCardWinner(pCardOne, pCardTwo, pCardOneHasEffect, pCardTwoHasEffect);
                }
            }
            return -1;
        }

        public double CalcMultiplier(element pElementOne, element pElementTwo, bool pCardHasEffect)
        {
            double lMultiplier = 1;
            if (pElementOne == pElementTwo)
            {
                return 1;
            }
            switch (pElementOne)
            {
                case element.Water when pElementTwo == element.Fire:
                    lMultiplier = 2;
                    break;
                case element.Fire when pElementTwo == element.Normal:
                    lMultiplier = 2;
                    break;
                case element.Normal when pElementTwo == element.Water:
                    lMultiplier = 2;
                    break;
                case element.Fire when pElementTwo == element.Water:
                    lMultiplier = 0.5;
                    break;
                case element.Normal when pElementTwo == element.Fire:
                    lMultiplier = 0.5;
                    break;
                case element.Water when pElementOne == element.Normal:
                    lMultiplier = 0.5;
                    break;
                default:
                    break;
            }

            if (!pCardHasEffect && lMultiplier > 1)
            {
                lMultiplier = 1;
            }

            return lMultiplier;
        }

        public int GetCardWinner(Card pCardOne, Card pCardTwo, bool pCardOneHasEffect, bool pCardTwoHasEffect)
        {
            if (pCardOne.Damage * CalcMultiplier(pCardOne.Element, pCardTwo.Element, pCardOneHasEffect) < pCardTwo.Damage * CalcMultiplier(pCardTwo.Element, pCardOne.Element, pCardTwoHasEffect))
            {
                return 2;
            }
            if (pCardOne.Damage * CalcMultiplier(pCardOne.Element, pCardTwo.Element, pCardOneHasEffect) == pCardTwo.Damage * CalcMultiplier(pCardTwo.Element, pCardOne.Element, pCardTwoHasEffect))
            {
                return 0;
            }
            if (pCardOne.Damage * CalcMultiplier(pCardOne.Element, pCardTwo.Element, pCardOneHasEffect) > pCardTwo.Damage * CalcMultiplier(pCardTwo.Element, pCardOne.Element, pCardTwoHasEffect))
            {
                return 1;
            }

            return -1;
        }

        public int GetCardWinnerPureMonster(Card CardOne, Card CardTwo)
        {
            if (CardOne.Damage == CardTwo.Damage)
            {
                return 0;
            }
            if (CardOne.Damage > CardTwo.Damage)
            {
                return 1;
            }
            if (CardOne.Damage < CardTwo.Damage)
            {
                return 2;
            }
            return -1;
        }
    }
}
