using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using Idea_API_Test.Models;

namespace IdeaAPI_Test
{
    public class IdeaAPITest
    {
        private RestClient client;
        private const string BASEURL = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";
        private const string EMAIL = "bobi@yahoo.com";
        private const string PASSWORD = "123456";

        private static string lastCreatedIdeaId;
        


        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken(EMAIL, PASSWORD);

            var options = new RestClientOptions(BASEURL)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var authClient = new RestClient(BASEURL);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

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
        public void TestCreateNewIdea_WithRequiredInformation_ShouldSucceed()
        {
            var ideaRequest = new IdeaDTO
            {
                Title = "test-Idea",
                Description = "testIdea from Visual Studio"
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(ideaRequest);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseData.Msg, Is.EqualTo("Successfully created!"));
        }

        [Test, Order(2)]
        public void TestGetAllIdeas()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.IsNotNull(responseItems);
            Assert.IsNotEmpty(responseItems);

            // Extract the ID of the last created idea, the list is ordered by creation date
            lastCreatedIdeaId = responseItems.LastOrDefault()?.IdeaId;

        }


        [Test, Order(3)]
        public void TestEdit_LastIdea()
        {
            var editRequest = new IdeaDTO
            {
                Title = "Edited Idea",
                Url = "",
                Description = "Updated description."

            };

            var request = new RestRequest($"/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            request.AddJsonBody(editRequest);
            var response = this.client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Test, Order(4)]
        public void Test_Delete_LastIdea()
        {
            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);

            var response = client.Execute(request, Method.Delete);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));
        }

        [Test, Order(5)]
        public void CreateNewIdea_WithoutCorrectData_ShouldFail()
        {
            var requestData = new IdeaDTO
            {
                Title = "TestTitle"
            };

            var request = new RestRequest("/api/Idea/Create");
            request.AddJsonBody(requestData);

            var response = client.Execute(request, Method.Post);
            var responseData = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditNonExistingIdea_ShouldFail()
        {
            var requestData = new IdeaDTO
            {
                Title = "editedTestTitle",
                Description = "TestDescription with edits",
            };

            var request = new RestRequest("/api/Idea/Edit");
            request.AddQueryParameter("ideaId", "112233");
            request.AddJsonBody(requestData);
            var response = client.Execute(request, Method.Put);
        }

        [Test, Order(7)]
        public void DeleteNonExistingIdea_ShouldFail()
        {

            var request = new RestRequest("/api/Idea/Delete");
            request.AddQueryParameter("ideaId", "nonExistingId");

            var response = client.Execute(request, Method.Delete);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        }




    }
}
