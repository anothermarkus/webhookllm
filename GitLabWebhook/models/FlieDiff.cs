namespace Models
{
    public class FileDiff
    {
        public string FileName { get; set; }
        public string FileContents { get; set; }
        public string BaseSha { get; set; }
        public string StartSha { get; set; }
        public string HeadSha { get; set; }
        public string Diff { get; set; }
        public int LineForComment { get; set; }
        public string LLMComment { get; set; }
        public bool HasSuggestion { get; set; }

        // Override ToString to provide a custom string representation of the FileDiff object
        public override string ToString()
        {
            return $"FileDiff [FileName={FileName}, BaseSha={BaseSha}, StartSha={StartSha}, HeadSha={HeadSha}, " +
                   $"Diff={Diff?.Substring(0, Math.Min(50, Diff.Length))}..., LineForComment={LineForComment}, " +
                   $"LLMComment={LLMComment}, HasSuggestion={HasSuggestion}]";
        }
    }
}
