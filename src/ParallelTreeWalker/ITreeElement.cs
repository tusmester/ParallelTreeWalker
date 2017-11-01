using System.Collections.Generic;

namespace Skraalsoft.ParallelTreewalker
{
    public interface ITreeElement<T> where T:ITreeElement<T>
    {
        IEnumerable<T> Children { get; }
        bool IsContainer { get; }
    }
}
