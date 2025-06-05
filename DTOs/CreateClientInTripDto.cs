using System;
using System.ComponentModel.DataAnnotations;

namespace CW_10_s30320.DTOs
{
    public class CreateClientInTripDto
    {
        [Required, MaxLength(11)]
        public string Pesel { get; set; } = null!;

        [Required, MaxLength(120)]
        public string FirstName { get; set; } = null!;

        [Required, MaxLength(120)]
        public string LastName { get; set; } = null!;

        [Required, EmailAddress, MaxLength(120)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(30)]
        public string Telephone { get; set; } = null!;

        [Required]
        public DateTime RegistrationDate { get; set; }

        public DateTime? PaymentDate { get; set; }
    }
}
