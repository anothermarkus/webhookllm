# webhookllm
Git Webhook API integration with LLM

![image](https://github.com/user-attachments/assets/ecb6f5fa-8089-474e-abc8-1d56109f4abf)


Use webhook.site to test https://webhook.site/

Replace config.json with actual data and run
set your OS env variables so this doesn't blow.


![image](https://github.com/user-attachments/assets/5c8e8237-a9e2-40f6-b784-3dfe90b691e3)

Improved MR review, fed the entire filename into review, will consider summarizing at the MR level with Markup text for readability in GitLab.

LLM is not deterministic, so it will give a different response for the same code review, depending on the run!


![image](https://github.com/user-attachments/assets/d85d95cb-e65a-4f34-bc07-bb1c8b58941a)

Checks branches against JIRA/Confluence and the MR

![image](https://github.com/user-attachments/assets/1dce6770-f473-4824-ac8f-0115ec0a9456)

New route that Posts updates to the MR along with autodismiss functionality when issue is resolved and called again.
![image](https://github.com/user-attachments/assets/864c37cd-20f7-4ba9-8ff3-e3ce466f4d24)

Adding deny and allow list via configuration



Documents to reference for this project
    // Review API Patterns https://github.com/openai/openai-dotnet/tree/OpenAI_2.1.0
    // Documentation https://platform.openai.com/docs/api-reference/introduction
    // Cookbook code quality https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions


TODO
- Add Embeddings to OpenAI calls which should provide context / code standards
- Have the standards/criteria be flexible via dynamic configuration based on how the service is managing the MRs
- Introspection into how much compute power and/or time is spent per suggestion
- Parallelize suggestions across all files, or submit multiple files together, only review certain files: .ts, .cs  standards for each file
- Consideradding LangFuse to monitor performance
