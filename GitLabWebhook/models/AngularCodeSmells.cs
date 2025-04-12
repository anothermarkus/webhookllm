
namespace GitLabWebhook.Models
{
    public enum AngularCodeSmellType
    {
        SelectorDuplication,
        // TooMuchLogicInTemplate,
        // HardCodedStrings,
         InefficientNgForTrackBy,
        // LargeComponent,
        // ImproperServiceInjection,
        // ManipulatingDOMDirectly,
        // NotUsingAsyncPipe

    }

    public static class AngularCodeSmells
    {
        public static readonly Dictionary<AngularCodeSmellType, string> PrettyNames = new()
        {
            { AngularCodeSmellType.SelectorDuplication, "Selector Duplication" },
            // { AngularCodeSmellType.TooMuchLogicInTemplate, "Too Much Logic in Template" },
            // { AngularCodeSmellType.HardCodedStrings, "Hard-Coded Strings" },
             { AngularCodeSmellType.InefficientNgForTrackBy, "Inefficient *ngFor Without trackBy" },
            // { AngularCodeSmellType.LargeComponent, "Large Component" },
            // { AngularCodeSmellType.ImproperServiceInjection, "Improper Service Injection" },
            // { AngularCodeSmellType.ManipulatingDOMDirectly, "Direct DOM Manipulation" },
            // { AngularCodeSmellType.NotUsingAsyncPipe, "Not Using Async Pipe" }
        };

        public static readonly Dictionary<string, string> Definitions = new()
        {

            {
            "Selector Duplication",
            """
              @"This issue occurs when multiple instances of the same UI component (e.g., <any-component>) are conditionally rendered using different *ngIf expressions, 
            but the component structure remains largely the same. This often results in verbose, repetitive templates that are harder to maintain and can be simplified using *ngFor.

            📌 Why it's a problem:
            Even though the *ngIf conditions differ, the duplicated structure can often be abstracted into a list or array of config objects, which allows the template to be simplified using *ngFor.

            ⚠️ The following examples are not from the user's code. They are provided only to explain the concept:

            🧪 Example of bad code (for illustration only):
            ```html
            <any-component *ngIf=""conditionA"" [input]=""valueA""></any-component>
            <any-component *ngIf=""conditionB"" [input]=""valueB""></any-component>
            <any-component *ngIf=""conditionC"" [input]=""valueC""></any-component>
            ```

            ✅ Better approach:
            ```html
            <ng-container *ngFor=""let item of items"">
            <any-component 
                [inputA]=""item.inputA"" 
                [inputB]=""item.inputB""
                *ngIf=""item.show"">
            </any-component>
            </ng-container>
            ```

            """
            },

        // {
        //     "Selector Duplication",
        //     "Same selector is repeated multiple times in the same heirarchical level with different *ngIf conditions instead of being rendered dynamically using *ngFor with the exception of built in structural drictives such as <ng-container> and <ng-template>."
        // }
            

            // {
            // "Too Much Logic in Template",
            // @"Complex expressions or method calls in the HTML template can reduce readability and performance.
            
            // Example:
            // <div *ngIf=""items.filter(i => i.isActive).length > 0""></div>
            
            // Better:
            // <!-- Move logic to the component -->
            // <div *ngIf=""hasActiveItems""></div>
            
            // Component:
            // hasActiveItems = this.items.some(i => i.isActive);"
            // },
            // {
            // "Hard-Coded Strings",
            // @"Embedding user-facing text directly in templates makes localization difficult and violates separation of concerns.

            // Example:
            // <h1>Welcome to our store!</h1>

            // Better:
            // <h1>{{ 'HOME.TITLE' | translate }}</h1>

            // Use a translation system like ngx-translate for i18n support."
            // },
            {
            "Inefficient *ngFor Without trackBy",
            @"Using *ngFor without trackBy can lead to performance issues during DOM diffing.

            Example:
            <li *ngFor=""let item of items"">{{ item.name }}</li>

            Better:
            <li *ngFor=""let item of items; trackBy: trackByItemId"">{{ item.name }}</li>

            Component:
            trackByItemId(index: number, item: Item) { return item.id; }"
            },
            // {
            // "Large Component",
            // @"Components with too many responsibilities are hard to maintain.

            // Example:
            // A component that handles UI logic, API calls, and formatting.

            // Better:
            // Split into smaller components and delegate logic to services."
            // },
            // {
            // "Improper Service Injection",
            // @"Injecting services into components without specifying proper scopes can lead to shared state bugs.

            // Example:
            // providers: [SomeService] in component metadata

            // Better:
            // Declare services in modules or use @Injectable({ providedIn: 'root' }) for global scope."
            // },
            // {
            // "Direct DOM Manipulation",
            // @"Manipulating the DOM using document.querySelector or nativeElement can cause bugs and breaks Angular's rendering model.

            // Example:
            // document.querySelector('#myElement').classList.add('hidden');

            // Better:
            // Use Angular Renderer2 or structural directives like *ngIf and [ngClass]."
            // },
            // {
            // "Not Using Async Pipe",
            // @"Manually subscribing to Observables and not unsubscribing causes memory leaks.

            // Example:
            // ngOnInit() {
            // this.subscription = this.myService.getData().subscribe(data => this.data = data);
            // }

            // Better:
            // <div *ngIf=""data$ | async as data"">{{ data }}</div>
            // Use the async pipe to auto-subscribe and unsubscribe."
            // }
        };


        public static string GetDefinition(AngularCodeSmellType smellType)
        {
            var prettyName = PrettyNames[smellType];
            return Definitions.TryGetValue(prettyName, out var definition)
                ? definition
                : "No definition available.";
        }

        public static AngularCodeSmellType? ParseFromString(string prettyName)
        {
            return PrettyNames
                .FirstOrDefault(kvp => kvp.Value.Equals(prettyName, StringComparison.OrdinalIgnoreCase)).Key;
        }

    }
}

