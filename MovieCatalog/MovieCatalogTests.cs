using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using MovieCatalog.Models;

namespace MovieCatalog
{
    [TestFixture]
    public class Tests
    {
        private static string lastCreatedMovieId;

        private RestClient client;
        private const string BaseUrl = "http://144.91.123.158:5000";  
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI4NWZmNjRmZC1lNGU3LTQxYjktYmViZS1jMGI5YTk0NTgxZjEiLCJpYXQiOiIwNC8xOC8yMDI2IDA2OjA4OjA0IiwiVXNlcklkIjoiYzYyMzFmODEtNzc1OC00ODAzLTYyMjktMDhkZTc2OTcxYWI5IiwiRW1haWwiOiJ0b25pdGlhOTYxNkBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoidG9uaXRpYTk2MTYiLCJleHAiOjE3NzY1MTQwODQsImlzcyI6Ik1vdmllQ2F0YWxvZ19BcHBfU29mdFVuaSIsImF1ZCI6Ik1vdmllQ2F0YWxvZ19XZWJBUElfU29mdFVuaSJ9.aY_8jlFqEXtO0AxDZ5QhndzznOuGSWGDA0Y3rBNwAsg";  // Change to your data

        private const string LoginEmail = "tonitia9616@example.com";  
        private const string LoginPassword = "tonitia9616";  

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, LoginPassword);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }
        [Order(1)]
        [Test]
        public void CreateMovie_WithReguiredFields_ShouldReturnSuccess()
        {
            MovieDto movieData = new MovieDto
            {
                Title = "The movie of my life",
                Description = "This is a movie description"
            
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Response code should be 200 OK");

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content, options);

            Assert.That(readyResponse, Is.Not.Null, "Response data should not be null.");
            Assert.That(readyResponse.Movie, Is.Not.Null, "Movie object should be returned.");
            Assert.That(readyResponse.Movie.Id, Is.Not.Null.And.Not.Empty, "Movie ID should not be null or empty.");
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));

 
            lastCreatedMovieId = readyResponse.Movie.Id;

        }

        [Order(2)]
        [Test]
        public void EditMovieTitle_ShouldChangeTitle()
        {

            MovieDto editedMovie = new MovieDto
            {
                Title = "The title has been edited",
                Description = "This is a movie description"

            };


            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", lastCreatedMovieId);

            request.AddBody(editedMovie);

            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            List<MovieDto> readyResponse = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);

            Assert.That(readyResponse, Is.Not.Null);
            Assert.That(readyResponse, Is.Not.Empty);
            Assert.That(readyResponse.Count, Is.GreaterThanOrEqualTo(1));

        }

        [Order(4)]
        [Test]
        public void DeleteExistingMovie_ShouldSucceed()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", lastCreatedMovieId);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);


            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutReguiredFields_ShouldReturnBadRequest()
        {
            MovieDto movieData = new MovieDto
            {
                Title = "",
                Description = ""

            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movieData);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Response code should be 400 Bad Request");

        }

        [Order(6)]
        [Test]
        public void EditNonExistingMovie_ShouldReturnBadRequest()
        {

            MovieDto editedMovie = new MovieDto
            {
                Title = "The title has been edited",
                Description = "This is a movie description"

            };

            var nonExistingMovieId = "12345";

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);

            request.AddQueryParameter("movieId", nonExistingMovieId);

            request.AddBody(editedMovie);

            RestResponse response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));

        }

        [Order(7)]
        [Test]
        public void DeleteNonExistingMovie_ShouldReturnBadRequest()
        {
            var nonExistingMovieId = "12345";

            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);

            request.AddQueryParameter("movieId", nonExistingMovieId);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);


            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}


