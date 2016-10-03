using System;
using System.Collections.Generic;
using System.Text;

namespace ReviewCollector
{
    public class Review
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime DisplayDate { get; set; }
        public int? OrderId { get; set; }
        public string Comments { get; set; }
        public string Url { get; set; }
        public double? Overall { get; set; }
       
    }
}
