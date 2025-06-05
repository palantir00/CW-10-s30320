using System;
using System.Collections.Generic;

namespace CW_10_s30320.DTOs
{
    public class TripDto
    {
        public int IdTrip { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public int MaxPeople { get; set; }

        public List<CountryDto> Countries { get; set; } = new();
        public List<ClientDto> Clients { get; set; } = new();
    }
}
