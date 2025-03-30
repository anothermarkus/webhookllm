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
    /// <summary>
    /// Class responsible for providing JIRA API functionality.
    /// </summary>
    public class JiraService
    {
        private readonly string _jiraBearerToken;
        private readonly string _jiraBaseURL;
        private readonly HttpClient _httpClient;
        private readonly string _jiraFieldLabel;

        /// <summary>
        /// Constructor for the JiraService class.
        /// </summary>
        /// <param name="configuration">An instance of IConfiguration used to retrieve JIRA API token and base URL.</param>
        public JiraService(IConfiguration configuration)
        {
            _jiraBearerToken = Environment.GetEnvironmentVariable("JIRATOKEN") ?? throw new ArgumentNullException("JIRATOKEN needs to be set in the environment.");
            _jiraBaseURL = configuration["JIRA:ApiBaseUrl"] ?? throw new ArgumentException("JIRA:ApiBaseUrl needs to be set in the configuration.");
            _jiraFieldLabel = configuration["JIRA:ReleaseTargetLabel"] ?? throw new ArgumentException("JIRA:ReleaseTargetLabel needs to be set in the configuration.");
            _httpClient = new HttpClient(); // TODO move this out to DI
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jiraBearerToken);

        }

        /// <summary>
        /// Retrieves the release target from a specific JIRA ticket.
        /// </summary>
        /// <param name="JIRATicketID">The ID of the JIRA ticket.</param>
        /// <returns>The release target as a string.</returns>
        public async Task<string> GetReleaseTarget(string JIRATicketID)
        {
            string url = $"{_jiraBaseURL}/rest/api/latest/issue/{JIRATicketID}?fields={_jiraFieldLabel}";
            var response = await _httpClient.GetStringAsync(url);
            var json = JObject.Parse(response);

            return json["fields"]?[_jiraFieldLabel]?["value"]?.ToString() ?? string.Empty;
        }
    }
}
