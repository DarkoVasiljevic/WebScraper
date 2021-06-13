
using HtmlAgilityPack;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace WebScraper
{
    public class Program
    {
        private static readonly string url = 
            ConfigurationManager.AppSettings.Get("URL_l");

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

