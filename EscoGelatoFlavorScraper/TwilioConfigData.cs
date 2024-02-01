using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscoGelatoFlavorScraper
{
    public static class TwilioConfigData
    {
        private readonly static string? twilioUserName;
        private readonly static string? twilioSID;
        private readonly static string? twilioSourcePhNo;
        private readonly static string? twilioRegisteredPhNo;

        public static string? TwilioUserName => twilioUserName;
        public static string? TwilioSID => twilioSID;
        public static string? TwilioSourcePhNo => twilioSourcePhNo;
        public static string? TwilioRegisteredPhNo => twilioRegisteredPhNo;

        static TwilioConfigData()
        {
            twilioUserName = Environment.GetEnvironmentVariable("YOUR_TWILIO_ACCOUNT_SID");
            twilioSID = Environment.GetEnvironmentVariable("YOUR_TWILIO_AUTH_TOKEN");
            twilioSourcePhNo = Environment.GetEnvironmentVariable("TWILIO_PHNO");
            twilioRegisteredPhNo = Environment.GetEnvironmentVariable("TWILIO_REGISTERED_PHNO");
        }
    }
}
