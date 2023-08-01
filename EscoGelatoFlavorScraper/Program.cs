//these using directives can be added as global using directives in another file
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V85.Debugger;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MySqlConnector;
using System.Data.Common;
using System.IO;
using System.Reflection;
/// <author> Shane Laskowski</author>
/// <summary>
/// This program is a webscraper that upon collecting data from twitter, sends a SMS text alert to a
/// personal phone number.  The program collects the frequently posted icecream flavors instock for
/// the local business EscoGelato.  If a favorited flavor is in stock for the day a text alert sent
/// informing the text reciever that their favorite flavor is in stock.
/// </summary>
/// <design_issues>
/// * Must account for a scenario where there are mutliple twitter postings that aren't flavor posting
/// * Must figure out a way to deal with daily flavor postings that are not in expected format
/// * Selenium is not an ideal webscraper, it is slower than other tools/apis.
/// </design_issues>

namespace EscoGelatoFlavorScraper
{
    class Program
    {
        //---It might be good to chuck the code of Main into a class and call that object in main.
        //---Should chuck an Exception Catch around methods in Main method and ditch the if-else blocks
        //---At this time app will not import customer flavor data from DB or file.  (personal use sofar)
        //---Perhaps add an Admin User that get's alerts if "isWellFormedFlavorPosting" is a "close-call".
        //---Consider adding Google Analytics to track app statistics and soforth.
        static void Main()
        {

            MyDbConnectionClass DbConnection = CreateDbConnector();
            DbConnection.IsConnect();

            List<(string, string)> allFlavorNames = QueryForFlavors(DbConnection);

            IWebDriver browserDriver = new ChromeDriver();
            SetUpBrowser(browserDriver);
            string postedMessege = GetPostedMessage(browserDriver);

            if (!isLatestFlavorPosting(browserDriver) || !isWellFormedFlavorPosting(postedMessege))
                exitProgram(browserDriver);
            else
            {
                //UpdateLatestFlavorPostingDate(the new date);
                List<string> flavorsInStock = ExtractFlavorsFromPosting(
                    FormatFlavorPostedData(postedMessege), 
                    FormatImportedAllFlavorsNameSet(allFlavorNames));

                //Query for Phno's and Fav Flavors using flavorsInStock, (place into objects that go into List)
                //firstname, favorite flavors in stock, phno.
                List<Tuple<string, List<string>, string>> CustomersToAlert = QueryForCustomersToAlert(flavorsInStock, DbConnection);

                //iterate through the customer list and send texts.
                DbConnection.Close();
                exitProgram(browserDriver);
            }
        }

        private static List<Tuple<string, List<string>, string>> QueryForCustomersToAlert(List<string> flavorsInStock, MyDbConnectionClass dbConnection)
        {
            string CustName = "";
            List<string> FavoriteFlavorsInstock = new List<string>();
            string phno = "";
            var custToAlert = new List<Tuple<string, List<string>, string>>();

            //write the string query using this methods parameters
            using var sqlcommand = new MySqlCommand("", dbConnection.Connection);
            using var sqlreader = sqlcommand.ExecuteReader();

            //run the query and return its result

            //iterate through result set and "combine" similar elements

            //return custToAlert

            throw new NotImplementedException();
        }

        /// <summary>
        /// grabs the string literals from this computers environment variables and creates a
        /// connection to the AWS Database for Escogelatoflavorscraper app.  Environmental
        /// variables are used to protect the passwords and other sensitive data from the public.
        /// </summary>
        /// <returns></returns>
        public static MyDbConnectionClass CreateDbConnector()
        {
            string? myserver = Environment.GetEnvironmentVariable("AWS_EscoGelato_DB_URL_ENDPOINT");
            string? mylogin = Environment.GetEnvironmentVariable("AWS_EscoGelato_Login_Username");
            string? mypass = Environment.GetEnvironmentVariable("AWS_EscoGelato_Login_Password");
            string? mydatabase = Environment.GetEnvironmentVariable("AWS_EscoGelato_DB_Name");
            string? myPort = Environment.GetEnvironmentVariable("AWS_EscoGelato_DB_URL_Port");
            MyDbConnectionClass Connection = new MyDbConnectionClass(myserver, mydatabase, mylogin, mypass, myPort);
            return Connection;
        }

        /// <summary>
        /// Gets the date and time of the latest flavor posting recorded by this program.  a file
        /// is used to keep this data persistant as this app quits.
        /// </summary>
        /// <returns></returns>
        public static DateTime? GetLatestFlavorPostingDate()
        {

            try
            {
                string? LatestFlavorPostingRecordedFileDirectory = Environment.GetEnvironmentVariable("LatestFlavorPostingDateFile");
                if (LatestFlavorPostingRecordedFileDirectory == "")
                    throw new Exception("file directory not set");
                else
                {
                    using (StreamReader sr = new StreamReader(LatestFlavorPostingRecordedFileDirectory))
                    {
                        //need to ensure sr isn't null
                        string? line = sr.ReadLine();
                        return DateTime.Parse(line);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return new DateTime();
            }
        }

        /// <summary>
        /// Updates the new flavor posting date discovered by this app.  It doesnt append but
        /// creates a new names.txt, overwriting it with the new date text.
        /// </summary>
        public static void UpdateLatestFlavorPostingDate(string newDate)
        {
            using (StreamWriter sw = new StreamWriter("LatestOfficialFlavorPostingDate.txt", false))
            {
                    sw.WriteLine(newDate);
            }
        }

        /// <summary>
        /// Querys the Database for all flavor names, both official and alias names.
        /// </summary>
        /// <param name="dataBaseConnection"></param>
        /// <returns>a list of the flavor names in the form of tuples consisting of two strings, (official, alias)</returns>
        public static List<(string, string)> QueryForFlavors(MyDbConnectionClass dataBaseConnection)
        {
            using var sqlcommand = new MySqlCommand("CALL GetOfficialAndAliasFlavorNames();", dataBaseConnection.Connection);
            using var sqlreader = sqlcommand.ExecuteReader();

            List<(string, string)> allExistingFlavorNames = new List<(string, string)>();
            string officialFlavorName = "";
            string aliasFlavorName = "";
            while (sqlreader.Read())
            {
                if (sqlreader.GetValue(0) != System.DBNull.Value)
                    officialFlavorName = sqlreader.GetString(0).Replace(" ", "");
                if (sqlreader.GetValue(1) != System.DBNull.Value)
                    aliasFlavorName = sqlreader.GetString(1).Replace(" ", "");
                allExistingFlavorNames.Add((officialFlavorName, aliasFlavorName));
                officialFlavorName = "";
                aliasFlavorName = "";
            }
            return allExistingFlavorNames;
        }

        /// <summary>
        /// Checks to see if posting is the latest posting by the company.  If it is not the latest 
        /// posting then it probably isn't the latest in-stock flavor announcement.
        /// </summary>
        /// <param name="browserDriver"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Should query all postings of the day from twitter feed to handle case
        /// of the business posting both a flavor annoucement and another unrelated annoucement 
        /// afterwards.</todo>
        private static bool isLatestFlavorPosting(IWebDriver browserDriver)
        {

            DateTime? dateOfLatestFlavorPosting = GetLatestFlavorPostingDate();
            DateTime dateOfAnnouncement = RetrieveAnnouncementDate(browserDriver);
            return (dateOfAnnouncement > dateOfLatestFlavorPosting ? true : false);
        }
        /// <summary>
        /// Gets the announcement posting from the business's website.
        /// </summary>
        /// <param name="browserDriver"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static string GetPostedMessage(IWebDriver browserDriver)
        {
            return browserDriver.FindElement(By.XPath("/html/body/div[3]/b/section[2]/div[1]/main/div/" +
                "div[2]/div/div/div/div/div/div/div[2]")).Text;
        }
        /// <summary>
        /// whitelisted "favorite" flavors
        /// </summary>
        /// <returns></returns>
        /// <todo>should import customer data from DB or file</todo>
        private static List<string> ImportFavoriteFlavors()
        {
            //throw new NotImplementedException();
            List<string> favorites = 
                new List<string> { "Chocolate", "Vanilla", "Strawberry", "Snickers", "Pecan"};
            Customer hardcodedCustomer = new Customer("shane", "laskowski", "0123456789", favorites);
            return (hardcodedCustomer.FavoriteFlavors);
        }
        /// <summary>
        /// Ensure that this passes the argument by reference
        /// </summary>
        /// <param name="driver"></param>
        /// <exception cref="NotImplementedException"></exception>
        static void SetUpBrowser(IWebDriver driver)
        {
            //"C:\Users\zaggn\OneDrive\Desktop\EscoGelato\EscoGelato – Gelato, Coffee & Panini
            //in downtown Escondido.html"
            //***the navigate string should be passed into this method, not hard-coded.
            driver.Navigate().GoToUrl("file:///C:/Users/zaggn/OneDrive/Desktop/SOFTWARE%20TESTING/EscoGelato/EscoGelato%20%E2%80%93%20Gelato,%20Coffee%20&%20Panini%20in%20downtown%20Escondido.html");
            driver.Manage().Window.Maximize();
        }

        /// <summary>
        /// Grabs the dates of the latest postings on the announcement feed.  Need to add the 
        /// amount of postings grabbed.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        static DateTime RetrieveAnnouncementDate(IWebDriver driver)
        {
            DateTime dateOfAnnouncement;

            //click on the href link label of the posting time
            driver.FindElement(By.XPath("/html/body/div[3]/b/section[2]/div[1]/main/div/div[2]/" +
                "div/div/div/div/div/div/div[2]/span[1]/a")).Click();
            driver.SwitchTo().Window(driver.WindowHandles[1]);

            //add an explicit wait here here!!!!!!!!!!!!!!
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(30);

            //locate the label with info of time and date of the posting
            IWebElement dateText =
            driver.FindElement(By.XPath("/html/body/div[1]/div/div/div[2]/main/div/div/div/" +
            "div/div/section/div/div/div[1]/div/div/article/div/div/div[3]/div[5]/div/" +
            "div[1]/div/div/a/time"));

            dateOfAnnouncement = convertTwitterDateTextToDateTime(dateText.Text);
            //dateOfAnnouncement = new DateTime();

            //return that datetime info
            return (dateOfAnnouncement);
        }
        /// <summary>
        /// Converts the specific format Twitter uses for their timestamp of a twitter posting
        /// from a string to a DateTime object.  Will be used to determine if the post being
        /// analyzed is in-fact the latest announcement.
        /// </summary>
        /// <param name="dateText"></param>
        /// <returns>DateTime Object representation of the time the announcement was posted</returns>
        /// <todo>Need to handle Possiable Exceptions in this method</todo>
        private static DateTime convertTwitterDateTextToDateTime(string dateText)
        {
            DateTime date;
            int indexOfSpecialPointChar = dateText.IndexOf('·');
            if (indexOfSpecialPointChar != -1)
            {
                if (DateTime.TryParse(dateText.Remove(indexOfSpecialPointChar, 2), out date))
                    return date;
                else throw new FormatException("Could not parse date text.");
            }
            else throw new FormatException("Text did not contain expected '·' character in the time" +
                " & date label.");
        }

        /// <summary>
        /// Retieves the posted annoucement text for future analysis
        /// </summary>
        /// <param name="driver">The interface that controls the webbrowser. 
        /// Used to navigate webpages and retrieve data</param>
        /// <returns>text of the annoucement in the form of IWebElement for analysis.</returns>
        /// <exception cref="NotImplementedException"></exception>
        static IWebElement RetrievePostedMessage(IWebDriver driver)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// To ensure that the announcement/posting is indeed a stocking update of flavors the
        /// text of the posting needs to be validated.  There is a chance that the annoucement
        /// feed might be used for postings unrelated to flavor postings.
        /// </summary>
        /// <param name="messagePosted"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        static bool isWellFormedFlavorPosting(string messagePosted)
        {
            string[] linesOfTheText = messagePosted.Split("\n");

            const int typicalMaxTotalLines = 25;
            const int typicalMinTotalLines = 10;
            const int typicalMaxLineLength = 30;
            //there is typically more than 10 lines per posting and less than 25
            if (linesOfTheText.Length > typicalMaxTotalLines || linesOfTheText.Length <
                typicalMinTotalLines)
                return false;

            //!!!!!!!!!!!!!!!!!! This method below is wrong, it doesnt consider the 1st line may be in xx/xx/xxxx format
           // if (!containsOneMonthStringText(linesOfTheText[0]))
             //   return false;

            //the 1st or 2nd line of text usually contains the word "flavors";
            if (linesOfTheText[0].IndexOf("flavors", StringComparison.OrdinalIgnoreCase) == -1 && 
                linesOfTheText[1].IndexOf("flavors", StringComparison.OrdinalIgnoreCase) == -1)
                return false;

            //Each line that lists flavors is typically "short", the 1st 3 lines don't contain flavors
            string s = "";
            for(int i = 3; i < linesOfTheText.Length; i++ )
            {
                s = linesOfTheText[i];
                if (s.Length > typicalMaxLineLength)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        private static bool containsOneMonthStringText(string inputText)
        {
            string[] months = { "january", "february", "march", "april", "may", "june", "july",
                "august", "september", "october", "november", "december" };
            int numMonthsCount = 0;
            foreach (string m in months)
            {
                if (inputText.IndexOf(m, StringComparison.OrdinalIgnoreCase) != -1)
                    numMonthsCount++;
            }

            return (numMonthsCount == 1 ? true : false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postedFlavorMessege"></param>
        /// <returns></returns>
        public static List<string> FormatFlavorPostedData(string postedFlavorMessege)
        {
            List<string> flavors = postedFlavorMessege.Split("\n").ToList();
            //first two or three lines are non-saliant therefore they are removed.
            flavors.RemoveAt(0);
            flavors.RemoveAt(0);
            if (flavors[0] == "\r")
                flavors.RemoveAt(0);
            int numberOfFlavors = flavors.Count;

            for (int i = 0; i < numberOfFlavors; i++)
            {
                flavors.Add(ReturnLowerCaseASCII(flavors[0].Replace(" ", "")));
                flavors.RemoveAt(0);
            }
            return flavors;
        }
        /// <summary>
        ///  lower-case + remove spaces
        /// </summary>
        /// <param name="allFlavorNames"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<(string, string)> FormatImportedAllFlavorsNameSet(List<(string, string)> allFlavorNames)
        {
            (string,string) flavorNamePair = ("", "");
            string officialFlavorName = "";
            string aliasFlavorName = "";
            for(int i = 0; i < allFlavorNames.Count; i++)
            {
                officialFlavorName = ReturnLowerCaseASCII(allFlavorNames[i].Item1.Replace(" ", ""));
                aliasFlavorName = ReturnLowerCaseASCII(allFlavorNames[i].Item2.Replace(" ", ""));
                flavorNamePair = (officialFlavorName, aliasFlavorName); //MUST be sure that item1 is officialname and not item2
                allFlavorNames.Add(flavorNamePair);
                allFlavorNames.RemoveAt(0);
            }
            return allFlavorNames;
        }

        /// <summary>
        /// Extracts the flavor strings mentioned inside the posting text
        /// </summary>
        /// <param name="flavorPosting">Flavor announcement of the newly instock flavors</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Remove Emojis And rename Flavor-Name Aliases</todo>
        static List<string> ExtractFlavorsFromPosting(List<string> postedFlavors, List<(string, string)> everyFlavorName)
        {
            List<string> RecognizedFlavors = new List<string>();
            foreach(var ef in everyFlavorName)
            {
                foreach(var flavor in postedFlavors)
                {
                    if (flavor == ef.Item1 || flavor == ef.Item2)
                        RecognizedFlavors.Add(ef.Item1);
                }
            }

            return RecognizedFlavors;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns>similar input string but its lower case and removes non-Alphabetical chars
        /// </returns>
        private static string ReturnLowerCaseASCII(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if ((int)c > 127)
                    continue;
                if ((int)c < 32) 
                    continue;
                sb.Append(Char.ToLower(c));
            }
            return sb.ToString();
        }
        /// <summary>Determines which, if any, favorite flavors are instock.</summary>
        /// <param name="favoriteFlavors"></param>
        /// <param name="flavorsInStock"></param>
        /// <returns>A collection of flavors that are instock and on the favorite's list</returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Consider using HashMap to reduce Time-Complexity.  Note that some flavors
        /// posted in the annoucements are short-handed (ex: "strawberry chip" == 
        static List<string> DetermineFavoriteFlavorsInStock(List<string> favoriteFlavors, 
            List<string> flavorsInStock)
        {
            List<string> favoriteFlavorsInstock = new List<string>();
            foreach(string flavor in flavorsInStock)
            {
                if (favoriteFlavors.Contains(flavor))
                    favoriteFlavorsInstock.Add(flavor);
            }
            return favoriteFlavorsInstock;
        }
        /// <summary>
        /// Sends a Text message using the Twilio API to alert a user that atleast one of their
        /// favorite flavors are instock.  The user will then be super happy that to know this
        /// information.
        /// </summary>
        /// <param name="instockFavorites"></param>
        /// <todo>Need to import user data and not just hardcode my contact info.</todo>
        /// <exception cref="NotImplementedException"></exception>
        static void SendFavoriteFlavorStockingAlert(List<string> instockFavorites)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Exits the program!
        /// </summary>
        /// <param name="driver">An interface with the webbrowser.  The browser needs to when 
        /// program exits so that the resources are not taken up on the computer.</param>
        /// <todo>May want to overload with exception parameter</todo>
        static void exitProgram(IWebDriver driver)
        {

            driver.Close();
            driver?.Quit();
            //call logging fuctions here
            Environment.Exit(0);
        }
    }
}