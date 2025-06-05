namespace CW_10_s30320.DTOs
{
   public class ClientDto
   {
       public int IdClient { get; set; }
       public string Pesel { get; set; } = default!;
       public string FirstName { get; set; } = default!;
       public string LastName { get; set; } = default!;
       public string Email { get; set; } = default!;
       public string Telephone { get; set; } = default!;
       // testy korzystają z tego pola:
       public List<int> TripIds { get; set; } = new List<int>();
   }
}