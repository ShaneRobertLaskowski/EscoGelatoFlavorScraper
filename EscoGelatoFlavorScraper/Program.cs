//these using directives can be added as global using directives in another file
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

//need to add Environment Variables for Twilio API, phno, file directory of testing site
namespace EscoGelatoFlavorScraper
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Hello, World!");
            IWebDriver browserDriver = new ChromeDriver();
            //SetUpBrowser(browserDriver);
        }


        static void SetUpBrowser(IWebDriver driver)
        {
            //navigate to gelato URL twitter feed
            //browserDriver.Navigate().GoToUrl("");
            //driver.get("file:///C:/Users/User/Desktop/index.html")
            //setup - sets up the browser (like max size)
            throw new NotImplementedException();
        }
        static DateTime FetchLatestPostingDate(IWebDriver driver)
        {
            //grab the date of the posting
            throw new NotImplementedException();
        }

        static bool IsLatestProcessedPosting(DateTime prospectivePostingDate, DateTime latestProcesedPostingDate)
        {
            //compare the newly grabbed posting date with the date of the most recent posting that was processsed
            //the potentional issue with this method is that is doesn't handle the case of double postings by the business.
            throw new NotImplementedException();
        }
        static string[] ExtractFlavorsFromPosting(IWebDriver driver)
        {
            throw new NotImplementedException();
        }
        static string[] VerifyFavoriteFlavorsInStock(string[] favoriteFlavors, string[] flavorsInStock)
        {
            throw new NotImplementedException();
        }
        static void SendFavoriteFlavorStockingAlert(string[] instockFavorites)
        {
            throw new NotImplementedException();
        }
    }
}

//setup browser
//navigate to twitter posting webpage
//grab date of posting
//check to see if the posting is new and unprocessed
//grab the content of twitter posting and place into data structure
//verify the correctness/expectations of the twitter posting
//get whitelisted (favorite) flavors and compare to the posting of flavors
//send Text Message Alert to phone


/// <summary>
///     
/// </summary>
/// 
