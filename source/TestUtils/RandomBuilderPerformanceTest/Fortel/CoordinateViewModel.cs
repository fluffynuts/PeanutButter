using System.ComponentModel.DataAnnotations;

namespace RandomBuilderPerformanceTest.Fortel
{
    public class CoordinateViewModel
    {
        public int SequenceNumber { get; set; }
        [DisplayFormat(DataFormatString = "{0:N6}") ]
        public decimal Latitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:N6}")]
        public decimal Longitude { get; set; }
        [DisplayFormat(DataFormatString = "{0:N1}")]
        public decimal Distance { get; set; }
    }
}