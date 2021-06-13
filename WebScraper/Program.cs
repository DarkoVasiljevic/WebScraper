
using HtmlAgilityPack;
using System;
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
        private static readonly string url = "https://srh.bankofchina.com/search/whpj/searchen.jsp";

        public static void Main(string[] args)
        {
            try
            {
                new HtmlParser(new Scraper(url)).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Error] :: " + ex.Message);
            }

            Console.ReadKey();
        }
    }
}

