namespace GitLabWebhook.Models
{

    public enum CodeFramework
    {
        Unknown,
        Angular,
        DotNet
    }

    public static class FrameworkDetector
    {
        public static CodeFramework DetectFrameworkFromFiles(List<string> changedFiles)
        {
            bool hasCsproj = changedFiles.Any(f => f.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
            bool hasCSharp = changedFiles.Any(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
            bool hasAngularJson = changedFiles.Any(f => f.EndsWith("angular.json", StringComparison.OrdinalIgnoreCase));
            bool hasTypescript = changedFiles.Any(f => f.EndsWith(".ts", StringComparison.OrdinalIgnoreCase));

            if (hasAngularJson || (hasTypescript && changedFiles.Any(f => f.Contains("/src/app/"))))
            {
                return CodeFramework.Angular;
            }

            if (hasCsproj || hasCSharp)
            {
                return CodeFramework.DotNet;
            }

            return CodeFramework.Unknown;
        }
    }

}