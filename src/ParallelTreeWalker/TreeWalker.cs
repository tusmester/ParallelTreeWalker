using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTreeWalker
{
    public class TreeWalker<T> where T:ITreeElement<T>
    {
        private readonly T _rootElement;
        private readonly Func<T, Task> _processElementAsync;
        private readonly TreeWalkerOptions _options;

        private readonly SemaphoreSlim _workerSemaphore;
        private readonly SemaphoreSlim _mainSemaphore;
        private readonly IProducerConsumerCollection<T> _containerCollection;

        //================================================================================================== Construction

        private TreeWalker(T root, Func<T, Task> processElementAsync, TreeWalkerOptions options = null)
        {
            _rootElement = root;
            _processElementAsync = processElementAsync;
            _options = options ?? TreeWalkerOptions.Default;

            _workerSemaphore = new SemaphoreSlim(_options.MaxDegreeOfParallelism);
            _mainSemaphore = new SemaphoreSlim(1);

            // a LIFO collection for storing containers temporarily for later processing 
            _containerCollection = new ConcurrentStack<T>();
        }

        //================================================================================================== Static API

        /// <summary>
        /// Processes elements in a tree in parallel. Parent items are always processed before their children.
        /// </summary>
        /// <param name="root">Root element of the tree.</param>
        /// <param name="options">Options for customizing tree processing.</param>
        public static async Task WalkAsync(T root, Func<T,Task> processElementAsync, TreeWalkerOptions options = null)
        {
            if (root == null)
                throw new ArgumentNullException("root");

            // create an instance to let clients start multiple tree walk operations in parallel
            var treeWalker = new TreeWalker<T>(root, processElementAsync, options);

            await treeWalker.WalkInternalAsync();
        }

        //================================================================================================== Internal instance API

        private async Task WalkInternalAsync()
        {
            // enter the main processing phase
            await _mainSemaphore.WaitAsync();

            //process root element first
            await _processElementAsync(_rootElement);

            // algorithm: recursive, limited by the max degree of parallelism option
            await StartProcessingChildrenAsync(_rootElement);

            // check if there is any work to do (e.g. no folders and files at all)
            if (TreeWalkIsCompleted())
                _mainSemaphore.Release();

            // This is where we wait for the whole process to end. This semaphore will be
            // released by one of the subtasks when it finishes its job and realizes that
            // there are no more folders in the queue and no more tasks to wait for.
            // This technique is better than calling Task.Delay in a loop.
            await _mainSemaphore.WaitAsync();
        }

        private async Task StartProcessingChildrenAsync(T element)
        {
            // This methods enumerates direct child elements and starts
            // tasks for creating them - but only in a pace as there are
            // available 'slots' for them (determined by the configured
            // max degree of parallelism).

            var children = element.Children;
            if (children != null)
            {
                // start tasks for child elements
                foreach (var childElemenet in children)
                {
                    // start a new task only if we did not exceed the max concurrent limit 
                    await _workerSemaphore.WaitAsync();

#pragma warning disable CS4014
                    // start the task but do not wait for it
                    ProcessElementAsync(childElemenet);
#pragma warning restore CS4014
                }
            }

            // this makes sure that the process ends after processing the last, empty container
            if (TreeWalkIsCompleted())
                _mainSemaphore.Release();
        }
        private async Task ProcessElementAsync(T element)
        {
            try
            {
                await _processElementAsync(element);

                // after a container has been processed, it is allowed to deal with its children
                if (element.IsContainer)
                    _containerCollection.TryAdd(element);
            }
            catch (Exception ex)
            {
                //UNDONE: handle error
            }
            finally
            {
                ReleaseSemaphoreAndContinue();
            }
        }

        //================================================================================================== Helper methods

        private bool TryProcessingNextContainer()
        {
            T container;

            if (!_containerCollection.TryTake(out container))
                return false;

#pragma warning disable CS4014
            // start the task but do not wait for it
            StartProcessingChildrenAsync(container);
#pragma warning restore CS4014

            return true;
        }
        private void ReleaseSemaphoreAndContinue()
        {
            // release the worker semaphore to let other threads start new creator tasks
            _workerSemaphore.Release();

            // Try to process another container. If the queue is empty and there are no
            // working jobs (the worker semaphore does not block anything) that means
            // we can safely set the main semaphore and end the whole process.
            if (!TryProcessingNextContainer() && _workerSemaphore.CurrentCount == _options.MaxDegreeOfParallelism)
                _mainSemaphore.Release();
        }
        private bool TreeWalkIsCompleted()
        {
            return _workerSemaphore.CurrentCount == _options.MaxDegreeOfParallelism && _containerCollection.Count == 0;
        }
    }
}
