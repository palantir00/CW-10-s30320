namespace CW_10_s30320.DTOs
{
    public class ClientDto
    {
        public int IdClient { get; set; }
        public string Pesel { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Telephone { get; set; } = null!;
    }
}
