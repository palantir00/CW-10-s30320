using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CW_10_s30320.DTOs;
namespace CW_10_s30320.Tests
{
   public class ClientsApiTests : IClassFixture<WebApplicationFactory<Program>>
   {
       private readonly HttpClient _client;
       public ClientsApiTests(WebApplicationFactory<Program> factory)
       {
           _client = factory.CreateClient();
       }
       [Fact(DisplayName = "Usuń klienta, który nie jest przypisany do żadnej wycieczki → 204 No Content")]
       public async Task Delete_ClientWithoutTrips_ReturnsNoContent()
       {
           var newClient = new ClientDto
           {
               Pesel = "55010212345",
               FirstName = "Jan",
               LastName = "Kowalski",
               Email = "jan.kowalski@example.com",
               Telephone = "123-456-789",
               TripIds = new System.Collections.Generic.List<int>()
           };
           var newClientJson = JsonSerializer.Serialize(newClient);
           var postResponse = await _client.PostAsync(
               "/api/clients",
               new StringContent(newClientJson, Encoding.UTF8, "application/json")
           );
           postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
           var createdJson = await postResponse.Content.ReadAsStringAsync();
           var createdClient = JsonSerializer.Deserialize<ClientDto>(createdJson,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
           createdClient.Should().NotBeNull();
           int clientId = createdClient!.IdClient;
           var deleteResponse = await _client.DeleteAsync($"/api/clients/{clientId}");
           deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
       }
       [Fact(DisplayName = "Usuń klienta, który jest przypisany do wycieczki → 400 BadRequest")]
       public async Task Delete_ClientWithTrip_ReturnsBadRequest()
       {
           var newTrip = new CreateTripDto
           {
               Name = "TestTrip",
               Description = "Wycieczka testowa",
               DateFrom = System.DateTime.UtcNow.AddDays(10),
               DateTo = System.DateTime.UtcNow.AddDays(15),
               MaxPeople = 5
           };
           var newTripJson = JsonSerializer.Serialize(newTrip);
           var tripPostResponse = await _client.PostAsync(
               "/api/trips",
               new StringContent(newTripJson, Encoding.UTF8, "application/json")
           );
           tripPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);
           var tripJson = await tripPostResponse.Content.ReadAsStringAsync();
           var createdTrip = JsonSerializer.Deserialize<TripDto>(tripJson,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
           createdTrip.Should().NotBeNull();
           int tripId = createdTrip!.IdTrip;
           var newClient = new ClientDto
           {
               Pesel = "66010254321",
               FirstName = "Anna",
               LastName = "Nowak",
               Email = "anna.nowak@example.com",
               Telephone = "987-654-321",
               TripIds = new System.Collections.Generic.List<int>()
           };
           var newClientJson = JsonSerializer.Serialize(newClient);
           var clientPostResponse = await _client.PostAsync(
               "/api/clients",
               new StringContent(newClientJson, Encoding.UTF8, "application/json")
           );
           clientPostResponse.StatusCode.Should().Be(HttpStatusCode.Created);
           var clientJson = await clientPostResponse.Content.ReadAsStringAsync();
           var createdClient = JsonSerializer.Deserialize<ClientDto>(clientJson,
               new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
           createdClient.Should().NotBeNull();
           int clientId = createdClient!.IdClient;
           var assignDto = new CreateClientInTripDto
           {
               Pesel = newClient.Pesel,
               FirstName = newClient.FirstName,
               LastName = newClient.LastName,
               Email = newClient.Email,
               Telephone = newClient.Telephone
           };
           var assignJson = JsonSerializer.Serialize(assignDto);
           var assignResponse = await _client.PostAsync(
               $"/api/trips/{tripId}/clients",
               new StringContent(assignJson, Encoding.UTF8, "application/json")
           );
           assignResponse.StatusCode.Should().Be(HttpStatusCode.Created);
           var deleteResponse = await _client.DeleteAsync($"/api/clients/{clientId}");
           deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
           var errorBody = await deleteResponse.Content.ReadAsStringAsync();
           errorBody.Should().ContainEquivalentOf("nie można usunąć, ponieważ klient ma przypisaną wycieczkę");
       }
   }
}