using System.Collections.Generic;

namespace Skraalsoft.ParallelTreeWalker
{
    public interface ITreeElement<T> where T:ITreeElement<T>
    {
        IEnumerable<T> Children { get; }
        bool IsContainer { get; }
    }
}
