using System;

namespace CW_10_s30320.Models
{
    public class Client_Trip
    {
        public int IdClient { get; set; }
        public int IdTrip { get; set; }

        public DateTime RegistrationDate { get; set; }
        public DateTime? PaymentDate { get; set; }

     
        public virtual Client IdClientNavigation { get; set; } = null!;
        public virtual Trip IdTripNavigation { get; set; } = null!;
    }
}
