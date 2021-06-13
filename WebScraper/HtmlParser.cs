using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Configuration;

namespace WebScraper
{
    public class HtmlParser
    {
        private readonly string OutputDirectoryDefault =
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\ScraperResult";

        public string OutputDirectoryFromConfig { get; private set; }

        public HtmlDocument Html { get; private set; }

        public string StartDate { get; private set; }

        public string EndDate { get; private set; }

        public static int StartingPage { get; private set; }

        public IList<string> Currencies { get; private set; }

        public List<List<string>> TableData { get; private set; }

        public bool NoRecords { get; private set; }

        public string OutputDirectory { get; private set; }

        public Scraper Scraper { get; private set; }

        public HtmlParser(Scraper sc)
        {
            Scraper = sc;

            Html = new HtmlDocument();
            TableData = new List<List<string>>();

            StartingPage = 1;
            NoRecords = false;
        }

        public void Start()
        {
            try
            {
                SetEndDate();
                SetStartDate();

                Console.WriteLine("------------------");
                Console.WriteLine("Start scrapping from URL: " + Scraper.Url);
                Console.WriteLine("From: " + StartDate + " To: " + EndDate);
                Console.WriteLine("\r\n------------------");

                LoadHtmlInit();
                
                SetCurrencies("option");
                CreateOutputDirectory();

                ParseHtmlDocument();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Start() => " + ex.Message);
            }

            Console.WriteLine("End of scrapping...");
            Console.WriteLine("------------------");
        }

        private void SetStartDate()
        {
            StartDate = Convert.ToDateTime(EndDate).Date.AddDays(-2).ToString("yyyy-MM-dd");
        }

        private void SetEndDate() => EndDate = DateTime.Now.ToString("yyyy-MM-dd");

        private void LoadHtmlInit()
        {
            Html.LoadHtml(Scraper.HttpGetString);
            //Console.WriteLine(Scraper.HttpGetString);
        }

        private void SetCurrencies(string desc)
        {
            Currencies = Html.DocumentNode.Descendants(desc)
                            .Select(e => e.InnerText.Trim() + " ")
                            .ToList()
                            ;
            if (Currencies == null)
                throw new ApplicationException("SetCurrencies()");
        }

        private void CreateOutputDirectory()
        {
            try
            {
                OutputDirectoryFromConfig =
                        ConfigurationManager.AppSettings.Get("OUTPUT_DIRECTORY");

                bool isValidPath = Path.IsPathFullyQualified(OutputDirectoryFromConfig);

                if (isValidPath)
                {
                    if (!Directory.Exists(OutputDirectoryFromConfig))
                        Directory.CreateDirectory(OutputDirectoryFromConfig);

                    OutputDirectory = OutputDirectoryFromConfig;
                    Console.WriteLine($"Output directory PATH " +
                        $"< { Path.GetFullPath(OutputDirectory) } >");
                    Console.WriteLine("------------------");
                }
                else
                {
                    Console.WriteLine($"Output directory PATH from App.config file" +
                   $" < { OutputDirectoryFromConfig } > is invalid...");
                    Console.WriteLine("------------------");

                    if (!Directory.Exists(OutputDirectoryDefault))
                        Directory.CreateDirectory(OutputDirectoryDefault);

                    OutputDirectory = OutputDirectoryDefault;
                    Console.WriteLine($"Deafult Output directory PATH" +
                        $" < { Path.GetFullPath(OutputDirectory) } > is created...");
                    Console.WriteLine("------------------");
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("CreateDirectory() => " + ex.Message);
            }
        }

        private void ParseHtmlDocument()
        {
            Currencies.Skip(1)
                .ToList()
                .ForEach(e => ParseHtmlDocumentByCurency(e))
                ;
        }

        public void ParseHtmlDocumentByCurency(string currency)
        {
            Console.WriteLine("\nSending POST methods...\n");

            int currentPage = StartingPage;
            int nextPage = StartingPage + 1;

            LoadHtmlPost(currency, currentPage.ToString());
            GetTableData(currentPage);

            while (true)
            {
                try
                {
                    Console.WriteLine("Currency: " + currency + " Page: " + currentPage);
                    
                    LoadHtmlPost(currency, nextPage.ToString());

                    string page = Html.DocumentNode.SelectSingleNode("/html/body/form/input[4]")
                                              .GetAttributeValue("value", "");

                    nextPage = Convert.ToInt32(page);

                    if (currentPage == nextPage || NoRecords)
                    {
                        CreateFileAndWriteData(currency);
                        StartingPage = 1;
                        NoRecords = false;

                        break;
                    }

                    GetTableData(nextPage);
                    
                    currentPage = nextPage;
                    nextPage++;
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("ParseHtmlDocumentByCurency() => " + ex.Message);
                } 
            }
        }

        private void CreateFileAndWriteData(string currency)
        {
            Console.WriteLine("\nCreating file...");
            string fileName = CreateFileAndOpen(currency);

            Console.WriteLine("\nWriting data...");
            WriteDataToFile(fileName);

            TableData = new List<List<string>>();
        }

        private void LoadHtmlPost(string currency, string page)
        {
            try
            {
                Scraper.HttpPost(CreatePostData(currency, page));
                //Console.WriteLine(Scraper.HttpPostString);

                Html.LoadHtml(Scraper.HttpPostString);
                //Console.WriteLine("Response from server:\r\n" + Scraper.HttpPostString);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("LoadHtmlPost() => " + ex.Message);
            }
        }

        private string CreatePostData(string currency, string page)
        {
            return new string( "erectDate=" + Uri.EscapeDataString(StartDate)
                             + "&nothing="  + Uri.EscapeDataString(EndDate)
                             + "&pjname="   + Uri.EscapeDataString(currency)
                             + "&page="     + Uri.EscapeDataString(page) );
        }

        public void GetTableData(int page)
        {
            var table = Html.DocumentNode.Descendants("table")
                               .Where(el => el.GetAttributeValue("width", "")
                                    .Equals("640"))
                               .FirstOrDefault();

            if (table == null)
                throw new ApplicationException("GetTableData()");

            table.SelectNodes("tr").ToList().ForEach(row =>
            {
                var singleLineData = new List<string>();

                row.SelectNodes("th|td").ToList().ForEach(cell =>
                {
                    if (cell.InnerText.Trim().StartsWith("sorry, no records") ||
                        row.SelectNodes("th|td").ToList().Count == 1)
                    {
                        singleLineData.Add("sorry, no records");

                        NoRecords = true;
                        return;
                    }

                    singleLineData.Add(cell.InnerText.Trim());
                });

                TableData.Add(singleLineData);
            });
        }

        private void WriteDataToFile(string fileName)
        {
            try
            {
                using TextWriter writer = new StreamWriter(File.Open(OutputDirectory + "\\" + fileName, 
                    FileMode.OpenOrCreate));
                
                using CsvWriter csvWriter = new CsvWriter(writer, CultureInfo.CurrentCulture);

                List<string> header = TableData[0].ToList();
                header.ForEach(h => csvWriter.WriteField(h));
                csvWriter.NextRecord();

                TableData.Skip(1).ToList()
                    .ForEach(r => {
                        csvWriter.WriteField(r);
                        csvWriter.NextRecord();
                    });
                
                Console.WriteLine("\nCreated file: " + fileName);
                Console.WriteLine("------------------");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("WriteDataToFile() => " + ex.Message);
            }
        }

        private string CreateFileAndOpen(string currency)
        {
            return new string(currency.Trim() + "_" + StartDate + "_to_" + EndDate + ".csv");
        }
    }
}
