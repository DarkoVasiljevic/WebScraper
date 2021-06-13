using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebScraper
{
    public class Scraper
    {
        public string HttpGetString { get; private set; }

        public string HttpPostString { get; private set; }

        public string Url { get; }
        
        public Scraper(string url)
        {
            Url = url;
            
            HttpGet();
        }

        public void HttpGet()
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(Url));
                request.Method = "GET";
                request.ContentType = "text/html;charset=GBK";

                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException("Status code => " + response.StatusCode);

                using StreamReader streamReader = new StreamReader(
                    response.GetResponseStream(), Encoding.UTF8);

                HttpGetString = streamReader.ReadToEnd().Trim();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("HttpGet() => " + ex.Message);
            }
        }

        public void HttpPost(string postData)
        {
            try
            {
                var data = Encoding.ASCII.GetBytes(postData);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                using Stream stream = request.GetRequestStream();
                stream.Write(data, 0, data.Length);

                using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new ApplicationException("Status code => " + response.StatusCode);

                using StreamReader responseStream = new StreamReader(response.GetResponseStream());

                HttpPostString = responseStream.ReadToEnd().Trim();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("HttpPost() => " + ex.Message);
            }
        }
    }
}
