using System.Net.Http;
using System.Threading.Tasks;

namespace CodeReviewServices {

    public class GitLabService
    {
        private readonly string _gitlabToken;
        private readonly string _gitlabUrl;

        public GitLabService()
        {
            _gitlabToken = "TODO";
            _gitlabUrl = "TODO";
        }

        public async Task<string> GetMergeRequestFiles(string projectId, string mergeRequestURL)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Private-Token", _gitlabToken);

            var response = await client.GetStringAsync(mergeRequestURL);
            return response;
        }
    }

}