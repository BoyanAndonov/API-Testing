using foody_API_Test.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace foody_API_Test
{
    public class FoodApiTests
    {
        private static RestClient client;
        private static string createdFoodId;
        private string? lastCreatedFoodid;
        private const string BASE_URL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:86";
        private const string USERNAME = "sabre2";
        private const string PASSWORD = "123456";

       

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(USERNAME, PASSWORD);

            var options = new RestClientOptions(BASE_URL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);

        }

        private string GetJwtToken(string username, string password)
        {
            var authClient = new RestClient(BASE_URL);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = authClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Access Token is null or empty");
                }

                return token;
            }
            else
            {
                throw new InvalidOperationException($"Authentication failed: {response.StatusCode} with data {response.Content}");
            }
        }

        [Test, Order(1)]
        public void CreateNewFood_ReturnsCreatedStatusCodeAndFoodId()
        {
            // Arrange
            var foodToAdd = new
            {
                Name = "Test Food",
                Description = "This is a test food."
            };

            var request = new RestRequest("/api/Food/Create", Method.Post)
                .AddJsonBody(foodToAdd);

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.IsTrue(responseBody.TryGetProperty("foodId", out JsonElement foodId));
            createdFoodId = foodId.GetString();
        }

        [Test, Order(2)]
        public void GetAllFoods_ReturnsOkStatusCodeAndNonEmptyArray()
        {
            var request = new RestRequest("/api/Food/All", Method.Get);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.IsNotNull(responseItems);
            Assert.IsNotEmpty(responseItems);

            // Extract the ID of the last created idea, the list is ordered by creation date
            lastCreatedFoodid = responseItems.LastOrDefault()?.FoodId;
        }

        [Test, Order(3)]
        public void EditFoodTitle_ReturnsOkStatusCodeAndSuccessMessage()
        {
            // Arrange
            var newTitle = "Updated Food Title";
            var request = new RestRequest($"/api/Food/Edit/{createdFoodId}", Method.Patch)
                .AddJsonBody(new[]
                {
                    new { path = "/name", op = "replace", value = newTitle }
                });

            // Act
            var response = client.Execute(request);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var responseBody = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.IsTrue(responseBody.TryGetProperty("msg", out JsonElement msg));
            Assert.AreEqual("Successfully edited", msg.GetString());
        }

        [Test, Order(4)]
        public void DeleteFood_ReturnsBadRequestStatusCodeAndErrorMessage()
        {

        }

        [Test, Order(5)]
        public void CreateFoodWithoutRequiredFields_ReturnsBadRequestStatusCode()
        {
            // Arrange: Create food object without required fields
            var foodToAdd = new { };

            // Act: Send POST request with incomplete data
            var request = new RestRequest("/api/Food/Create", Method.Post)
                .AddJsonBody(foodToAdd);

            var response = client.Execute(request);

            // Assert: Response status code should be BadRequest (400)
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test, Order(6)]
        public void EditNonExistingIdea_ShouldFail()
        {
            var requestData = new FoodDTO
            {
                Name = "editedTestTitle",
                Description = "TestDescription with edits",
            };

            var request = new RestRequest("/api/Food/Edit");
            request.AddQueryParameter("foodid", "112233");
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);
        }


        [Test]
        public void DeleteNonExistingFood_ReturnsBadRequestStatusCodeAndErrorMessage()
        {

        }









    }
}
