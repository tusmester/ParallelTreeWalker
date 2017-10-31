using Microsoft.VisualStudio.TestTools.UnitTesting;
using ParallelTreeWalker.Elements;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelTreeWalker.Tests
{
    [TestClass]
    public class FileSystemTests
    {
        private readonly string ROOTNAME = "TestRoot";

        [TestMethod]
        public async Task TwoLevels_Balanced()
        {
            var rootDir = CreateTestStructure();

            var allVisitedPath = await TestWalk(new FileSystemElement(rootDir, true), 5);
            var actual = string.Join(Environment.NewLine, allVisitedPath);

            // expected results: enumerate manually, add root dir and sort
            var expected = string.Join(Environment.NewLine, 
                Enumerable.Union(new List<string> { rootDir }, Directory.EnumerateFileSystemEntries(rootDir, "*", SearchOption.AllDirectories)).OrderBy(p => p));

            Assert.AreEqual(expected, actual);
        }

        public async Task<string[]> TestWalk(FileSystemElement root, int maxParallel)
        {
            var allPaths = new ConcurrentBag<string>();

            await TreeWalker< FileSystemElement>.WalkAsync(root, (el) =>
            {
                Trace.WriteLine(string.Format("##PTW> {0} Path: {1}", DateTime.UtcNow.ToLongTimeString(), el.Path));

                allPaths.Add(el.Path);

                return Task.FromResult<object>(null);
            }, new TreeWalkerOptions
            {
                MaxDegreeOfParallelism = maxParallel,
            });

            return allPaths.OrderBy(p => p).ToArray();
        }

        private string CreateTestStructure()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = Path.Combine(currentDir, ROOTNAME);

            // cleanup
            if (Directory.Exists(rootDir))
                Directory.Delete(rootDir, true);

            var subDirs = new string[]
            {
                Path.Combine(rootDir, @"F1\F11"),
                Path.Combine(rootDir, @"F1\F12"),
                Path.Combine(rootDir, @"F2\F21"),
                Path.Combine(rootDir, @"F2\F22"),
                Path.Combine(rootDir, @"F3\F31"),
                Path.Combine(rootDir, @"F3\F32"),
            };

            foreach (var subDir in subDirs)
            {
                Directory.CreateDirectory(subDir);

                using (var writer = File.CreateText(Path.Combine(subDir, "F1.txt")))
                {
                    writer.Write("TEST - " + DateTime.UtcNow);
                }
            }

            return rootDir;
        }
    }
}
