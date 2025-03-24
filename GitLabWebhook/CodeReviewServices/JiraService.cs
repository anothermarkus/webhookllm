using CodeReviewServices;
using Microsoft.AspNetCore.HttpLogging;
using System.Net.Http.Headers;
using System.Text;

namespace GitLabWebhook.CodeReviewServices
{
    public class JiraService
    {
        private readonly string _jiraBearerToken;
        private readonly string _jiraBaseURL;
        private readonly HttpClient _httpClient;

        public JiraService(IConfiguration configuration)
        {
            _jiraBearerToken = Environment.GetEnvironmentVariable("JIRATOKEN");
            _jiraBaseURL = configuration["JIRA:ApiBaseUrl"];
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jiraBearerToken);

        }

        public async Task<string> GetReleaseTarget(string JIRATicketID)
        {
            // GET
            //https://jira.dell.com/rest/api/latest/issue/{JIRATicketID}?fields=customfield_10220

            /*
            {
                "expand": "renderedFields,names,schema,operations,editmeta,changelog,versionedRepresentations",
                "id": "3968870",
                "self": "https://jira.dell.com/rest/api/latest/issue/3968870",
                "key": "QJ-7344",
                "fields": {
                    "customfield_10220": {
                        "self": "https://jira.dell.com/rest/api/2/customFieldOption/59312",
                        "value": "FY26FW11-0403",
                        "id": "59312",
                        "disabled": false
                    }
                }
            }
            */

            //fields.customfield_10220.value
            return null; // TODO
        }

    }
}
