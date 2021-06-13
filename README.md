# WebScraper
Console app demonstrates how a web scraper works.

## Dependences
```bash
Before using the app you need to install these packages from NuGet Package Manager:
  >> ConfigurationManager :: provides access to configuration files,
  >> HtmlAgilityPack      :: HTML parser written in C# to read/write DOM,
  >> Newtonsoft.Json      :: popular JSON framework for .NET,
  >> CsvHelper            :: package that’s used to read and write CSV files.
```

## Info
```bash
Application is automatically collecting data from a specific URL.
In addition, you can specify the output directory at the App.config file.
As a final result app:
  >> loads all available data,
  >> formats and writes them to a .csv files.
```
