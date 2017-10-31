using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelTreeWalker
{
    public interface ITreeElement<T> where T:ITreeElement<T>
    {
        IEnumerable<T> Children { get; }
        bool IsContainer { get; }
    }
}
