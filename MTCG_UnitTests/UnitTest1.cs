using System;
using System.Collections.Generic;
using MTCG3;
using NUnit.Framework;

namespace MTCG_UnitTests
{
    public class Tests
    {
       
        [Test]
        public void AddUserTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            Assert.AreEqual("Ok", lAuth.Register("benjamin", "karafiat"));
            Assert.AreEqual("Ok", lAuth.Register("admin", "testpw123"));
        }

        [Test]
        public void AddUserTest_Fail()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Register("admin", "testpw123");
            Assert.AreEqual("Error, User already exists", lAuth.Register("benjamin", "karafiat"));
            Assert.AreEqual("Error, User already exists", lAuth.Register("admin", "testpw123"));
        }

        [Test]
        public void LoginUserTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Register("admin", "testpw123");
            Assert.AreEqual("Erfolgreich eingeloggt", lAuth.Login("benjamin", "karafiat"));

            Assert.AreEqual("Erfolgreich eingeloggt", lAuth.Login("admin", "testpw123"));

        }

        [Test]
        public void LoginUserTest_Failed()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Register("admin", "testpw123");
            Assert.AreEqual("Falsche Anmeldedaten", lAuth.Login("benjamin", "12345678"));

            Assert.AreEqual("Falsche Anmeldedaten", lAuth.Login("admin", "abcdefg"));

        }

        [Test]
        public void IsAuthenticatedTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
           
            lAuth.Login("benjamin", "karafiat");
            
            Assert.IsTrue(lAuth.isAuthenticated("benjamin-mtcgToken"));
        }

        [Test]
        public void IsAuthenticatedTest_Fail()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
           
            Assert.IsFalse(lAuth.isAuthenticated("benjamin"));
           
        }

        [Test]
        public void CreatePackageTest_Success()
        {
            DB db = new DB();
            Assert.IsTrue(db.AddPackage());
        }

        [Test]
        public void CreatePachakgeWithCardsTest_Success()
        {
            DB db = new DB();
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            Assert.IsTrue(db.AddCard(newCard,packageId));
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            Assert.IsTrue(db.AddCard(newCard, packageId));
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            Assert.IsTrue(db.AddCard(newCard, packageId));
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            Assert.IsTrue(db.AddCard(newCard, packageId));
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            Assert.IsTrue(db.AddCard(newCard, packageId));
        }

        [Test]
        public void AcquireCardsTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            Assert.IsTrue(db.BuyPackage("benjamin"));
        }

        [Test]
        public void AcquireCardsTestWrongUser_Failed()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            Assert.IsFalse(db.BuyPackage("bernhard"));
        }

        [Test]
        public void AcquireCardsTestNoMoney_Failed()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            db.BuyPackage("benjamin");
            db.BuyPackage("benjamin");
            db.BuyPackage("benjamin");
            db.BuyPackage("benjamin");
            Assert.IsFalse(db.BuyPackage("benjamin"));
        }

        [Test]
        public void ShowCardsTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            db.BuyPackage("benjamin");

            List<PrintableCard> CardList = new List<PrintableCard> { };
            CardList = db.GetCards("benjamin");
            Assert.IsNotEmpty(CardList);
        }

        [Test]
        public void ShowDeckTestNoDeck_Fail()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            db.BuyPackage("benjamin");

            List<PrintableCard> CardList = new List<PrintableCard> { };
            CardList = db.GetPrintableDeck("benjamin");
            Assert.IsEmpty(CardList);
        }

        [Test]
        public void ConfigureDeckTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            db.BuyPackage("benjamin");
            List<string> ids = new List<string>() { "845f0dc7-37d0-426e-994e-43fc3ac83c08", "644808c2-f87a-4600-b313-122b02322fd5", "b017ee50-1c14-44e2-bfd6-2c0c5653a37c", "ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", "d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8" };
            Assert.IsTrue(db.SetDeck("benjamin", ids));
        }

        [Test]
        public void ShowDeckTest_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.AddPackage();
            int packageId = db.getPackageId();
            Card newCard = new Card("845f0dc7-37d0-426e-994e-43fc3ac83c08", element.Water, type.Goblin, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("644808c2-f87a-4600-b313-122b02322fd5", element.Fire, type.Dragon, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("b017ee50-1c14-44e2-bfd6-2c0c5653a37c", element.Normal, type.Spell, Double.Parse("11.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", element.Normal, type.Dragon, Double.Parse("10.0"));
            db.AddCard(newCard, packageId);
            newCard = new Card("d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8", element.Water, type.Knight, Double.Parse("9.0"));
            db.AddCard(newCard, packageId);

            db.BuyPackage("benjamin");
            List<string> ids = new List<string>() { "845f0dc7-37d0-426e-994e-43fc3ac83c08", "644808c2-f87a-4600-b313-122b02322fd5", "b017ee50-1c14-44e2-bfd6-2c0c5653a37c", "ed1dc1bc-f0aa-4a0c-8d43-1402189b33c8", "d7d0cb94-2cbf-4f97-8ccf-9933dc5354b8" };
            db.SetDeck("benjamin", ids);
            List<PrintableCard> CardList = new List<PrintableCard> { };
            CardList = db.GetPrintableDeck("benjamin");
            Assert.AreEqual(5, CardList.Count);
        }

        [Test]
        public void ShowUserData_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
           
            UserStats newUserStats = db.GetUserStats("benjamin");
            Assert.AreEqual("No Data", newUserStats.Name);
            Assert.AreEqual("No Data", newUserStats.Bio);
            Assert.AreEqual("", newUserStats.Image);
        }

        [Test]
        public void EditUserData_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");
            db.EditUserData("benjamin", "Benjamin Karafiat", "Gaming...",
                ":D");
            UserStats newUserStats = db.GetUserStats("benjamin");

            Assert.AreEqual("Benjamin Karafiat", newUserStats.Name);
            Assert.AreEqual("Gaming...", newUserStats.Bio);
            Assert.AreEqual(":D", newUserStats.Image);
        }

        [Test]
        public void GetScoreboard_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Login("benjamin", "karafiat");

            List<UserStats> newUserStats = db.GetScoreboard();
            Assert.AreEqual("benjamin",newUserStats[0].Username);
        }


        [Test]
        public void EditElo_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Register("stefan", "kudernatsch");
            lAuth.Login("benjamin", "karafiat");

            UserStats UserStats = db.GetUserStats("benjamin");
            Assert.AreEqual(1000, UserStats.Elo);
            UserStats.Elo -= 5;
            db.UpdateUserStats(UserStats);
            UserStats newUserStats = db.GetUserStats("benjamin");
            Assert.AreEqual(995, newUserStats.Elo);
        }

        [Test]
        public void ScoreboardChanged_Success()
        {
            DB db = new DB();
            Authorization lAuth = new Authorization();
            lAuth.Register("benjamin", "karafiat");
            lAuth.Register("stefan", "kudernatsch");
            lAuth.Login("benjamin", "karafiat");

            List<UserStats> newScoreboard = db.GetScoreboard();
            Assert.AreEqual("benjamin", newScoreboard[0].Username);            
            UserStats UserStats = db.GetUserStats("benjamin");
            UserStats.Elo -= 5;
            db.UpdateUserStats(UserStats);
            newScoreboard = db.GetScoreboard();
            Assert.AreEqual("stefan", newScoreboard[0].Username);
        }
    }
}