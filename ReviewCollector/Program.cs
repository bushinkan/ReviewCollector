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

            Collector collector = new Collector();
            collector.BuildQueryString(ref uri, "14054", "SYzHKXnFhTjfqpP", SortParameters.newest);
            string data = collector.Get(uri.AbsoluteUri);
        }
    }
}
