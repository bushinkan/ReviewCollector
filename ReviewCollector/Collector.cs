using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ReviewCollector
{
    public class Collector
    {
        string connectionString = ConfigurationManager.ConnectionStrings["ReviewCollectorConnection"].ConnectionString;
        string queryText = @"IF NOT EXISTS (SELECT * FROM Reviews WHERE Id = @Id) 
                             INSERT Reviews VALUES 
                             (@Id, @CustomerName, @DisplayDate, @OrderId, @Comments, @Url, @Overall)";
        
        public string Get(string url)
        {
            var request = BuildRequest(url);
            var response = request.GetResponse();
            string rawData = String.Empty;
            var encoding = Encoding.UTF8;
            var xmlFormatter = new XmlSerializer(typeof(Review));

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                reader.ReadToEnd();
                var review = xmlFormatter.Deserialize(reader);
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

        public int WriteData(List<Review> reviews)
        {
            int affected = 0;

            using (var connection = new SqlConnection(connectionString))
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

        public Uri AttachGetParameters(Uri uri, Dictionary<string, string> parameters)
        {
            var builder = new StringBuilder("api/xml/reviews/");

            string symbol = "?";

            foreach (var pair in parameters)
            {
                builder.Append(symbol + pair.Key + "=" + pair.Value);
                symbol = "&";
            }

            return new Uri(uri + builder.ToString());
        }

        public void BuildQueryString(ref Uri uri, string siteId, string token, SortParameters sort)
        {
            var parameters = new Dictionary<string, string> 
            {
                { "siteid", siteId },
                { "token", token },
                { "from", this.GetStartDate() }, 
                { "to", DateTime.Now.ToString("yyyy-MM-dd") }, 
                { "sort", sort.ToString() },
                { "page", "0" }
            };

            uri = this.AttachGetParameters(uri, parameters);        
        }

        private string GetStartDate()
        {
            using (var db = new ReviewsDataContext(connectionString))
            {
                var startDate = db.Reviews.Max(d => d.DisplayDate).ToString("yyyy-MM-dd");

                return startDate;
            }
        }
    }

    public enum SortParameters
    {
        highest, lowest, oldest, newest
    }
}
