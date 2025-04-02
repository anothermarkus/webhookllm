using Xunit;
using System.Threading.Tasks;
using GitLabWebhook.models;
using GitLabWebhook.CodeReviewServices;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Moq.Protected;
using System.Text;

namespace GitLabWebhook.Tests
{
    public class GitLabServiceTests
    {
        private GitLabService _service;
        private IConfiguration _configurationBuilder;

     

        public GitLabServiceTests()
        {
            _configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(new[] { new KeyValuePair<string, string?>("GitLab:ApiBaseUrl", "http://fakeValue") }).Build();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });
                

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);


            _service = new GitLabService(_configurationBuilder, httpClientFactory.Object);
        }

        [Fact]
        public async Task GetMergeRequestDetailsFromUrl_WhenUrlIsValid_ReturnsMRDetails()
        {

            
             var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

             var httpResponseMessage = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
             var httpResponseBody = "{\"source_branch\":\"test-source-branch\",\"target_branch\":\"test-target-branch\",\"diff_refs\":{\"base_sha\":\"test-base-sha\",\"head_sha\":\"test-head-sha\",\"start_sha\":\"test-start-sha\"},\"title\":\"test-mr-title\",\"changes\":[{\"new_path\":\"test-new-path\",\"old_path\":\"test-old-path\",\"diff\":\"test-diff\"},{\"new_path\":\"test-new-path-2\",\"old_path\":\"test-old-path-2\",\"diff\":\"test-diff-2\"}],\"sha\":\"test-mr-sha\"}";
             httpResponseMessage.Content = new StringContent(httpResponseBody, Encoding.UTF8, "application/json");
 

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);


            GitLabService service = new GitLabService(_configurationBuilder, httpClientFactory.Object);


            // Arrange
            var url = "https://example.com/merge_requests/1";
            var mrDetails = await service.GetMergeRequestDetailsFromUrl(url);

            // Assert
            Assert.NotNull(mrDetails);
        }

        [Fact]
        public async Task FindExistingDiscussion_WhenDiscussionExists_ReturnsDiscussion()
        {

             var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

             var httpResponseMessage = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
             var httpResponseBody = "[{\"source_branch\":\"test-source-branch\",\"target_branch\":\"test-target-branch\",\"diff_refs\":{\"base_sha\":\"test-base-sha\",\"head_sha\":\"test-head-sha\",\"start_sha\":\"test-start-sha\"},\"title\":\"test-mr-title\",\"notes\":[{\"id\":\"1\",\"body\":\"### Branch Sanity Check - FAIL\"}],\"sha\":\"test-mr-sha\"}]";
             httpResponseMessage.Content = new StringContent(httpResponseBody, Encoding.UTF8, "application/json");
             httpResponseMessage.Headers.Add("X-Total-Pages", "1");

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);


            GitLabService service = new GitLabService(_configurationBuilder, httpClientFactory.Object);


            // Arrange
            var mrId = "1";
            var repoPath = "path/to/repo";
            var discussion = await service.FindExistingDiscussion(mrId, repoPath, "### Branch Sanity Check - FAIL");

            // Assert
            Assert.NotNull(discussion);
        }

        [Fact]
        public async Task FindExistingNote_WhenNoteExists_ReturnsNote()
        {

             var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

             var httpResponseMessage = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
             var httpResponseBody = "[{\"body\":\"### Branch Sanity Check - PASS\"}]";
             httpResponseMessage.Content = new StringContent(httpResponseBody, Encoding.UTF8, "application/json");
             httpResponseMessage.Headers.Add("X-Total-Pages", "1");

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);


            GitLabService service = new GitLabService(_configurationBuilder, httpClientFactory.Object);


            // Arrange
            string mrId = "1";
            var repoPath = "path/to/repo";
            var note = await service.FindExistingNote(mrId, repoPath, "### Branch Sanity Check - PASS");

            // Assert
            Assert.NotNull(note);
        }

        [Fact]
        public async Task PostCommentToMR_WhenCommentIsPosted_ReturnsOk()
        {
            // Arrange
            var mrId = "1";
            var repoPath = "path/to/repo";
            var comment = "This is a comment";
            await _service.PostCommentToMR(comment, mrId, repoPath);

            // Assert
            // No need to check the return value as the method is void
        }

        [Fact]
        public async Task DismissReview_WhenReviewIsDismissed_ReturnsOk()
        {

             var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

             var httpResponseMessage = new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK };
             var httpResponseBody = "[{\"source_branch\":\"test-source-branch\",\"target_branch\":\"test-target-branch\",\"diff_refs\":{\"base_sha\":\"test-base-sha\",\"head_sha\":\"test-head-sha\",\"start_sha\":\"test-start-sha\"},\"title\":\"test-mr-title\",\"changes\":null,\"sha\":\"test-mr-sha\"}]";
             httpResponseMessage.Content = new StringContent(httpResponseBody, Encoding.UTF8, "application/json");
             httpResponseMessage.Headers.Add("X-Total-Pages", "1");

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);


            GitLabService service = new GitLabService(_configurationBuilder, httpClientFactory.Object);

            // Arrange
            var mrId = "1";
            var repoPath = "path/to/repo";
            var review = "This is a review";
            await service.DismissReview(review, mrId, repoPath);

            // Assert
            // No need to check the return value as the method is void
        }
    }
}