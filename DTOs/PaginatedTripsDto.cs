using System.Collections.Generic;

namespace CW_10_s30320.DTOs
{
    public class PaginatedTripsDto
    {
        public int PageNum { get; set; }
        public int PageSize { get; set; }
        public int AllPages { get; set; }
        public List<TripDto> Trips { get; set; } = new();
    }
}
