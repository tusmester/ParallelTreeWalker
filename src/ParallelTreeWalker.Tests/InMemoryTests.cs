using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Linq;
using System.Diagnostics;

namespace ParallelTreeWalker.Tests
{
    public class TestElement : ITreeElement<TestElement>
    {
        public IEnumerable<TestElement> Children { get; set; }
        public bool IsContainer { get; set; }
        public bool Visited { get; set; }
    }

    [TestClass]
    public class InMemoryTests
    {
        //====================================================================== Visitor tests

        [TestMethod]
        public async Task EmptyLeaf()
        {
            var root = GetTree_EmptyLeaf();
            await TestWalk(root, 1, 3);
        }
        [TestMethod]
        public async Task EmptyContainer()
        {
            var root = GetTree_EmptyContainer();
            await TestWalk(root, 1, 2);
        }
        [TestMethod]
        public async Task OneLevel_OnlyContainers()
        {
            var root = GetTree_OneLevel_OnlyContainers();
            await TestWalk(root, 3, 2);
        }
        [TestMethod]
        public async Task OneLevel_OnlyLeaves()
        {
            var root = GetTree_OneLevel_OnlyLeaves();
            await TestWalk(root, 3, 2);
        }
        [TestMethod]
        public async Task OneLevel_Mixed()
        {
            var root = GetTree_OneLevel_Mixed();
            await TestWalk(root, 5, 2);
        }
        [TestMethod]
        public async Task TwoLevels_Balanced()
        {
            var root = GetTree_TwoLevels_Balanced();
            await TestWalk(root, 7, 2);
        }        
        [TestMethod]
        public async Task FourLevels_Unbalanced()
        {
            var root = GetTree_FourLevels_Unbalanced();
            await TestWalk(root, 9, 2);
        }

        //====================================================================== Parallel tests

        [TestMethod]
        public async Task TwoLevels_Balanced_Parallel()
        {
            var root = GetTree_TwoLevels_Balanced();
            await TestWalk(root, 7, 3, 1000, 4000, 4100);
        }
        [TestMethod]
        public async Task FourLevels_Unbalanced_Parallel()
        {
            var root = GetTree_FourLevels_Unbalanced_2();
            await TestWalk(root, 21, 5, 1000, 7000, 7100);
        }
        [TestMethod]
        public async Task DeepTree_Parallel_Sequential()
        {
            // Parents are processed before children, so parallel behavior cannot occur
            // here. Containers are processed sequentially, one after the other.
            var root = GetTree_Deep();
            await TestWalk(root, 6, 10, 500, 3000, 3100);
        }
        [TestMethod]
        public async Task WideTree_Parallel()
        {
            var root = GetTree_Wide();
            await TestWalk(root, 61, 10, 1000, 7000, 7100);
        }

        //====================================================================== Walk

        public async Task TestWalk(TestElement root, int elementCount, int maxParallel, int delay = 0, int expectedMinTime = 0, int expectedMaxTime = 0)
        {
            var allElements = new ConcurrentBag<TestElement>();
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await TreeWalker<TestElement>.WalkAsync(root, async (el) =>
            {
                Trace.WriteLine(string.Format("##PTW> STARTTIME: {0}", DateTime.UtcNow.ToLongTimeString()));

                if (delay > 0)
                    await Task.Delay(delay);
                
                el.Visited = true;

                allElements.Add(el);
            }, new TreeWalkerOptions
            {
                MaxDegreeOfParallelism = maxParallel,
            });

            stopwatch.Stop();

            Trace.WriteLine(string.Format("##PTW> ELAPSED: {0}", stopwatch.ElapsedMilliseconds));

            if (delay > 0)
            {
                Assert.IsTrue(stopwatch.ElapsedMilliseconds >= expectedMinTime);
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < expectedMaxTime);
            }

            Assert.AreEqual(elementCount, allElements.Count);
            Assert.IsTrue(allElements.All(e => e.Visited));
        }

        //====================================================================== Generate trees

        private static TestElement GetTree_EmptyLeaf()
        {
            return new TestElement();
        }
        private static TestElement GetTree_EmptyContainer()
        {
            return new TestElement() { IsContainer = true };
        }
        private static TestElement GetTree_OneLevel_OnlyContainers()
        {
            // R
            // +---C
            // +---C

            return new TestElement() {
                IsContainer = true,
                Children = new TestElement[] 
                {
                    new TestElement() { IsContainer = true },
                    new TestElement() { IsContainer = true }
                }
            };
        }
        private static TestElement GetTree_OneLevel_OnlyLeaves(int leafCount = 2)
        {
            // R
            // +---L
            // +---L
            // +...

            return new TestElement()
            {
                IsContainer = true,
                Children = Enumerable.Range(0, leafCount).Select(i => new TestElement()).ToArray()
            };
        }
        private static TestElement GetTree_OneLevel_Mixed()
        {
            // R
            // +---C
            // +---C
            // +---L
            // +---L

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    new TestElement() { IsContainer = true },
                    new TestElement() { IsContainer = true },
                    new TestElement(),
                    new TestElement()
                }
            };
        }
        private static TestElement GetTree_TwoLevels_Balanced()
        {
            // R
            // +---C
            //     +---L
            //     +---L
            // +---C
            //     +---L
            //     +---L

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    GetTree_OneLevel_OnlyLeaves(),
                    GetTree_OneLevel_OnlyLeaves()
                }
            };
        }
        private static TestElement GetTree_FourLevels_Unbalanced()
        {
            // R
            // +---C
            //     +---C
            //         +---C
            //             +---L
            //             +---L
            // +---C
            //     +---L
            //     +---L

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    new TestElement
                    {
                        IsContainer = true,
                        Children = new TestElement[]
                        {
                            new TestElement
                            {
                                IsContainer = true,
                                Children = new TestElement[]
                                {
                                    GetTree_OneLevel_OnlyLeaves()
                                }
                            }
                        }
                    },
                    GetTree_OneLevel_OnlyLeaves()
                }
            };
        }
        private static TestElement GetTree_FourLevels_Unbalanced_2()
        {
            // R
            // +---C
            //     +---C
            //         +---C
            //         |   +---L
            //         |   +---L
            //         |   +---L
            //         |   +---L
            //         +---C
            //         |   +---L
            //         |   +---L
            //         |   +---L
            //         |   +---L
            //         +---C
            //             +---L
            //             +---L
            //             +---L
            //             +---L
            // +---C
            //     +---L
            //     +---L

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    new TestElement
                    {
                        IsContainer = true,
                        Children = new TestElement[]
                        {
                            new TestElement
                            {
                                IsContainer = true,
                                Children = new TestElement[]
                                {
                                    GetTree_OneLevel_OnlyLeaves(4),
                                    GetTree_OneLevel_OnlyLeaves(4),
                                    GetTree_OneLevel_OnlyLeaves(4)
                                }
                            }
                        }
                    },
                    GetTree_OneLevel_OnlyLeaves()
                }
            };
        }
        private static TestElement GetTree_Deep()
        {
            // R
            // +---C
            //     +---C
            //         +---C
            //             +---C
            //                 +---C

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    new TestElement()
                    {
                        IsContainer = true,
                        Children = new TestElement[]
                        {
                            new TestElement()
                            {
                                IsContainer = true,
                                Children = new TestElement[]
                                {
                                    new TestElement()
                                    {
                                        IsContainer = true,
                                        Children = new TestElement[]
                                        {
                                            new TestElement()
                                            {
                                                IsContainer = true,
                                                Children = new TestElement[]
                                                {
                                                    new TestElement()
                                                    {
                                                        IsContainer = true
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
        private static TestElement GetTree_Wide()
        {
            // R
            // +---C
            //     +---L
            //     +---L
            //     +---L
            //     +---L
            //     +---L
            //
            // ...x 10

            return new TestElement()
            {
                IsContainer = true,
                Children = new TestElement[]
                {
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5),
                    GetTree_OneLevel_OnlyLeaves(5)
                }
            };
        }
    }
}
