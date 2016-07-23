using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Called every time when a tree element is reached. This is the place for element
        /// processing (e.g. uploading a file). The method is called with await.
        /// </summary>
        public Func<ITreeElement, Task> ProcessElementAsync { get; set; }

        internal static TreeWalkerOptions Default { get; } = new TreeWalkerOptions();

        /// <summary>
        /// Creates a new instance of ImportOptions for configuring the import behavior.
        /// </summary>
        public TreeWalkerOptions()
        {
            // default values
            MaxDegreeOfParallelism = 5;
        }
    }
}
