namespace GitLabWebhook.Models
{
    public static class OpenAPIPrompts
    {



       public static string FewShotCodeSmellPromptUserInput = @"Here are the deltas of all the files that have changed in my Merge Request. Please review them for any potential code smells:\n\n{code}";


       public static string ZeroShotCodeSmellPromptUserInput = @"Here are the deltas of all the files that have changed in my Merge Request. Please review them for any potential code smells:\n\n{code}";

       

        


        public static string GetPositiveFewShotCodeSmellSystemMessageAngular(string codeSmellDefinition)
        {
            return $"""
                You are a code reviewer. Analyze the following Angular code changes defined by a unified diff.

                🔍 Instructions:
                Your task is to detect a specific code smell: Selector Duplication **only**.

                👁️‍🗨️ Definition:
                Selector Duplication happens when the **exact same component or element** is repeated multiple times with different `*ngIf`s in the same heirachial level instead of being rendered once using `*ngFor`.

                {codeSmellDefinition}

                📌 If the code does not match the issue described, respond strictly with:
                "No code smell detected."
                """;
        }

          public static string GetPositiveFewShotCodeAssistantMessageAngular(string codeSmellDefinition)
        {
            return $"Based on the provided code, I have detected {codeSmellDefinition} The following changes should be made to address the smell: [recommended changes]";                
        }


        public static string GetPositiveFewShotCodeSmellSystemMessageDotNet(string codeSmellDefinition){
            return   @"You are a code reviewer. Analyze the following DotNet code changes defined by Diff for a specific anti-pattern.\n " +
            "Instructions: If it looks like an anti-pattern, please output the following:" +
            "Issue: [Name]   Explanation: [Short explanation]   Suggestion: [Improvement] " +
            "If it doesn't look like an anti-pattern, respond with: \"No code smell detected.\"" +
            $"Here is the anti-pattern you are reviewing against the code: {codeSmellDefinition}"; 
        }
        

        /// <summary>
        ///  gleaned from: https://cookbook.openai.com/examples/third_party/code_quality_and_security_scan_with_github_actions
        /// </summary>
        public static string EnterpriseStandardsPrompt =
            "You are an Enterprise Code Assistant. Review each code snippet below for its adherence to the following categories:\n" +
            "1) Code Style & Formatting\n" +
            "2) Security & Compliance\n" +
            "3) Error Handling & Logging\n" +
            "4) Readability & Maintainability\n" +
            "5) Performance & Scalability\n" +
            "6) Testing & Quality Assurance\n" +
            "7) Documentation & Version Control\n" +
            "8) Accessibility & Internationalization\n\n" +
            "Create a table and assign a rating of 'extraordinary', 'acceptable', or 'poor' for each category.\n" +
            "Return a markdown table titled 'Enterprise Standards' with rows for each category and columns for 'Category' and 'Rating'.\n\n" +
            "Here are the changed file contents to analyze:\n" +
            "{{CODE_SNIPPET_HERE}}";


        // public static string CodeReviewPromptDotNet =
        //     "You are an Enterprise Code Assistant ensuring the code follows best practices. " +

        //     "I am providing you with only a diff representing the changes for C# code, please distiguish which you are reviewing and understand it is only a git diff." +


        //     "Review each file change thoroughly for these code standards:\n\n" +

        //     // Code Style
        //     "• Ensure adherence to DRY (Don't Repeat Yourself) principle by avoiding code duplication and reusing code effectively.\n" +
        //     "• Maintain cyclomatic complexity under 10; break down complex methods into smaller, manageable ones.\n" +
        //     "• Avoid deep nesting; use early returns for cleaner logic flow.\n" +
        //     "• Use null-conditional operators (?.) and null-coalescing operators (??) for safe null value access.\n" +
        //     "• Implement guard clauses to handle null or invalid inputs early in methods.\n\n" +

        //     // Memory Management
        //     "• Always use 'using' statements for disposable objects to ensure automatic resource disposal.\n" +
        //     "• Minimize memory leaks by unsubscribing from events when no longer needed.\n" +
        //     "• Dispose of unmanaged resources properly and be mindful of large object retention in memory.\n" +
        //     "• Avoid unnecessary object creation; use weak references or caching where applicable.\n\n" +

        //     // Error Handling
        //     "• Use try-catch blocks to handle exceptions; catch specific exceptions, not generic ones.\n" +
        //     "• Always use 'finally' for cleanup operations to release resources.\n" +
        //     "• Avoid silent failures; log exceptions for troubleshooting and debugging.\n" +
        //     "• Throw custom exceptions only for business logic errors, not for regular control flow.\n" +
        //     "• Don't use exceptions for control flow; use conditional checks instead.\n\n" +

        //     // Thread Handling & Async/Await
        //     "• Use async/await for asynchronous programming; avoid manually managing threads.\n" +
        //     "• Use ConfigureAwait(false) to avoid deadlocks in non-UI thread operations.\n" +
        //     "• Avoid blocking async calls (e.g., don't use Result or Wait()).\n" +
        //     "• Ensure thread safety by using locks or thread-safe collections when accessing shared resources.\n" +
        //     "• Use CancellationToken for graceful cancellation of long-running async operations.\n" +
        //     "• Avoid using Thread.Sleep() in async code; prefer Task.Delay() for non-blocking waits.\n\n" +

        //     "Strictly summarize only issues that are found and create a summary table in markdown format";


        public static string CodeReviewPromptDotNet =
                                "You are an Enterprise Code Assistant ensuring the code follows best practices. I am providing you with a diff representing the changes for a merge request — this may include controllers, services, models, or configuration files. Please distinguish which type of file you are reviewing.\n" +
                                @"- Avoid code duplication (DRY)
                                    - Use null-conditional and null-coalescing operators when appropriate
                                    - Minimize deep nesting and improve readability
                                    - Proper exception handling: avoid empty catch blocks and use specific exceptions
                                    - Remove commented-out or obsolete code
                                    - Avoid hardcoded credentials, URLs (e.g., localhost), or magic strings
                                    - Use async/await for I/O-bound operations instead of blocking calls
                                    - Ensure proper dependency injection rather than using static or singleton patterns inappropriately
                                    - Follow SOLID principles and maintain separation of concerns
                                    - Validate inputs and use appropriate data annotations on models
                                    - Use meaningful logging (not just Console.WriteLine)
                                    - Favor `readonly` fields when possible and avoid unnecessary setters
                                    - Dispose of IDisposable resources correctly, especially in services or controllers
                                    - In ASP.NET Core, prefer minimal APIs or clean controller actions — avoid bloated actions\n" +
                                "Strictly summarize only issues that are found and create a summary table in markdown format";

        //     public static string CodeReviewPromptAngular =
        //         "You are an Enterprise Code Assistant ensuring the code follows best practices. I am providing you with a diff representing the changes for a merge request these include templates as well as components and services please distiguish which you are reviewing." +

        //         "Review each file change thoroughly for these code standards:\n\n" +
        //         "• **Repetitive Component Usage**: Avoid duplicating the same components multiple times in a template with only slight variations. If the same component is rendered with different conditions or input bindings (e.g., *ngIf), it's often a sign of duplication. Consider using *ngFor with a data structure to dynamically render components instead of repeating them manually.\n" +
        //         //"• **Improper Use of ngIf vs ngFor**: Don't use *ngIf when *ngFor should be used. *ngIf is designed to conditionally render a single element, while *ngFor should be used when rendering multiple items from an array or iterable. If you have multiple similar elements that could be dynamically created from an array, replace *ngIf with *ngFor.\n" +
        //         "• **Improper Use of ngIf**\n\n**Definition**: Multiple elements are rendered conditionally using *ngIf with similar structure. Consider replacing these with a single *ngFor loop over an array. Do not call methods like computeList() directly in templates.\n"+
        //         "• **Change Detection Issues**: Be mindful of Angular's change detection strategy. Avoid writing complex logic directly in templates that could lead to performance issues (e.g., expensive calculations or function calls). Instead, move these calculations into the component class and use memoization or `trackBy` functions to optimize performance.\n" +
        //         "• **Excessive Component Nesting**: Avoid deep component hierarchies. Too much nesting can lead to performance issues and hard-to-maintain code. Break down large components into smaller, reusable, and more manageable ones.\n" +
        //         "• **Template-Driven Forms Overuse**: Avoid using template-driven forms for complex form logic. For more complex forms, prefer reactive forms, as they provide better control, scalability, and testability. Template-driven forms are more suited for simpler, static forms.\n" +
        //         "• **Lack of Lazy Loading**: Failing to implement lazy loading can result in unnecessarily large initial payloads for the application. Use Angular's lazy loading feature to split the application into smaller chunks that are only loaded when needed, reducing the initial loading time.\n" +
        //         "• **Hardcoding Data in Templates**: Avoid hardcoding dynamic data directly in the template. For example, avoid binding UI elements to data that’s not coming from the component or service. Always aim to keep the logic and data in the component class for better maintainability and testability.\n" +
        //         "• **Inefficient Event Binding**: Be cautious about binding events within loops or repeated elements. This can result in unnecessary function calls and decreased performance. Instead, optimize by binding the event handler at the component level and using `$event` to access specific data.\n" +
        //         "• **Improper Use of Observables**: Avoid unnecessary subscriptions or subscriptions without proper cleanup. Ensure that Observables are unsubscribed properly, especially in components where the lifecycle is tied to DOM changes, to prevent memory leaks. Prefer using the `async` pipe in templates or use `takeUntil` in component code to handle subscriptions.\n" +
        //         "• **Memory Leaks Due to Subscriptions**: In Angular, it's crucial to unsubscribe from any subscriptions when a component is destroyed. If you're subscribing to the store or observables manually (e.g., via `store.select()` or `observable.subscribe()`), ensure you unsubscribe to avoid memory leaks. Use `takeUntil` with a `Subject` to handle cleanup on component destruction, or use the `async` pipe to manage subscriptions automatically in the template.\n" +
        //         "• **Using Services to Store State**: When using NgRx, avoid storing component state within services for cross-component state sharing. Instead, rely on the store to manage application state. Using the store keeps state management predictable, traceable, and scalable. Components should subscribe to the store directly to receive state updates, rather than holding state in services that need to be referenced later.\n" +
        //         "• **Mismanagement of Services and Singletons**: Avoid creating services inside components. Services should be injected via dependency injection and should be provided at the appropriate level (root or module) to avoid multiple instances and ensure proper singleton behavior.\n" +
        //         "• **Improper Error Handling**: Ensure proper error handling in your services and components, especially for HTTP requests. Use `.catchError()` or `try-catch` blocks to gracefully handle errors and provide fallback logic or user-friendly messages when needed.\n" +
        //         "• **Direct DOM Manipulation**: Avoid direct DOM manipulation using `document.getElementById()` or other native JavaScript DOM APIs. Instead, rely on Angular’s built-in mechanisms like directives or `Renderer2` to manipulate the DOM safely and efficiently, ensuring cross-platform compatibility.\n" +
        //         "• **Excessive use of Local Storage or Session Storage**: Be careful when using local storage or session storage for data persistence in Angular. Sensitive data should never be stored in the browser storage without encryption, and reliance on storage should be minimized to ensure proper application performance and security.\n" +
        //         "Strictly summarize only issues that are found and create a summary table in markdown format";


        public static string CodeReviewPromptAngular =
                                "You are an Enterprise Code Assistant ensuring the code follows best practices. " +
                                "I am providing you with a diff representing the changes for a merge request these include templates as well as components and services please distiguish which you are reviewing.\n" +
                                @"- Avoid code duplication (DRY)
                                    - Use null-conditional operators
                                    - Minimize deep nesting                       
                                    - Proper exception handling
                                    - Remove commented-out or testing code
                                    - Avoid hardcoded localhost or credentials
                                    - In Angular, unsubscribe from observables to prevent memory leaks
                                    - Follow proper state management practices using ngrx (avoid updating services directly) \n" +
                                "Strictly summarize only issues that are found and create a summary table in markdown format";

    }



}