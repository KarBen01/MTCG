using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MTCG3
{
    public class Program
    {
        static Authorization auth = new Authorization();
        static DB db = new DB();
        static Thread serverThread = null;
        static TcpListener Listener;
        private static ConcurrentQueue<User> userQueue;
        private static ConcurrentDictionary<string, Task> tasks;
        private static CancellationTokenSource tokenSource;
        private static Game game = new Game();

        static void Main(string[] args)
        {
            StartServer();
            StartCurl();
        }

        static void StartServer()
        {
            if (serverThread == null)
            {
                IPAddress ipAddress = new IPAddress(0);
                Listener = new TcpListener(ipAddress, 10001);
                serverThread = new Thread(ServerHandler);
                serverThread.Start();
                userQueue = new ConcurrentQueue<User>();
                tasks = new ConcurrentDictionary<string, Task>();
                tokenSource = new CancellationTokenSource();
            }
        }

        static String ReadRequest(NetworkStream stream)
        {
            MemoryStream contents = new MemoryStream();
            var buffer = new byte[2048];
            do
            {
                var size = stream.Read(buffer, 0, buffer.Length);
                if (size == 0)
                {
                    return null;
                }
                contents.Write(buffer, 0, size);
            } while (stream.DataAvailable);



            var retVal = Encoding.UTF8.GetString(contents.ToArray());
            return retVal;
        }

        static void ServerHandler(Object o)
        {
            List<string> parameters = new List<string> { };
            Listener.Start();
            while (true)
            {
                TcpClient client = Listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                string method = String.Empty;
                string authToken = String.Empty;
                string format = String.Empty;
                try
                {
                    var request = ReadRequest(stream);
                    //Console.WriteLine("\nNeuer Read\n");
                    string[] lines = request.Split('\n');
                    for (int i = 0; i < lines.Length - 1; i++)
                    {
                        if (i == 0)
                        {
                            string[] words = lines[i].Split(' ');
                            method = words[0];
                            words[1] = words[1].Remove(0, 1);
                            parameters = words[1].Split('/').ToList();
                        }
                        else if (lines[i].StartsWith("Authorization:"))
                        {
                            string[] words = lines[i].Split(' ');
                            authToken = words[2];
                            authToken = authToken.Substring(0, authToken.Length - 1);
                        }
                    }



                    string mainParam = parameters.First();

                    foreach (string s in parameters)
                    {
                        if (s.Contains("?"))
                        {
                            string[] tempStringArray = s.Split('?');
                            mainParam = tempStringArray[0];
                            string tempString = tempStringArray[1];
                            if (tempString.Contains("format"))
                                format = tempString.Substring(tempString.LastIndexOf('=') + 1);
                        }
                    }

                    string response = HandleRequest(method, mainParam, parameters, lines[lines.Length - 1], authToken, format);

                    var responseBuilder = new StringBuilder();
                    responseBuilder.AppendLine("HTTP/1.1 200 OK");
                    responseBuilder.AppendLine("Content-Type: text/html");
                    responseBuilder.AppendLine();
                    responseBuilder.AppendLine(response);
                    var responseString = responseBuilder.ToString();
                    var responseBytes = Encoding.UTF8.GetBytes(responseString);
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
            }
        }



        static void StartCurl()
        {
            ProcessStartInfo processInfo;
            Process process;

            try
            {
                Console.WriteLine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\MonsterTradingCards.exercise.curl.bat")));
                processInfo = new ProcessStartInfo(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\..\\MonsterTradingCards.exercise.curl.bat")));
                processInfo.CreateNoWindow = false;
                process = Process.Start(processInfo);
                Console.WriteLine("Bat started");
                process.WaitForExit();
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); Console.WriteLine("Bat NOT started"); }
        }

        static string HandleRequest(string pMethod, string pUrl, List<string> pParams, string pData, string pToken, string pFormat)
        {
            //Console.WriteLine($"PARAM: {pUrl}");
            switch (pMethod)
            {
                case "POST":
                    {
                        switch (pUrl)
                        {
                            case "users":
                                {
                                    dynamic lData = JObject.Parse(pData);
                                    string lRetVal = auth.Register(lData.Username.ToString(), lData.Password.ToString());
                                    return lRetVal;
                                }
                            case "sessions":
                                {
                                    dynamic lData = JObject.Parse(pData);
                                    string lRetVal = auth.Login(lData.Username.ToString(), lData.Password.ToString());
                                    return lRetVal;
                                }
                            case "packages":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        if (!db.AddPackage())
                                            return "Package not Added";
                                        int packageId = db.getPackageId();
                                        JArray array = JArray.Parse(pData);
                                        foreach (JObject obj in array.Children<JObject>())
                                        {
                                            List<string> cardProperties = new List<string> { };
                                            foreach (JProperty singleProp in obj.Properties())
                                            {
                                                cardProperties.Add(singleProp.Value.ToString());
                                            }

                                            element lElement;
                                            Enum.TryParse(cardProperties[1], out lElement);
                                            type lType;
                                            Enum.TryParse(cardProperties[2], out lType);
                                            Card newCard = new Card(cardProperties[0], lElement, lType, Double.Parse(cardProperties[3]));
                                            if (!db.AddCard(newCard, packageId))
                                            {
                                                return "Card not Added";
                                            }
                                        }
                                        return "Package Added";
                                    }
                                    return "Not authenticated";
                                }
                            case "transactions":
                                {
                                    if (pParams.Count > 1)
                                    {
                                        if (pParams[1] == "packages")
                                        {
                                            if (auth.isAuthenticated(pToken))
                                            {
                                                pToken = pToken.Substring(0, pToken.Length - 10);
                                                if (db.BuyPackage(pToken))
                                                {
                                                    return "Package bought succesfully";
                                                }
                                                return "Could not buy package";
                                            }

                                            return "Not authenticated";
                                        }
                                    }
                                    return "OK";
                                }
                            case "battles":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        User lUser = db.GetUser(pToken);
                                        if (lUser != null) { userQueue.Enqueue(lUser); }

                                        if (userQueue.Count == 2)
                                        {
                                            User lPlayerOne = null, lPlayerTwo = null;
                                            while (lPlayerOne == null) userQueue.TryDequeue(out lPlayerOne);
                                            while (lPlayerTwo == null) userQueue.TryDequeue(out lPlayerTwo);

                                            if (string.Equals(lPlayerOne.Username, lPlayerTwo.Username))
                                            {
                                                return "Can't battle oneself";
                                            }

                                            var token = tokenSource.Token;
                                            var id = Guid.NewGuid().ToString();
                                            var task = Task.Run(() => game.InitiateFight(lPlayerOne, lPlayerTwo), token);
                                            tasks[id] = task;
                                            task.ContinueWith(t => { tasks.TryRemove(id, out t!); }, token);

                                            return "OK";
                                        }
                                        else
                                        {
                                            Thread.Sleep(15);
                                            return "OK";
                                        }
                                    }
                                    return "Not authenticated";
                                }
                            default:
                                {
                                    return "Invalid argument";
                                }
                        }
                    }
                case "GET":
                    {
                        switch (pUrl)
                        {
                            case "users":
                                {
                                    if (pParams.Count < 2)
                                        return "Mising arguments";
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        if (pToken == pParams[1])
                                        {
                                            UserStats newUserStats = db.GetUserStats(pToken);
                                            return newUserStats.PrintUserData();
                                        }
                                    }
                                    return "Not authenticated";
                                }
                            case "cards":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        string lRetVal = "All Cards owned: \n";
                                        List<PrintableCard> CardList = new List<PrintableCard> { };

                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        CardList = db.GetCards(pToken);

                                        foreach (var Card in CardList)
                                        {
                                            lRetVal += "------------------------------------\n";
                                            lRetVal += Card.PrintCardAllStats();
                                        }
                                        lRetVal += "------------------------------------\n";
                                        return lRetVal;
                                    }
                                    return "Not authenticated";
                                }
                            case "deck":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        string lRetVal = "Current Deck: \n";
                                        List<PrintableCard> CardList = new List<PrintableCard> { };

                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        CardList = db.GetPrintableDeck(pToken);
                                        if (CardList.Count == 0)
                                        {
                                            return "Currently no Cards in Deck";
                                        }
                                        foreach (var Card in CardList)
                                        {
                                            lRetVal += "------------------------------------\n";
                                            lRetVal += Card.PrintCardDeckStats(pFormat);
                                        }
                                        lRetVal += "------------------------------------\n";
                                        return lRetVal;
                                    }
                                    return "Not authenticated";
                                }
                            case "stats":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);

                                        UserStats newUserStats = db.GetUserStats(pToken);
                                        return newUserStats.PrintUserStats();

                                    }
                                    return "Not authenticated";
                                }
                            case "score":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);

                                        List<UserStats> newUserStats = db.GetScoreboard();
                                        string lRetVal = String.Empty;
                                        for (int i = 0; i < newUserStats.Count; i++)
                                        {
                                            lRetVal += i + 1 + ": ";
                                            lRetVal += newUserStats[i].PrintElo();
                                        }
                                        return lRetVal;

                                    }
                                    return "Not authenticated";
                                }
                            default:
                                {
                                    return "Invalid argument";
                                }
                        }
                    }
                case "PUT":
                    {
                        switch (pUrl)
                        {
                            case "users":
                                {
                                    if (pParams.Count < 2)
                                        return "Mising arguments";
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        if (pToken == pParams[1])
                                        {
                                            dynamic lData = JObject.Parse(pData);
                                            if (db.EditUserData(pToken, lData.Name.ToString(), lData.Bio.ToString(),
                                                lData.Image.ToString()))
                                            {
                                                return "User updated";
                                            }

                                            return "User update failed";
                                        }
                                    }
                                    return "Not authenticated";
                                }
                            case "deck":
                                {
                                    if (auth.isAuthenticated(pToken))
                                    {
                                        pToken = pToken.Substring(0, pToken.Length - 10);
                                        List<string> ids = JsonConvert.DeserializeObject<List<string>>(pData);
                                        if (ids != null && ids.Count == 4)
                                        {
                                            if (db.SetDeck(pToken, ids))
                                                return "OK";
                                            return "Could not configure Deck";
                                        }

                                        return "Invalid amount of Cards";
                                    }
                                    return "Not authenticated";
                                }
                            default:
                                {
                                    return "Invalid argument";
                                }
                        }
                    }
                default:
                    {
                        return "Invalid method";
                    }
            }

        }
    }
}
