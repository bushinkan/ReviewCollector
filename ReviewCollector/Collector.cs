using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Linq;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;

namespace ReviewCollector
{
    public class Collector
    {
        string strConnection = ConfigurationManager.ConnectionStrings["ReviewsConnection"].ConnectionString;
        string queryText = @"IF NOT EXISTS (SELECT * FROM Reviews WHERE Id = @Id) 
                             INSERT Reviews VALUES 
                             (@Id, @CustomerName, @DisplayDate, @OrderId, @Comments, @Url, @Overall)";
        public string Get(string url)
        {
            var request = BuildRequest(url);
            var response = request.GetResponse();
            string rawData = String.Empty;
            var encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                rawData = reader.ReadToEnd();
            }

            return rawData;
        }

        public HttpWebRequest BuildRequest(string url)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/xml";

            return request;
        }

        public List<Review> ParseData(string data)
        {
            var document = ConvertToXml(data);
            var xReviews = document.Element("reviews").Elements("review");
            var reviews = new List<Review>(xReviews.Count());

            foreach (var item in xReviews)
            {
                var review = new Review
                {
                    Id = Int32.Parse(item.Attribute("id").Value),
                    CustomerName = item.Element("name").Value,
                    OrderId = Int32.Parse(item.Element("orderid").Value),
                    Comments = item.Element("textcomments").Value,
                    Url = item.Element("fullurl").Value,
                    Overall = Double.Parse(item.Element("Overall").Value, CultureInfo.InvariantCulture),
                    DisplayDate = DateTime.Parse(item.Element("displaydate").Value)
                };

                reviews.Add(review);
            }
                
            return reviews;
        }

        private static XDocument ConvertToXml(string data)
        {
            if (String.IsNullOrEmpty(data))
                throw new Exception("Could not create an XML document, because the data argument is null or empty string.");

            return XDocument.Parse(data);
        }

        public int WriteData(List<Review> reviews)
        {
            int affected = 0;

            using (var connection = new SqlConnection(strConnection))
            {
                var sqlCommand = new SqlCommand
                {
                    Connection = connection,
                    CommandText = queryText
                };

                connection.Open();

                foreach (var review in reviews)
                {
                    AddInputParameters(sqlCommand, review);
                    affected += sqlCommand.ExecuteNonQuery();
                }
            }

            return affected;
        }

        private void AddInputParameters(SqlCommand command, Review parameters)
        {
            command.Parameters.Clear();

            foreach (var prop in parameters.GetType().GetProperties())
            {
                var val = prop.GetValue(parameters);

                var param = command.CreateParameter();
                param.ParameterName = prop.Name;
                param.Value = val ?? DBNull.Value;

                command.Parameters.Add(param);
            }
        }
    }
}
