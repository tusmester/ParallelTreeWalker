# ParallelTreeWalker
An extensible and highly scalable **.Net component** for processing elements in a **tree** in an **async** and **parallel** way.

* it is able to process **huge trees**, like a large directory structure in the file system.
* **parent folders** are always processed before children.
* **parallel**: you decide how many tree elements can be visited at the same time.
* **efficient**: consumed resources depend only on how many parallel operations you allow, not on the size of the tree.
* **async**: when a tree element is visited, you can perform an asynchronous operation (e.g. execute a database command or web request).

[Latest release](https://github.com/tusmester/ParallelTreeWalker/releases/latest)

## Usage
First implement the *ITreeElement* interface (relax, it is simple) to represent your tree element - or if you want to discover the file system, you can use the built-in *FileSystemElement* class.

Just call the *WalkAsync* method with a root element and it will do the rest. You can define a callback function that will be executed every time a tree element is reached.

```c#
await TreeWalker<FileSystemElement>.WalkAsync(root, async (element) =>
{
    var path = element.Path;
    var isDirectory = element.IsDirectory;

    await DoStuffAsync(element);
}, 
new TreeWalkerOptions
{
    MaxDegreeOfParallelism = 10
});
```
