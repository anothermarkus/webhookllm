# webhookllm
Git Webhook API integration with LLM

![image](https://github.com/user-attachments/assets/ecb6f5fa-8089-474e-abc8-1d56109f4abf)


Use webhook.site to test https://webhook.site/

Replace config.json with actual data and run
set your OS env variables so this doesn't blow.


![image](https://github.com/user-attachments/assets/455d8892-cad4-4bf5-93ae-be170a4d1aff)


Checks branches against JIRA/Confluence and the MR

![image](https://github.com/user-attachments/assets/1dce6770-f473-4824-ac8f-0115ec0a9456)

![image](https://github.com/user-attachments/assets/9b92c494-0966-400f-8247-2c721ec69a49)



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
- Move these TODOs to issues list
