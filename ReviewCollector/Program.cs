using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace ReviewCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri uri = new Uri("https://shopperapproved.com/");

            string url = uri.AbsoluteUri + "api/xml/reviews/?siteid=14054&token=SYzHKXnFhTjfqpP&from=2016-01-01&to=2016-10-03&sort=newest&page=0";

            Collector collector = new Collector();
            string data = collector.Get(url);

            List<Review> reviews = collector.ParseData(data);
            Console.WriteLine("Affected " + collector.WriteData(reviews) + " rows");

            Console.ReadLine();
        }
    }
}
