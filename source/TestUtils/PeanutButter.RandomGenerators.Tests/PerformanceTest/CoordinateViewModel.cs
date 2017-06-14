using System.ComponentModel.DataAnnotations;

namespace PeanutButter.RandomGenerators.Tests.PerformanceTest
{
    public class CoordinateViewModel
    {
        public int SequenceNumber { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}")]
        public decimal Latitude { get; set; }

        [DisplayFormat(DataFormatString = "{0:N6}")]
        public decimal Longitude { get; set; }

        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal Distance { get; set; }
    }
}