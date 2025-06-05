using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using CW_10_s30320;                      
using CW_10_s30320.DTOs;                 
using CW_10_s30320.Models;               
using CW_10_s30320.Data;                
namespace CW_10_s30320.Tests
{
   public class ClientsApiTests : IClassFixture<WebApplicationFactory<Program>>
   {
       private readonly HttpClient _client;
       public ClientsApiTests(WebApplicationFactory<Program> factory)
       {
           _client = factory.CreateClient();
       }
       [Fact]
       public async Task GetAllClients_ShouldReturn200AndListOfClients()
       {

           var response = await _client.GetAsync("/api/clients");
           response.StatusCode.Should().Be(HttpStatusCode.OK);
           var clients = await response.Content.ReadFromJsonAsync<ClientDto[]>();
           clients.Should().NotBeNull();

       }
       [Fact]
       public async Task CreateClient_WithInvalidModel_ShouldReturnBadRequest()
       {
          
           var badDto = new ClientDto
           {

               FirstName = "",  
               LastName = "",
               Email = "not-an-email"
           };
           var response = await _client.PostAsJsonAsync("/api/clients", badDto);
           response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
           var body = await response.Content.ReadAsStringAsync();
           body.ToLower().Should().Contain("badrequest"); 
       }
       [Fact]
       public async Task CreateClient_WithValidModel_ShouldReturnCreated()
       {
           var newDto = new ClientDto
           {
               FirstName = "Jan",
               LastName = "Kowalski",
               Email = "jan.kowalski@example.com"
           };
           var response = await _client.PostAsJsonAsync("/api/clients", newDto);
           response.StatusCode.Should().Be(HttpStatusCode.Created);
           var created = await response.Content.ReadFromJsonAsync<ClientDto>();
           created.Should().NotBeNull();
           created.Id.Should().BeGreaterThan(0);
           created.FirstName.Should().Be("Jan");
       }
   }
}