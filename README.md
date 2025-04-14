# webhookllm
Git Webhook API integration with LLM

![image](https://github.com/user-attachments/assets/2dfea711-a684-480a-a102-78cea5b2dd95)

Source: https://fintech.theodo.com/blog-posts/eslint-on-steroids-with-custom-rules


New Static Analysis Endpoint

![image](https://github.com/user-attachments/assets/5fb233ae-1a67-4f56-9ba1-f99b924d2971)




![image](https://github.com/user-attachments/assets/ecb6f5fa-8089-474e-abc8-1d56109f4abf)


Use webhook.site to test https://webhook.site/

Replace config.json with actual data and run
set your OS env variables so this doesn't blow.


![image](https://github.com/user-attachments/assets/455d8892-cad4-4bf5-93ae-be170a4d1aff)


Checks branches against JIRA/Confluence and the MR

![image](https://github.com/user-attachments/assets/1dce6770-f473-4824-ac8f-0115ec0a9456)

![image](https://github.com/user-attachments/assets/9b92c494-0966-400f-8247-2c721ec69a49)

Tokenizing a list of code smells, iterating a batch of files at a time per folder up to a token limit across a code smell one-by-one, so far it's good, but giving up after finding only one identified issue rather than all of them.

Failed at iterating code smell at a time, doing ZeroShot strategy with new model mistral, getting much better results.

There was too much uncertainty with regards to the models and the types of prompts and their ability to detect code issues.
I have created a new repo that summarizes the results of a few test-runs here.  The conclusion was very good results for llama-3-1-8b-instruct compared to other models

<img src="https://github.com/user-attachments/assets/db6851c9-c30e-4671-bd85-518413e99d13" width="500" />


https://github.com/anothermarkus/llmexperiment



Documents to reference for this project
- Review API Patterns https://github.com/openai/openai-dotnet/tree/OpenAI_2.1.0
- Documentation https://platform.openai.com/docs/api-reference/introduction
- Cookbook code quality https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions

