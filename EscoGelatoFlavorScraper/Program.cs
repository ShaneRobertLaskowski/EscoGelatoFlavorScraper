//these using directives can be added as global using directives in another file
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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

        static void Main()
        {
            //1:browser setup
            IWebDriver browserDriver = new ChromeDriver();
            SetUpBrowser(browserDriver);

            //2: load-in persistant data using a DB or text file.
            DateTime latestRecordedFlavorPosting = RetrieveLatestRecordedFlavorPosting();

            //3: retrieve the announcement posting, end program if the posting is not new
            DateTime dateOfCurrentPosting = RetrieveLatestAnnouncementDate(browserDriver);
            if (!IsMostRecentPosting(dateOfCurrentPosting, latestRecordedFlavorPosting))
                exitProgram(browserDriver);

            //4: select the most recent, correct/well formed FLAVOR posting
            IWebElement postedMessage = RetrievePostedMessage(browserDriver);
            if(!VerifyPostingCorrectness(postedMessage))
                exitProgram(browserDriver);

            //5: extract the data from the lastest, correct flavor posting
            List<string> flavorsInStock = ExtractFlavorsFromPosting(postedMessage);
            
            //6: import whitelisted "favorite" flavors
            List<string> favoriteFlavors = ImportFavoriteFlavors();
            
            //7: compare whitelisted flavors with those instock
            List<string> favoriteFlavorsInStock = FavoriteFlavorsInStock(favoriteFlavors, flavorsInStock);
            
            //8: send text alert regarding any favorite flavors that are in stock.
            if(favoriteFlavorsInStock.Count >= 1)
                SendFavoriteFlavorStockingAlert(favoriteFlavorsInStock);
            
            exitProgram(browserDriver);
        }

        private static List<string> ImportFavoriteFlavors()
        {
            //throw new NotImplementedException();
            return (new List<string> { "Chocolate", "Vanilla", "Strawberry", "Snickers", "Pecan" });
        }

        /// <summary>
        /// Ensure that this passes the argument by reference
        /// </summary>
        /// <param name="driver"></param>
        /// <exception cref="NotImplementedException"></exception>
        static void SetUpBrowser(IWebDriver driver)
        {
            //browserDriver.Navigate().GoToUrl("");
            //driver.get("file:///C:/Users/User/Desktop/index.html")
            //setup - sets up the browser (like max size)
            throw new NotImplementedException();
        }
        private static DateTime RetrieveLatestRecordedFlavorPosting()
        {
            //throw new NotImplementedException();
            return new DateTime(1990, 10, 20, 13, 22, 30);
        }
        /// <summary>
        /// Grabs the dates of the latest postings on the announcement feed.  Need to add the 
        /// amount of postings grabbed.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
        static DateTime RetrieveLatestAnnouncementDate(IWebDriver driver)
        {
            //grab the dates of the posting
            DateTime datesOfAnnouncement = new DateTime(2022, 6, 16);
            return (datesOfAnnouncement);
        }

        static bool IsMostRecentPosting(DateTime prospectivePostingDate, DateTime latestProcesedPostingDate)
        {
            //compare the newly grabbed posting date with the date of the most recent posting that was processsed
            throw new NotImplementedException();
        }
        static IWebElement RetrievePostedMessage(IWebDriver driver)
        {
            throw new NotImplementedException();
        }
        static bool VerifyPostingCorrectness(IWebElement messagePosted)
        {
            throw new NotImplementedException();
        }
        static List<string> ExtractFlavorsFromPosting(IWebElement flavorPosting)
        {
            throw new NotImplementedException();
        }
        static List<string> FavoriteFlavorsInStock(List<string> favoriteFlavors, List<string> flavorsInStock)
        {
            throw new NotImplementedException();
        }
        static void SendFavoriteFlavorStockingAlert(List<string> instockFavorites)
        {
            throw new NotImplementedException();
        }
        static void exitProgram(IWebDriver driver)
        {
            driver.Close();
            driver?.Quit();
            Environment.Exit(0);
        }
    }
}