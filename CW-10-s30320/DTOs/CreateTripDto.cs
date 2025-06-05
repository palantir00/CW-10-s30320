using System;
using System.ComponentModel.DataAnnotations;

namespace CW_10_s30320.DTOs
{
    public class CreateTripDto
    {
        [Required] public string Name { get; set; } = null!;
        [Required] public string Description { get; set; } = null!;
        [Required] public DateTime DateFrom { get; set; }
        [Required] public DateTime DateTo   { get; set; }
        [Required] public int MaxPeople     { get; set; }
    }
}
