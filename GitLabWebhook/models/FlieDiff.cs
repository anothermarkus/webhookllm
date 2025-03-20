namespace Models
{
    public class FileDiff
    {
        public string FileName { get; set; }
        public string BaseSha { get; set; }
        public string StartSha { get; set; }
        public string HeadSha { get; set; }
        public string Diff { get; set; }
        public int LineForComment { get; set; } 
        public string LLMComment { get; set; }
        public bool HasSuggestion { get; set; }
    }
}
