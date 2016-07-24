using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ParallelTreeWalker.Elements
{
    public class FileSystemElement : ITreeElement
    {
        public string Path { get; private set; }
        public bool IsContainer { get; private set; }

        public IEnumerable<ITreeElement> Children
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
