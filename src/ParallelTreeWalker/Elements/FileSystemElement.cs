using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParallelTreeWalker.Elements
{
    public class FileSystemElement : ITreeElement<FileSystemElement>
    {
        public string Path { get; }
        public bool IsContainer { get; }

        public IEnumerable<FileSystemElement> Children
        {
            get
            {
                // enumerate directories, than files
                return Directory.EnumerateDirectories(Path).Select(dir => new FileSystemElement(dir, true))
                    .Concat(Directory.EnumerateFiles(Path).Select(file => new FileSystemElement(file, false)));
            }
        }

        public FileSystemElement(string path, bool isContainer)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(path);

            Path = path;
            IsContainer = isContainer;
        }
    }
}
