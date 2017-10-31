namespace ParallelTreeWalker
{
    /// <summary>
    /// Options for customizing the tree walker behavior.
    /// </summary>
    public class TreeWalkerOptions
    {
        /// <summary>
        /// Determines how many concurrent operations may occur at the same time. 
        /// Default is 5. 1 means sequential processing.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = 5;

        internal static TreeWalkerOptions Default { get; } = new TreeWalkerOptions();
    }
}
