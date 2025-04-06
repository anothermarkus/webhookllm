namespace GitLabWebhook.Models
{
    /// <summary>
    /// Represents a file diff.
    /// </summary>
    public class FileDiff
    {
        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the contents of the file.
        /// </summary>
        public string FileContents { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base SHA of the file.
        /// </summary>
        public string BaseSha { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start SHA of the file.
        /// </summary>
        public string StartSha { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the head SHA of the file.
        /// </summary>
        public string HeadSha { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the diff.
        /// </summary>
        public string Diff { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line number for the comment.
        /// </summary>
        public int LineForComment { get; set; } = -1;

        /// <summary>
        /// Gets or sets the LLM comment.
        /// </summary>
        public string LLMComment { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the file has a suggestion.
        /// </summary>
        public bool HasSuggestion { get; set; } = false;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public string GetFileNameAndDiff()
        {
            return $"[FileName={FileName},Diff={Diff}]";
        }
    }
}
