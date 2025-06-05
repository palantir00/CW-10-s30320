using System.Collections.Generic;

namespace CW_10_s30320.Models
{
    public class Country
    {
        public int IdCountry { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Country_Trip> Country_Trips { get; set; } = new List<Country_Trip>();
    }
}
