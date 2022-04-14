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

        }
    }
}

/// <summary>
///     
/// </summary>

Console.WriteLine("Hello, World!");
IWebDriver browserDriver = new ChromeDriver();
browserDriver.Navigate().GoToUrl("");
//driver.get("file:///C:/Users/User/Desktop/index.html")

//setup - sets up the browser (like max size)
//navigate to gelato URL twitter feed
//grab the date of the posting
//compare the latest posting time with the new posting time
//-if there was a new posting:
//grab the latest twitter feed content and place contents in a data structure
//clean the data and ensure its expected format and content
//compare flavors to whitelisted flavors
//-if whitelisted flavors found, send SMS text to phno