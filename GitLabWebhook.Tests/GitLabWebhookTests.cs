using Xunit;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using GitLabWebhook.Controllers;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace GitLabWebhook.Tests
{
    public class GitLabWebhookTests
    {
        private readonly GitLabWebhookController _controller;

        // Constructor sets up the controller with mocked configuration
        public GitLabWebhookTests()
        {

             var configurationBuilder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path to the current directory (tests folder)
            .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"), optional: false, reloadOnChange: true);

              var configuration = configurationBuilder.Build();

            _controller = new GitLabWebhookController(configuration);
        }

        [Fact]
        public async Task TestGitLabWebhookHandler_ValidPayload()
        {
            // Sample GitLab Webhook JSON from https://webhook.site/
            var webhookPayloadJson = @"{""object_kind"":""merge_request"",""event_type"":""merge_request"",""user"":{""id"":1891,""name"":""Kopec, Mark"",""username"":""Mark_Kopec"",""avatar_url"":""https://gitlab.dell.com/uploads/-/system/user/avatar/1891/avatar.png"",""email"":""[REDACTED]""},""project"":{""id"":7518,""name"":""dsa-commerce"",""description"":""DSA.CommerceDomain"",""web_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""avatar_url"":null,""git_ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""git_http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git"",""namespace"":""Kopec, Mark"",""visibility_level"":10,""path_with_namespace"":""Mark_Kopec/DSA-Commerce"",""default_branch"":""master-fy20-0202"",""ci_config_path"":null,""homepage"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git""},""object_attributes"":{""assignee_id"":null,""author_id"":1891,""created_at"":""2025-03-17 00:21:07 UTC"",""description"":"""",""draft"":true,""head_pipeline_id"":null,""id"":3646501,""iid"":1,""last_edited_at"":null,""last_edited_by_id"":null,""merge_commit_sha"":null,""merge_error"":null,""merge_params"":{""force_remove_source_branch"":""0""},""merge_status"":""checking"",""merge_user_id"":null,""merge_when_pipeline_succeeds"":false,""milestone_id"":null,""source_branch"":""master-fy20-0202-testbranch"",""source_project_id"":7518,""state_id"":1,""target_branch"":""master-fy20-0202"",""target_project_id"":7518,""time_estimate"":0,""title"":""Draft: Update CartItemManager.cs"",""updated_at"":""2025-03-17 00:21:08 UTC"",""updated_by_id"":null,""prepared_at"":""2025-03-17 00:21:08 UTC"",""assignee_ids"":[],""blocking_discussions_resolved"":true,""detailed_merge_status"":""checking"",""first_contribution"":true,""human_time_change"":null,""human_time_estimate"":null,""human_total_time_spent"":null,""labels"":[],""last_commit"":{""id"":""66906f259b179fefde403d059658ca1ecc06711b"",""message"":""Update CartItemManager.cs"",""title"":""Update CartItemManager.cs"",""timestamp"":""2025-03-17T00:18:20+00:00"",""url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce/-/commit/66906f259b179fefde403d059658ca1ecc06711b"",""author"":{""name"":""Kopec, Mark"",""email"":""mark_kopec@dell.com""}},""reviewer_ids"":[],""source"":{""id"":7518,""name"":""dsa-commerce"",""description"":""DSA.CommerceDomain"",""web_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""avatar_url"":null,""git_ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""git_http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git"",""namespace"":""Kopec, Mark"",""visibility_level"":10,""path_with_namespace"":""Mark_Kopec/DSA-Commerce"",""default_branch"":""master-fy20-0202"",""ci_config_path"":null,""homepage"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git""},""state"":""opened"",""target"":{""id"":7518,""name"":""dsa-commerce"",""description"":""DSA.CommerceDomain"",""web_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""avatar_url"":null,""git_ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""git_http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git"",""namespace"":""Kopec, Mark"",""visibility_level"":10,""path_with_namespace"":""Mark_Kopec/DSA-Commerce"",""default_branch"":""master-fy20-0202"",""ci_config_path"":null,""homepage"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce"",""url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""ssh_url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""http_url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce.git""},""time_change"":0,""total_time_spent"":0,""url"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce/-/merge_requests/1"",""work_in_progress"":true,""approval_rules"":[{""id"":4510611,""created_at"":""2025-03-17 00:21:07 UTC"",""updated_at"":""2025-03-17 00:21:07 UTC"",""merge_request_id"":3646501,""approvals_required"":0,""name"":""All Members"",""rule_type"":""any_approver"",""report_type"":null,""section"":null,""modified_from_project_rule"":false,""orchestration_policy_idx"":null,""vulnerabilities_allowed"":0,""scanners"":[],""severity_levels"":[],""vulnerability_states"":[""new_needs_triage"",""new_dismissed""],""security_orchestration_policy_configuration_id"":null,""scan_result_policy_id"":null,""applicable_post_merge"":null,""project_id"":7518,""approval_policy_rule_id"":null}],""action"":""open""},""labels"":[],""changes"":{""merge_status"":{""previous"":""preparing"",""current"":""checking""},""updated_at"":{""previous"":""2025-03-17 00:21:07 UTC"",""current"":""2025-03-17 00:21:08 UTC""},""prepared_at"":{""previous"":null,""current"":""2025-03-17 00:21:08 UTC""}},""repository"":{""name"":""dsa-commerce"",""url"":""git@gitlab.dell.com:Mark_Kopec/DSA-Commerce.git"",""description"":""DSA.CommerceDomain"",""homepage"":""https://gitlab.dell.com/Mark_Kopec/DSA-Commerce""}}";

            // Parse the JSON payload into a JObject
            var webhookPayload = JObject.Parse(webhookPayloadJson);

            // Act: Call the POST method in the controller with the payload
            var result = await _controller.Post(webhookPayload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            var response = okResult.Value as GitLabWebhookResponse;
            Assert.NotNull(response);
            Assert.Equal("received", response.Status);  
            Assert.Equal("Merge Request Event Processed", response.Message);  
        }
    }
}
