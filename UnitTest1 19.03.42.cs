using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace RestSharp_Unit_Test;

public class Tests
{
    private RestClient client;
    [SetUp]
    public void Setup()
    {
        var options = new RestClientOptions("https://api.github.com")
        {
            Authenticator = new HttpBasicAuthenticator
            ("")
        };
        this.client = new RestClient(options);
    }

    [Test]
    public void Test_GitHubAPI_EndPoint()
    {
        //Arrenge
        var client = new RestClient("https://api.github.com");
        var request = new RestRequest
        ("/repos/API-Testing/issues", Method.Get);

        //act
        var response = client.Get(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }


    [Test]
    public void Test_GET_GitHub_APIRequest()
    {
        //Arrange
        var request = new RestRequest
        ("/repos/API-Testing/issues", Method.Get);
        request. Timeout = 1000;

        //Act
        var response = client.Get(request);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public void Test_Get_ALL_GitHub_APIRequest()
    {
        //Arrange
        var request = new RestRequest
        ("/repos/API-Testing/issues", Method.Get);

        //Act
        var response = client.Get(request);
        var issues = JsonSerializer.Deserialize<List<Issue>>(response.Content);

        //Asset
       Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

    }

    [Test]
    public void Test_CreateGitHubIssue()
    {
        string title = "This is a Demo Issue";
        string body = "QA Back-End Automation Course February 2024";

        // Act
        var issue = CreateIssue(title, body);

        // Assert
        Assert.That(issue, Is.Not.Null);
        Assert.That(issue.id, Is.Not.EqualTo(default(int)));
        Assert.That(issue.number, Is.Not.EqualTo(default(int)));
        Assert.That(issue.title, Is.Not.Empty);
    }

    private Issue CreateIssue(string title, string body)
    {
        var request = new RestRequest("/repos/API-Testing/issues");
        request.AddJsonBody(new { body, title }); 

        var response = client.Execute(request, Method.Post);

        
        if (response.IsSuccessful)
        {
            
            var issue = JsonSerializer.Deserialize<Issue>(response.Content);
            return issue;
        }
        else
        {
            
            throw new Exception($"GitHub API request failed with status code {response.StatusCode}. Content: {response.Content}");
        }
    }



    [Test]
    public void Test_EditIssue()
    {
        var request = new RestRequest("/repos/......./API-Testing/issues/28");

        request.AddJsonBody(new
        {
            title = "Changing the name of the issue that I created"
        });

        var response = client.Execute(request, Method.Patch);
        var issue = JsonSerializer.Deserialize<Issue>(response.Content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(issue.id, Is.GreaterThan(0), "Issue ID should be greater than 0.");
        Assert.That(response.Content, Is.Not.Empty, "The response content should not be empty.");
        Assert.That(issue.number, Is.GreaterThan(0), "Issue number should be greater than 0.");
        Assert.That(issue.title, Is.EqualTo("Changing the name of the issue that I created"));
    }


}

