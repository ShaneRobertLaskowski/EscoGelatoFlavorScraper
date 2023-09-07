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
    /// <summary>
    ///     EscoGelato posts their in-stock flavors on their website.  This program scrapes that flavor
    ///     posting data and alerts users via text messege if their favorite flavors are in-stock.
    /// </summary>
    /// <issues>
    ///     *Ensure that resources are properly disposed of (database connection + webdriver dependencies)
    ///     **EscoGelato is rebuilding their website => need to switch to local website for testing => isLatestFlavorPosting will always return true
    ///     *Refactor code
    /// </issues>
    /// <changes>
    ///     *Added comments, migrated to use of "Chrome For Testing" browser, expanded the SetUpBrowser method, forced isLatestFlavorPosting to return true, removed CreateDbConnector
    /// </changes>>
    /// <todo>
    ///     *implement Twilio reporting feature
    ///     *setup a scheduler to run this program once a day or so.
    ///     *Add logging to the exitprogram method
    ///     *Add parameter to ExitProgram Method to close DB connection
    ///     *seperate business logic from methods, no hard-coded data in methods.
    /// </todo>
    class Program
    {
        //---Should chuck an Exception Catch around methods in Main method and ditch the if-else blocks
        //---Perhaps add an Admin User that get's alerts if "isWellFormedFlavorPosting" is a "close-call".
        //---Consider adding Google Analytics to track app statistics and soforth.
        static void Main()
        {
            string? dbServerEndpoint = Environment.GetEnvironmentVariable("EscoGelato_DB_Endpoint");
            string? dbServerPortNum = Environment.GetEnvironmentVariable("EscoGelato_DB_Port");
            string? dbName = Environment.GetEnvironmentVariable("EscoGelato_DB_Name");
            string? userName = Environment.GetEnvironmentVariable("EscoGelato_DB_UserName");
            string? userPassword = Environment.GetEnvironmentVariable("EscoGelato_DB_Password");
            
            MyDbConnectionClass DbConnection = new MyDbConnectionClass(dbServerEndpoint, dbName,
                userName, userPassword, dbServerPortNum);

            DbConnection.IsConnect();

            List<(string, string)> allFlavorNames = QueryForFlavors(DbConnection);

            IWebDriver browserDriver = SetUpBrowser("C:\\Users\\zaggn\\OneDrive\\Desktop\\chrome-win64\\chrome.exe",
                "file:///C:/Users/zaggn/OneDrive/Desktop/SOFTWARE%20TESTING/EscoGelato/EscoGelato%20%E2%80%93%20Gelato,%20Coffee%20&%20Panini%20in%20downtown%20Escondido.html");
            
            string postedMessege = GetPostedMessage(browserDriver);

            //islatestFlavorPosting will always return True as of now.
            if (!isLatestFlavorPosting(browserDriver) || !isWellFormedFlavorPosting(postedMessege))
                //just add the DB close inside he ExitProgram Method as argument
                exitProgram(browserDriver);
            else
            {
                //Add UpdateLatestFlavorPostingDate(the new date) method;

                List<string> flavorsInStock = ExtractFlavorsFromPosting(
                    FormatFlavorPostedData(postedMessege), 
                    FormatImportedAllFlavorsNameSet(allFlavorNames));

                //Query for Phno's and Fav Flavors using flavorsInStock, (place into objects that go into List)
                //firstname, favorite flavors in stock, phno.
                List<Tuple<string, List<string>, string>> CustomersAndFlavorsForAlerting = QueryForCustomersToAlert(flavorsInStock, DbConnection);

                //iterate through the customer list and send texts (including what favorite flavor names of theirs are instock).

                //clean and tear down program (close DB connection and exit browser(s)).
                //just add the DB close inside he ExitProgram Method as argument
                DbConnection.Close();
                exitProgram(browserDriver);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flavorsInStock"></param>
        /// <param name="dbConnection"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<Tuple<string, List<string>, string>> QueryForCustomersToAlert(List<string> flavorsInStock, MyDbConnectionClass dbConnection)
        {
            string CustName = "";
            List<string> FavoriteFlavorsInstock = new List<string>();
            string phno = "";
            var custToAlert = new List<Tuple<string, List<string>, string>>();

            //write the string query using this methods parameters
            using var sqlcommand = new MySqlCommand("", dbConnection.Connection);

            //run the query and return its result
            using var sqlreader = sqlcommand.ExecuteReader();

            //iterate through result set and "combine" similar elements

            //return custToAlert

            throw new NotImplementedException();
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
            return true; 

            DateTime? dateOfLatestFlavorPosting = GetLatestFlavorPostingDate();
            DateTime dateOfAnnouncement = RetrieveAnnouncementDate(browserDriver);
            //return (dateOfAnnouncement > dateOfLatestFlavorPosting ? true : false);
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
        /// Ensure that this passes the argument by reference
        /// </summary>
        /// <param name="driver"></param>
        /// <exception cref="NotImplementedException"></exception>
        static IWebDriver SetUpBrowser(string browswerBinaryLoc, string URL)
        {
            ChromeOptions ch = new ChromeOptions();
            ch.BinaryLocation = browswerBinaryLoc;
            IWebDriver browserDriver = new ChromeDriver(ch);
            browserDriver.Navigate().GoToUrl(URL);
            browserDriver.Manage().Window.Maximize();

            return browserDriver;
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
        /// <returns>A list of flavor names and associated alias names of flavor names.</returns>
        /// <issue>allFlavorNames is having elements being replaced by other elements</issue>
        public static List<(string, string)> FormatImportedAllFlavorsNameSet(List<(string, string)> flavors)
        {
            (string,string) flavorNamePair = ("", "");
            string officialFlavorName = "";
            string aliasFlavorName = "";
            List<(string, string)> allFlavorNames = new List<(string, string)>();

            for(int i = 0; i < flavors.Count; i++)
            {
                officialFlavorName = ReturnLowerCaseASCII(flavors[i].Item1.Replace(" ", ""));
                aliasFlavorName = ReturnLowerCaseASCII(flavors[i].Item2.Replace(" ", ""));
                flavorNamePair = (officialFlavorName, aliasFlavorName);
                allFlavorNames.Add(flavorNamePair);
            }
            return allFlavorNames;
        }

        /// <summary>
        /// Extracts the flavor strings mentioned inside the posting text
        /// </summary>
        /// <param name="flavorPosting">Flavor announcement of the newly instock flavors</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Remove Emojis And rename Flavor-Name Aliases, optimize the algorithm</todo>
        /// <issues>
        /// This method is returning multiple Repeated flavor names
        /// in everyFlavorName i spotted repeated flavors in the list ex: "vanilla, creamyvanilla"
        /// consider possiablity of postedFlavors containing a "" value
        /// </issues>
        static List<string> ExtractFlavorsFromPosting(List<string> postedFlavors, List<(string, string)> everyFlavorName)
        {
            List<string> RecognizedFlavors = new List<string>();
            foreach (var flavor in postedFlavors)
            {
                foreach(var ef in everyFlavorName)
                {
                    if (flavor == ef.Item1 || flavor == ef.Item2)
                    {
                        RecognizedFlavors.Add(ef.Item1);
                        break;
                    }
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