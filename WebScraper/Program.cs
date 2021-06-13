using System;
using System.Configuration;

namespace WebScraper
{
    public class Program
    {
        private static readonly string url = 
            ConfigurationManager.AppSettings.Get("URL");

        public static void Main(string[] args)
        {
            try
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                    new HtmlParser(new Scraper(url)).Start();
                else
                    throw new ApplicationException("Invalid URL string...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] :: " + ex.Message);
            }

            Console.ReadKey();
        }
    }
}

