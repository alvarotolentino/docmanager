using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;
using Xunit;

namespace DocManagerTest.IntegrationTest
{
    public class DocManagerTest : BaseTest
    {
        [Fact]
        public async Task Login_As_Admin_User_Successfully()
        {
            var contentString = new StringContent("{\"Email\": \"adminuser@gmail.com\",\"Password\": \"P@ssw0rd\"}", Encoding.UTF8, "application/json");
            AddHeaders(contentString);
            var response = await Client.PostAsync("/api/login", contentString).Result.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(response) as IDictionary<string, object>;
            Assert.NotNull(result);
            Assert.NotNull(result["Succeeded"]);
            Assert.True((bool)result["Succeeded"]);
        }

        [Fact]
        public async Task Create_New_Account_User_Successfully()
        {
            var loginContentString = new StringContent("{\"Email\": \"adminuser@gmail.com\",\"Password\": \"P@ssw0rd\"}", Encoding.UTF8, "application/json");
            AddHeaders(loginContentString);
            var loginResponse = await Client.PostAsync("/api/login", loginContentString).Result.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<dynamic>(loginResponse) as IDictionary<string, object>;
            Assert.NotNull(loginResult);
            Assert.True((bool)loginResult["Succeeded"]);


            var user = new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Email = Faker.Internet.Email(),
                UserName = Faker.Internet.UserName(),
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
            };
            var newAccountUserContent = new StringContent(JsonSerializer.ToJsonString(user), Encoding.UTF8, "application/json");
            AddHeaders(newAccountUserContent);
            AddAuthorizationToken(loginResult);

            var newAccountResponse = await Client.PostAsync("/api/v1/accounts", newAccountUserContent).Result.Content.ReadAsStringAsync();
            var newAccountResult = JsonSerializer.Deserialize<dynamic>(newAccountResponse) as IDictionary<string, object>;

            Assert.NotNull(newAccountResult);
            Assert.True((bool)newAccountResult["Succeeded"]);

        }

        [Fact]
        public async Task Create_New_Account_User_Unauthorized()
        {
            var loginContentString = new StringContent("{\"Email\": \"manageruser@gmail.com\",\"Password\": \"P@ssw0rd\"}", Encoding.UTF8, "application/json");
            AddHeaders(loginContentString);
            var loginResponse = await Client.PostAsync("/api/login", loginContentString).Result.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<dynamic>(loginResponse) as IDictionary<string, object>;
            Assert.NotNull(loginResult);
            Assert.True((bool)loginResult["Succeeded"]);

            var user = new
            {
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                Email = Faker.Internet.Email(),
                UserName = Faker.Internet.UserName(),
                Password = "P@ssw0rd",
                ConfirmPassword = "P@ssw0rd",
            };
            var newAccountUserContent = new StringContent(JsonSerializer.ToJsonString(user), Encoding.UTF8, "application/json");
            AddHeaders(newAccountUserContent);
            AddAuthorizationToken(loginResult);
            var unauthorizeResponse = await Client.PostAsync("/api/v1/accounts", newAccountUserContent);

            Assert.Equal(unauthorizeResponse.StatusCode, HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Upload_Document_Successfully()
        {
            var loginContentString = new StringContent("{\"Email\": \"adminuser@gmail.com\",\"Password\": \"P@ssw0rd\"}", Encoding.UTF8, "application/json");
            AddHeaders(loginContentString);

            var loginResponse = await Client.PostAsync("/api/login", loginContentString).Result.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<dynamic>(loginResponse) as IDictionary<string, object>;
            Assert.NotNull(loginResult);
            Assert.True((bool)loginResult["Succeeded"]);

            AddAuthorizationToken(loginResult);
            MultipartFormDataContent form = GetMultipartForm();

            var newAccountResponse = await Client.PostAsync("/api/v1/Documents/upload", form).Result.Content.ReadAsStringAsync();
            var newAccountResult = JsonSerializer.Deserialize<dynamic>(newAccountResponse) as IDictionary<string, object>;

            Assert.NotNull(newAccountResult);
            Assert.True((bool)newAccountResult["Succeeded"]);
            Assert.True((double)(newAccountResult["Data"] as IDictionary<string, object>)["Id"] > 0);

        }

        [Fact]
        public async Task Upload_Document_Unauthorize()
        {
            var loginContentString = new StringContent("{\"Email\": \"basicuser@gmail.com\",\"Password\": \"P@ssw0rd\"}", Encoding.UTF8, "application/json");
            AddHeaders(loginContentString);

            var loginResponse = await Client.PostAsync("/api/login", loginContentString).Result.Content.ReadAsStringAsync();
            var loginResult = JsonSerializer.Deserialize<dynamic>(loginResponse) as IDictionary<string, object>;
            Assert.NotNull(loginResult);
            Assert.True((bool)loginResult["Succeeded"]);

            AddAuthorizationToken(loginResult);
            MultipartFormDataContent form = GetMultipartForm();

            var unauthorizeResponse = await Client.PostAsync("/api/v1/Documents/upload", form);

            Assert.Equal(unauthorizeResponse.StatusCode, HttpStatusCode.Unauthorized);

        }

        private void AddAuthorizationToken(IDictionary<string, object>? loginResult)
        {
            var token = (loginResult["Data"] as IDictionary<string, object>)["JWToken"].ToString();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static MultipartFormDataContent GetMultipartForm()
        {
            MultipartFormDataContent form = new MultipartFormDataContent();
            HttpContent description = new StringContent("test");
            HttpContent category = new StringContent("test");
            form.Add(description, "Description");
            form.Add(category, "Category");

            var assembly = Assembly.GetExecutingAssembly();

            var stream = new FileStream("Resources/file.txt", FileMode.Open);
            var content = new StreamContent(stream);
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "File",
                FileName = "file.txt",
            };
            content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
            form.Add(content);
            return form;
        }

        private static void AddHeaders(StringContent contentString)
        {
            contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            contentString.Headers.Add("X-Forwarded-For", new[] { "0.0.0.0" });
        }
    }
}