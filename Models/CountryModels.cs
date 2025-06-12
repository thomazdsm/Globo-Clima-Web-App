namespace GloboClima.WebApp.Models
{
    public class CountryResponseDto
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Capital { get; set; }
        public string? Region { get; set; }
        public string? Flag { get; set; }
        public double[]? Coordinates { get; set; }
    }
}
