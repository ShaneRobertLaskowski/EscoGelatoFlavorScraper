## EscoGelatoFlavorScraper

* * *

### Summary

```
This application webscrapes a local business's website for their Twitter feed which
contains their instock flavors for the week.  This data is then compared to a database of customers
and a collection of those customers' favorite flavors.  Each customer that has a favorite flavor instock
will then recieve a text message informing them that their favorite flavors are instock.  This solves an
issue with customers not feeling inclined to check the company's Twitter feed often.
Tech: C#, 6.0.NET, Selenium Webdriver, Twilio API, MySQL RDMS
Notice: An older version of their website's files are included for the purposes of testing the webscraping.
```

### Prerequisites

What things you need to install and what you need to do

```
Unfortunately, The remote instance of MySQL hosted by AWS is now costing me $,
therefore i have migrated the instance to my a personal server,
therefore getting Customer data is unavailable to the public developers at this time.

Installation of Visual Studio along with your own Twilio API key

https://www.twilio.com/en-us?utm_source=google&utm_medium=cpc&utm_term=twilio&utm_campaign=G_S_NAMER_Brand_ROW_RLSA&cq_plac=&cq_net=g&cq_pos=&cq_med=&cq_plt=gp&gad_source=1&gclid=CjwKCAiA_aGuBhACEiwAly57MUnlJOcZB8U-U1H10EwLzeVgNErC_xlhLd7MSSjBQwdYgzixaGWTbxoCh7AQAvD_BwE

At a glance, many variables are initalized via Environment Variables.  You must create your own Environment Variables, name them accordingly, and give them appropriate values
such as assigning your personal Twilio Key to one.

```
* * *

### Author(s)

* **Shane Laskowski**
-------------------------
