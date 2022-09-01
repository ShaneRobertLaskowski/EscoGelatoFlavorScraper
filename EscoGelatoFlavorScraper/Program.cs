//these using directives can be added as global using directives in another file
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

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
        //---Should chuck an Exception Catch around methods in Main method and ditch the if-else control
        //---At this time app will not import customer flavor data from DB or file.  (personal use sofar)
        //---Perhaps add an Admin User that get's alerts if "isWellFormedFlavorPosting" is a close-call.
        //---Consider adding Google Analytics to track app statistics and soforth.
        static void Main()
        {
            IWebDriver browserDriver = new ChromeDriver();
            SetUpBrowser(browserDriver);
            string postedMessege = GetPostedMessage(browserDriver);
            if (!isLatestFlavorPosting(browserDriver) || !isWellFormedFlavorPosting(postedMessege))
                exitProgram(browserDriver); //add logging you dummy!
            else
            {
                //-import a general flavor list and pass to this ExtractFlavorsFromPosting method?
                List<string> flavorsInStock = ExtractFlavorsFromPosting(postedMessege);
                List<string> favoriteFlavors = ImportFavoriteFlavors();
                List<string> favoriteFlavorsInStock =
                    DetermineFavoriteFlavorsInStock(favoriteFlavors, flavorsInStock);
                if (favoriteFlavorsInStock.Count >= 1)
                    SendFavoriteFlavorStockingAlert(favoriteFlavorsInStock);
                exitProgram(browserDriver);
            }
        }
        /// <summary>
        /// Checks to see if posting is the newest posting.  If it is not the latest posting then
        /// it probably isn't the latest in-stock flavor announcement.
        /// </summary>
        /// <param name="browserDriver"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Should query all postings of the day from twitter feed to handle case
        /// of the business posting both a flavor annoucement and another unrelated annoucement 
        /// afterwards.</todo>
        private static bool isLatestFlavorPosting(IWebDriver browserDriver)
        {
            DateTime dateOfLatestFlavorPosting = RetrieveLatestRecordedFlavorPostingDate();
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
            driver.Navigate().GoToUrl("file:///C:/Users/zaggn/OneDrive/Desktop/EscoGelato/EscoGelato%20%E2%80%93%20Gelato,%20Coffee%20&%20Panini%20in%20downtown%20Escondido.html");
            driver.Manage().Window.Maximize();
        }
        /// <summary>
        /// Gets the date/time of the latest confirmed flavor announcement.  Used to determine if
        /// an annoucement to be analyzed is the most recent, non-stale information.
        /// </summary>
        /// <returns></returns>
        /// <todo>Should retrieve this persistant data from DB or stored file</todo>
        private static DateTime RetrieveLatestRecordedFlavorPostingDate()
        {
            //connect to DB or file and query this data.
            return new DateTime(1990, 10, 20, 13, 22, 30);
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
            "div/div/section/div/div/div[1]/div/div/div/article/div/div/div/div[3]/" +
            "div[4]/div/div[1]/a[1]/span"));

            //Convert dateText string to dateOfAnnoucement Datetime
            dateOfAnnouncement = convertTwitterDateTextToDateTime(dateText.Text);
            dateOfAnnouncement = new DateTime();

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
            throw new NotImplementedException();
            /*Review the typical flavor postings and analyze the characteristics of the postings
            There are typically more than 10 flavors posted the first line usually contains a date 
            and the word "Flavors!". Each line is typically short. Consider using a collection of 
            flavors and check how many lines match with each flavor. Note- they post flavors on one
            of their webpages, However not every flavor is mentioned.  Note-- "Strawberry Chocolate 
            Chip" can be referred as "Strawberry Chip".
            */
        }
        /// <summary>
        /// Extracts the flavor strings mentioned inside the posting text
        /// </summary>
        /// <param name="flavorPosting">Flavor announcement of the newly instock flavors</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /// <todo>Remove Emojis And rename Flavor-Name Aliases</todo>
        static List<string> ExtractFlavorsFromPosting(string flavorPosting)
        {
            List<string> flavors = flavorPosting.Split("\n").ToList();
            flavors.RemoveAt(0);
            flavors.RemoveAt(0);
            //flavors.Trim();
            int numberOfFlavors = flavors.Count;

            for(int i = 0; i < numberOfFlavors; i++)
            {
                flavors.Add(ReturnLowerCaseASCII(flavors[0]));
                flavors.RemoveAt(0);
            }

            return flavors;
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