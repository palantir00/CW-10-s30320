namespace CW_10_s30320.Models
{
    public class Country_Trip
    {
        public int IdCountry { get; set; }
        public int IdTrip { get; set; }

       
        public virtual Country IdCountryNavigation { get; set; } = null!;
        public virtual Trip IdTripNavigation { get; set; } = null!;
    }
}
