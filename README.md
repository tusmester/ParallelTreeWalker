# ParallelTreeWalker
An extensible and highly scalable **.Net component** for processing elements in a **tree** in an **async** and **parallel** way.

* it is able to process **huge trees**, like a large directory structure in the file system.
* **parent folders** are always processed before children.
* **parallel**: you decide how many tree elements can be visited at the same time.
* **efficient**: consumed resources depend only on how many parallel operations you allow, not on the size of the tree.
* **async**: when a tree element is visited, you can perform an asynchronous operation (e.g. execute a database command or web request).

## Usage

First implement the *ITreeElement* interface (relax, it is simple) to represent your tree element - or if you want to discover the file system, you can use the built-in *FileSystemElement* class.

Just call the *WalkAsync* method with a root element and it will do the rest. In the options parameter you can define a callback function (*ProcessElementAsync*) that will be executed every time a tree element is reached.

```c#
await TreeWalker.WalkAsync(root, new TreeWalkerOptions
{
    MaxDegreeOfParallelism = 5,
    ProcessElementAsync = async (element) =>
    {
        var el = element as FileSystemElement;
        var path = el.Path;
        var isDirectory = el.IsDirectory;
        
        await DoStuffAsync(el);
    }
});
```
