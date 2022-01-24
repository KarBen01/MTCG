using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace MTCG3
{
    public class DB
    {
        private static string user = "postgres", password = "postgres", ip = "localhost", db = "mtcg";
        private static int port = 5432;
        private static string connString = $"Server={ip};Port={port};User Id={user};Password={password}; Database={db};";


        public DB()
        {
            ClearDB();
        }

        private static NpgsqlConnection Connection()
        {
            var conn = new NpgsqlConnection(connString);
            conn.Open();
            return conn;
        }



        public bool BuyPackage(string pUsername)
        {
            int packageCost = 5;
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            NpgsqlDataReader? dr = null;
            try
            {
                var userStats = GetUserStats(pUsername);
                if (userStats is null) return false;
                if (userStats.Coins - packageCost < 0) return false;

                using var buyCmd = new NpgsqlCommand(
                    "UPDATE stats " +
                    "SET coins=@pCoins WHERE username=@pUsername"
                    , conn);
                buyCmd.Parameters.AddWithValue("pCoins", userStats.Coins - packageCost);
                buyCmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Bigint;
                buyCmd.Parameters.AddWithValue("pUsername", pUsername);
                buyCmd.Parameters[1].NpgsqlDbType = NpgsqlDbType.Varchar;
                buyCmd.ExecuteNonQuery();

                using var getPackageCmd = new NpgsqlCommand(
                    "SELECT * from packages LIMIT 1",
                    conn);
                getPackageCmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Integer)
                { Direction = ParameterDirection.Output });
                getPackageCmd.ExecuteNonQuery();
                int packageId;
                if (getPackageCmd.Parameters[0].Value is int value) packageId = value;
                else return false;

                using var cardQueryCmd = new NpgsqlCommand(
                    "SELECT * from cards WHERE package = @p1",
                    conn);
                cardQueryCmd.Parameters.AddWithValue("p1", packageId);
                cardQueryCmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Integer;

                dr = cardQueryCmd.ExecuteReader();
                var cards = new List<Card>();
                while (dr.Read())
                {
                    element lElement;
                    Enum.TryParse(dr.GetString(2), out lElement);
                    type lType;
                    Enum.TryParse(dr.GetString(3), out lType);
                    cards.Add(new Card(
                        dr.GetString(0), lElement, lType, dr.GetDouble(1)));
                }
                dr.Close();

                using var cardUpdateCmd = new NpgsqlCommand(
                    "UPDATE cards " +
                    "SET username = @p1 WHERE id = @p2 AND package = @p3",
                    conn);
                cardUpdateCmd.Parameters.AddWithValue("p1", pUsername);
                cardUpdateCmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Varchar;
                cardUpdateCmd.Parameters.Add("p2", NpgsqlDbType.Varchar);
                cardUpdateCmd.Parameters.AddWithValue("p3", packageId);
                cardUpdateCmd.Parameters[2].NpgsqlDbType = NpgsqlDbType.Integer;
                foreach (var card in cards)
                {
                    cardUpdateCmd.Parameters[1].Value = card.Id;
                    cardUpdateCmd.ExecuteNonQuery();
                }

                using var packageDeleteCmd = new NpgsqlCommand(
                    "DELETE FROM packages WHERE id=@p1",
                    conn);
                packageDeleteCmd.Parameters.AddWithValue("p1", packageId);
                packageDeleteCmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Integer;
                packageDeleteCmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                dr?.Close();
                return false;
            }
        }


        public List<UserStats> GetScoreboard()
        {
            using var conn = Connection();

            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT * FROM stats ORDER BY elo DESC LIMIT 100",
                    conn);
                NpgsqlDataReader DataReader = cmd.ExecuteReader();
                var scoreboard = new List<UserStats>();
                while (DataReader.Read())
                {
                    var username = DataReader.GetString(0);
                    var elo = DataReader.GetInt32(1);
                    var wins = DataReader.GetInt32(2);
                    var looses = DataReader.GetInt32(3);
                    var draws = DataReader.GetInt32(4);
                    scoreboard.Add(new UserStats(username, elo, wins, looses, draws));
                }
                DataReader.Close();

                return scoreboard;
            }
            catch (Exception)
            {
                return new List<UserStats>();
            }
        }

        public bool UpdateUserStats(UserStats pUserStats)
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            try
            {
                using var userStatsUpdateCmd = new NpgsqlCommand(
                    "UPDATE stats " +
                    "SET elo = @pElo, wins = @pWins, looses = @pLooses, draws = @pDraws, coins = @pCoins " +
                    "WHERE username = @pUsername",
                    conn);

                userStatsUpdateCmd.Parameters.AddWithValue("pElo", NpgsqlDbType.Integer, pUserStats.Elo);
                userStatsUpdateCmd.Parameters.AddWithValue("pWins", NpgsqlDbType.Integer, pUserStats.Wins);
                userStatsUpdateCmd.Parameters.AddWithValue("pLooses", NpgsqlDbType.Integer, pUserStats.Looses);
                userStatsUpdateCmd.Parameters.AddWithValue("pDraws", NpgsqlDbType.Integer, pUserStats.Draws);
                userStatsUpdateCmd.Parameters.AddWithValue("pCoins", NpgsqlDbType.Integer, pUserStats.Coins);
                userStatsUpdateCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUserStats.Username);

                userStatsUpdateCmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool EditUserData(string pUsername, string pName, string pBio, string pImage)
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            try
            {
                using var userStatsUpdateCmd = new NpgsqlCommand(
                    "UPDATE stats " +
                    "SET name = @pName, bio = @pBio, image = @pImage " +
                    "WHERE username = @pUsername",
                    conn);

                userStatsUpdateCmd.Parameters.AddWithValue("pName", NpgsqlDbType.Varchar, pName);
                userStatsUpdateCmd.Parameters.AddWithValue("pBio", NpgsqlDbType.Varchar, pBio);
                userStatsUpdateCmd.Parameters.AddWithValue("pImage", NpgsqlDbType.Varchar, pImage);
                userStatsUpdateCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);

                userStatsUpdateCmd.ExecuteNonQuery();
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddUser(User pUser)
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            try
            {
                using var userCmd = new NpgsqlCommand(
                    "INSERT INTO users (username, password) " +
                    "VALUES(@pUsername, @pPassword)",
                    conn);

                userCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUser.Username);
                userCmd.Parameters.AddWithValue("pPassword", NpgsqlDbType.Varchar, pUser.Password);

                var userStats = new UserStats(pUser.Username);
                using var statsCmd = new NpgsqlCommand(
                    "INSERT INTO stats " +
                    "VALUES(@pUsername, @pElo, @pWins, @pLooses, @pDraws, @pCoins, @pName, @pBio, @pImage)",
                    conn);

                statsCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, userStats.Username);
                statsCmd.Parameters.AddWithValue("pElo", NpgsqlDbType.Bigint, userStats.Elo);
                statsCmd.Parameters.AddWithValue("pWins", NpgsqlDbType.Bigint, userStats.Wins);
                statsCmd.Parameters.AddWithValue("pLooses", NpgsqlDbType.Bigint, userStats.Looses);
                statsCmd.Parameters.AddWithValue("pDraws", NpgsqlDbType.Bigint, userStats.Draws);
                statsCmd.Parameters.AddWithValue("pCoins", NpgsqlDbType.Bigint, userStats.Coins);
                statsCmd.Parameters.AddWithValue("pName", NpgsqlDbType.Varchar, userStats.Name);
                statsCmd.Parameters.AddWithValue("pBio", NpgsqlDbType.Varchar, userStats.Bio);
                statsCmd.Parameters.AddWithValue("pImage", NpgsqlDbType.Varchar, userStats.Image);

                userCmd.ExecuteNonQuery();
                statsCmd.ExecuteNonQuery();
                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public UserStats GetUserStats(string pUsername)
        {
            using var conn = Connection();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT * FROM stats WHERE username=@pUsername",
                    conn);
                cmd.Parameters.AddWithValue("pUsername", pUsername);
                cmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Varchar;

                cmd.Parameters.Add(new NpgsqlParameter("elo", NpgsqlDbType.Bigint)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("wins", NpgsqlDbType.Bigint)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("looses", NpgsqlDbType.Bigint)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("draws", NpgsqlDbType.Bigint)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("coins", NpgsqlDbType.Bigint)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("name", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("bio", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("image", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();
                if (cmd.Parameters[1].Value != null &&
                    cmd.Parameters[2].Value != null &&
                    cmd.Parameters[3].Value != null &&
                    cmd.Parameters[4].Value != null &&
                    cmd.Parameters[5].Value != null &&
                    cmd.Parameters[6].Value != null &&
                    cmd.Parameters[7].Value != null &&
                    cmd.Parameters[8].Value != null)
                {
                    return new UserStats(
                        pUsername,
                        (int)cmd.Parameters[1].Value!,
                        (int)cmd.Parameters[2].Value!,
                        (int)cmd.Parameters[3].Value!,
                        (int)cmd.Parameters[4].Value!,
                        (int)cmd.Parameters[5].Value!,
                        (string)cmd.Parameters[6].Value!,
                        (string)cmd.Parameters[7].Value!,
                        (string)cmd.Parameters[8].Value!);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public bool AddPackage()
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            try
            {
                using var packCmd = new NpgsqlCommand(
                    "INSERT INTO packages (id)" +
                    "VALUES(default)",
                    conn);

                packCmd.ExecuteNonQuery();

                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int getPackageId()
        {
            using var conn = Connection();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT * FROM packages ORDER BY ID DESC LIMIT 1",
                    conn);


                cmd.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });


                cmd.ExecuteNonQuery();
                if (cmd.Parameters[0].Value != null)
                {
                    return (int)cmd.Parameters[0].Value!;
                }

                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public bool AddCard(Card pCard, int pPackageNum)
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction == null) return false;
            try
            {
                using var cardCmd = new NpgsqlCommand(
                    "INSERT INTO cards (id, damage, element, type, package, username, deck) " +
                    "VALUES(@pCardname, @pDamage, @pElement, @pType, @pPackage, @pUsername, @pDeck)",
                    conn);

                cardCmd.Parameters.AddWithValue("pCardname", NpgsqlDbType.Varchar, pCard.Id);
                cardCmd.Parameters.AddWithValue("pDamage", NpgsqlDbType.Bigint, pCard.Damage);
                cardCmd.Parameters.AddWithValue("pElement", NpgsqlDbType.Varchar, pCard.Element.ToString());
                cardCmd.Parameters.AddWithValue("pType", NpgsqlDbType.Varchar, pCard.Type.ToString());
                cardCmd.Parameters.AddWithValue("pPackage", NpgsqlDbType.Bigint, pPackageNum);
                cardCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, "");
                cardCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, false);


                cardCmd.ExecuteNonQuery();

                transaction.Commit();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<PrintableCard> GetCards(string pUsername)
        {
            using var conn = Connection();
            NpgsqlDataReader? dr = null;
            try
            {
                using var cardQueryCmd = new NpgsqlCommand(
                    "SELECT * from cards WHERE username = @pUsername",
                    conn);
                cardQueryCmd.Parameters.AddWithValue("pUsername", pUsername);
                cardQueryCmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Varchar;
                dr = cardQueryCmd.ExecuteReader();
                var cards = new List<PrintableCard>();
                while (dr.Read())
                {
                    var id = dr.GetString(0);
                    var lDamage = dr.GetDouble(1);
                    var lPackageId = dr.GetDouble(4);
                    var lDeck = dr.GetBoolean(6);

                    element lElement;
                    Enum.TryParse(dr.GetString(2), out lElement);
                    type lType;
                    Enum.TryParse(dr.GetString(3), out lType);

                    cards.Add(new PrintableCard(
                        id, lElement, lType, lDamage, lPackageId, pUsername, lDeck
                    ));
                }

                dr.Close();
                return cards;
            }
            catch (Exception)
            {
                dr?.Close();
                return new List<PrintableCard>();
            }
        }

        public bool SetDeck(string pUsername, List<string> pIds)
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);
            if (transaction is null) return false;
            try
            {
                using var emptyDeckCmd = new NpgsqlCommand(
                    "UPDATE cards " +
                    "SET deck = @pDeck WHERE username = @pUsername",
                    conn);
                emptyDeckCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, false);
                emptyDeckCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);
                emptyDeckCmd.ExecuteNonQuery();

                using var setDeckCmd = new NpgsqlCommand(
                    "UPDATE cards " +
                    "SET deck = @pDeck WHERE id = @pId AND username = @pUsername",
                    conn);
                setDeckCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, true);
                setDeckCmd.Parameters.Add("pId", NpgsqlDbType.Varchar);
                setDeckCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);
                foreach (var Id in pIds)
                {
                    setDeckCmd.Parameters[1].Value = Id;
                    setDeckCmd.ExecuteNonQuery();
                }

                using var checkDeckCountCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM cards " +
                    "WHERE username = @pUsername AND deck = @pDeck",
                    conn);
                checkDeckCountCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);
                checkDeckCountCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, true);
                var result = checkDeckCountCmd.ExecuteScalar();
                // Check if all cards were properly configured
                // or if some ids were misleading (e.g. not user-owned cards)
                if ((result == null || (long)result != pIds.Count) && (result == null || (int)result != pIds.Count))
                    return false;
                transaction.Commit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public List<PrintableCard> GetPrintableDeck(string pUsername)
        {
            using var conn = Connection();
            NpgsqlDataReader? dr = null;
            try
            {
                using var cardQueryCmd = new NpgsqlCommand(
                    "SELECT * from cards WHERE username = @pUsername AND deck = @pDeck",
                    conn);
                cardQueryCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);
                cardQueryCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, true);
                dr = cardQueryCmd.ExecuteReader();
                var cards = new List<PrintableCard>();
                while (dr.Read())
                {
                    var id = dr.GetString(0);
                    var lDamage = dr.GetDouble(1);
                    var lPackageId = dr.GetDouble(4);
                    var lDeck = dr.GetBoolean(6);

                    element lElement;
                    Enum.TryParse(dr.GetString(2), out lElement);
                    type lType;
                    Enum.TryParse(dr.GetString(3), out lType);

                    cards.Add(new PrintableCard(
                        id, lElement, lType, lDamage, lPackageId, pUsername, lDeck
                    ));
                }

                dr.Close();
                return cards;
            }
            catch (Exception)
            {
                dr?.Close();
                return new List<PrintableCard>();
            }
        }

        public List<Card> GetDeck(string pUsername)
        {
            using var conn = Connection();
            NpgsqlDataReader dr = null;
            try
            {
                using var cardQueryCmd = new NpgsqlCommand(
                    "SELECT * from cards WHERE username = @pUsername AND deck = @pDeck",
                    conn);
                cardQueryCmd.Parameters.AddWithValue("pUsername", NpgsqlDbType.Varchar, pUsername);
                cardQueryCmd.Parameters.AddWithValue("pDeck", NpgsqlDbType.Boolean, true);
                dr = cardQueryCmd.ExecuteReader();
                var cards = new List<Card>();
                while (dr.Read())
                {
                    var id = dr.GetString(0);
                    var lDamage = dr.GetDouble(1);

                    element lElement;
                    Enum.TryParse(dr.GetString(2), out lElement);
                    type lType;
                    Enum.TryParse(dr.GetString(3), out lType);

                    cards.Add(new Card(id, lElement, lType, lDamage));
                }

                dr.Close();
                return cards;
            }
            catch (Exception)
            {
                dr?.Close();
                return new List<Card>();
            }
        }

        public User GetUser(string pUsername)
        {
            using var conn = Connection();
            try
            {
                using var cmd = new NpgsqlCommand(
                    "SELECT * FROM users WHERE username=@pUsername",
                    conn);

                cmd.Parameters.AddWithValue("pUsername", pUsername);
                cmd.Parameters[0].NpgsqlDbType = NpgsqlDbType.Varchar;

                cmd.Parameters.Add(new NpgsqlParameter("username", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("password", NpgsqlDbType.Varchar)
                { Direction = ParameterDirection.Output });

                cmd.ExecuteNonQuery();
                if (cmd.Parameters[1].Value != null &&
                    cmd.Parameters[2].Value != null)
                {
                    return new User(
                        (string)cmd.Parameters[1].Value!,
                        (string)cmd.Parameters[2].Value!);
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }



        public void ClearDB()
        {
            using var conn = Connection();
            var transaction = BeginTransaction(conn);

            using var clearUsersTable = new NpgsqlCommand(
                "DELETE FROM users;",
                conn);
            clearUsersTable.ExecuteNonQuery();

            using var clearStatsTable = new NpgsqlCommand(
                "DELETE FROM stats;",
                conn);
            clearStatsTable.ExecuteNonQuery();

            using var clearCardsTable = new NpgsqlCommand(
                "DELETE FROM cards;",
                conn);
            clearCardsTable.ExecuteNonQuery();

            using var clearPackagesTable = new NpgsqlCommand(
                "DELETE FROM packages;",
                conn);
            clearPackagesTable.ExecuteNonQuery();

            transaction.Commit();
        }

        private NpgsqlTransaction BeginTransaction(NpgsqlConnection conn)
        {
            for (var i = 0; i < 15; i++)
            {
                try
                {
                    var transaction = conn.BeginTransaction();
                    return transaction;
                }
                catch (InvalidOperationException)
                {
                    Thread.Sleep(50);
                }
            }
            return null;
        }
    }
}
