using System;
using System.Collections.Generic;

namespace CW_10_s30320.Models
{
    public class Trip
    {
        public int IdTrip { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }

        public virtual ICollection<Client_Trip> Client_Trips { get; set; } = new List<Client_Trip>();
        public virtual ICollection<Country_Trip> Country_Trips { get; set; } = new List<Country_Trip>();
    }
}
