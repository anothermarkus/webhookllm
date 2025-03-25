using CodeReviewServices;
using Microsoft.AspNetCore.HttpLogging;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace GitLabWebhook.CodeReviewServices
{
    public class JiraService
    {
        private readonly string _jiraBearerToken;
        private readonly string _jiraBaseURL;
        private readonly HttpClient _httpClient;
        private readonly string _jiraFieldLabel;

        public JiraService(IConfiguration configuration)
        {
            _jiraBearerToken = Environment.GetEnvironmentVariable("JIRATOKEN");
            _jiraBaseURL = configuration["JIRA:ApiBaseUrl"];
            _jiraFieldLabel = configuration["JIRA:ReleaseTargetLabel"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jiraBearerToken);

        }

        public async Task<string> GetReleaseTarget(string JIRATicketID)
        {
            string url = $"{_jiraBaseURL}/rest/api/latest/issue/{JIRATicketID}?fields={_jiraFieldLabel}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            return json["fields"]?[_jiraFieldLabel]?["value"]?.ToString();
        }
    }
}
