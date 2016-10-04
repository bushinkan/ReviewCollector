using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Linq.Mapping;

namespace ReviewCollector
{
    [Table(Name="Reviews")]
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
