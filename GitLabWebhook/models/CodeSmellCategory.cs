
namespace GitLabWebhook.models
{
    public enum CodeSmellType
    {
        LongMethod,
        GodClass,
        FeatureEnvy,
        DataClumps,
        DuplicateCode,
        ShotgunSurgery,
        DivergentChange,
        LazyClass,
        SpeculativeGenerality,
        SwitchStatements,
        TemporaryField,
        MessageChains,
        MiddleMan,
        PrimitiveObsession,
        CommentsSmell,
        SelectorDuplication
    }

    public static class CodeSmells
    {
        public static readonly Dictionary<CodeSmellType, string> PrettyNames = new()
        {
            // { CodeSmellType.LongMethod, "Long Method" },
            // { CodeSmellType.GodClass, "God Class" },
            // { CodeSmellType.FeatureEnvy, "Feature Envy" },
            // { CodeSmellType.DataClumps, "Data Clumps" },
            // { CodeSmellType.DuplicateCode, "Duplicate Code" },
            // { CodeSmellType.ShotgunSurgery, "Shotgun Surgery" },
            // { CodeSmellType.DivergentChange, "Divergent Change" },
            // { CodeSmellType.LazyClass, "Lazy Class" },
            // { CodeSmellType.SpeculativeGenerality, "Speculative Generality" },
            // { CodeSmellType.SwitchStatements, "Switch Statements" },
            // { CodeSmellType.TemporaryField, "Temporary Field" },
            // { CodeSmellType.MessageChains, "Message Chains" },
            // { CodeSmellType.MiddleMan, "Middle Man" },
            // { CodeSmellType.PrimitiveObsession, "Primitive Obsession" },
            // { CodeSmellType.CommentsSmell, "Comments Smell" },
            { CodeSmellType.SelectorDuplication, "Selector Duplication" }
        };

        public static readonly Dictionary<string, string> Definitions = new()
        {
            // {
            //     "Long Method",
            //     "A method that is too long and tries to do too much. It should be broken into smaller methods. Example: " +
            //     "void ProcessOrder() { " +
            //     "// hundreds of lines of logic " +
            //     "}"
            // },
            // {
            //     "God Class",
            //     "A class that knows too much or does too much, violating single responsibility. Example: " +
            //     "class OrderManager { " +
            //     "void CreateOrder() { } " +
            //     "void SendEmail() { } " +
            //     "void LogActivity() { } " +
            //     "}"
            // },
            // {
            //     "Feature Envy",
            //     "A method that accesses data from another object more than its own. Example: " +
            //     "void PrintUser(User user) { " +
            //     "Console.WriteLine(user.GetName()); " +
            //     "Console.WriteLine(user.GetEmail()); " +
            //     "}"
            // },
            // {
            //     "Data Clumps",
            //     "Groups of variables that are always used together and should be encapsulated in a class. Example: " +
            //     "void Register(string firstName, string lastName, string email) { } " +
            //     "// Better: void Register(User user)"
            // },
            // {
            //     "Duplicate Code",
            //     "The same or similar code appears in multiple places; should be refactored into a single method. Example: " +
            //     "if (status == 'active') { DoSomething(); } " +
            //     "// Appears again elsewhere"
            // },
            // {
            //     "Shotgun Surgery",
            //     "A change in one place requires many small changes in different classes. Example: " +
            //     "Renaming a field in a base class causes edits in 10+ subclasses and consumers."
            // },
            // {
            //     "Divergent Change",
            //     "A class that suffers from too many responsibilities and needs to change for many different reasons. Example: " +
            //     "class ReportManager { " +
            //     "void GenerateReport() { } " +
            //     "void SendEmail() { } " +
            //     "void Archive() { } " +
            //     "}"
            // },
            // {
            //     "Lazy Class",
            //     "A class that is not doing enough to justify its existence. Example: " +
            //     "class EmailHelper { " +
            //     "void Send() { Smtp.Send(); } " +
            //     "}"
            // },
            // {
            //     "Speculative Generality",
            //     "Code that is more abstract or flexible than needed, anticipating future needs that may never come. Example: " +
            //     "interface IDataProcessor<T> where T : class { } // when only one T is ever used"
            // },
            // {
            //     "Switch Statements",
            //     "Repeated switch or if-else chains that could be replaced with polymorphism. Example: " +
            //     "switch (shape.Type) { " +
            //     "case 'Circle': DrawCircle(); break; " +
            //     "case 'Square': DrawSquare(); break; " +
            //     "}"
            // },
            // {
            //     "Temporary Field",
            //     "Fields that are only sometimes used; leads to unclear object states. Example: " +
            //     "class Report { " +
            //     "string? footerText; // only used if IncludeFooter is true " +
            //     "}"
            // },
            // {
            //     "Message Chains",
            //     "A long chain of method calls like a.getB().getC().doSomething(); breaks encapsulation. Example: " +
            //     "var country = order.Customer.Address.Country.Name;"
            // },
            // {
            //     "Middle Man",
            //     "A class that delegates most of its work to other classes; can often be removed. Example: " +
            //     "class OrderService { " +
            //     "public void Process() => _processor.Process(); " +
            //     "}"
            // },
            // {
            //     "Primitive Obsession",
            //     "Overuse of primitives instead of creating meaningful domain classes. Example: " +
            //     "void CreateUser(string name, string email, string role) " +
            //     "// Better: void CreateUser(User user)"
            // },
            // {
            //     "Comments Smell",
            //     "Excessive or outdated comments that suggest the code is unclear or doing too much. Example: " +
            //     "// This method sorts users by last name alphabetically " +
            //     "users.Sort((a, b) => a.LastName.CompareTo(b.LastName));"
            // },
            {
                "Selector Duplication",
                "The same UI component or selector is repeated multiple times with different conditions, instead of being rendered dynamically using an *ngFor loop with data-driven logic. Example: " +
                "<my-selector *ngIf=\"showA\"></my-selector> " +
                "<my-selector *ngIf=\"showB\"></my-selector> " +
                "// Better: use *ngFor with a filtered array"
            }
        };


        public static string GetDefinition(CodeSmellType smellType)
        {
            var prettyName = PrettyNames[smellType];
            return Definitions.TryGetValue(prettyName, out var definition)
                ? definition
                : "No definition available.";
        }

        public static CodeSmellType? ParseFromString(string prettyName)
        {
            return PrettyNames
                .FirstOrDefault(kvp => kvp.Value.Equals(prettyName, StringComparison.OrdinalIgnoreCase)).Key;
        }

        public static string ToPromptFriendlyString()
        {
            var lines = PrettyNames.Select(kvp =>
            {
                var name = kvp.Value;
                var definition = Definitions.TryGetValue(name, out var def)
                    ? def
                    : "No definition available.";
                return $"- **{name}**: {definition}";
            });

            return "The following are common code smells and their definitions:\n\n" + string.Join("\n", lines);
        }
    }
}

