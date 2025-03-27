using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GitLabWebhook.CodeReviewServices
{
    public class ConfluenceService
    {
        private readonly string _confluenceToken;
        private readonly string _confluenceURL;
        private readonly string _releasePageID;
        private readonly HttpClient _httpClient;

        public ConfluenceService(IConfiguration configuration)
        {
            _confluenceToken = Environment.GetEnvironmentVariable("CONFLUENCETOKEN");
            _confluenceURL = configuration["Confluence:ApiBaseUrl"];
            _releasePageID = configuration["Confluence:ReleaseBranchPageID"];
            _httpClient = new HttpClient();
            var byteArray = System.Text.Encoding.ASCII.GetBytes($":{_confluenceToken}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _confluenceToken);
        }

        public async Task<String> GetTargetBranch(String targetRelease)
        {
            var url = $"{_confluenceURL}/rest/api/content/{_releasePageID}?expand=body.storage";

            HttpResponseMessage response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Not able to reach Confluence");
            }

            string content
                = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(content);
            string htmlContent = jsonResponse.body.storage.value;         

            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            var table = doc.DocumentNode.SelectSingleNode("//table[@class='relative-table wrapped']");

            var tableRows = table.SelectNodes(".//tr");

            // Find the header row (assuming the first row is the header)
            var headerRow = tableRows[1];  // Index 1 is the second row, which contains column names
            var headerCells = headerRow.SelectNodes("td");

            int releaseColumnIndex = -1;
            int branchColumnIndex = -1;

            // Find the indices of the "Release" and "Branch" columns based on the header
            for (int i = 0; i < headerCells.Count; i++)
            {
                var headerText = headerCells[i].InnerText.Trim();
                if (headerText.Equals("Release", StringComparison.OrdinalIgnoreCase))
                {
                    releaseColumnIndex = i;
                }
                if (headerText.Equals("Branch", StringComparison.OrdinalIgnoreCase))
                {
                    branchColumnIndex = i;
                }
            }

            if (releaseColumnIndex == -1 || branchColumnIndex == -1)
            {
                throw new Exception("Could not find 'Release' or 'Branch' columns.");
            }

            // Loop through the table rows to find the matching release and its corresponding branch
            foreach (var row in tableRows)
            {
                var cells = row.SelectNodes("td");
                if (cells != null && cells.Count > 0)
                {
                    string release = cells[releaseColumnIndex].InnerText.Trim();

                    // Compare the release value with the targetRelease
                    if (release.Equals(targetRelease, StringComparison.OrdinalIgnoreCase))
                    {
                        string branch = cells[branchColumnIndex].InnerText.Trim();
                        Console.WriteLine($"Release: {targetRelease} - Branch: {branch}");
                        return branch;
                    }
                }
            }


            throw new Exception($"Target Release Not Found! {targetRelease}");
        }
        

    }
}
