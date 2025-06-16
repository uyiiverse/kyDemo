using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;

public class SimpleThreadPool : IDisposable
{
    private readonly ConcurrentQueue<Action> _taskQueue = new ConcurrentQueue<Action>();
    private readonly Thread[] _workers;
    private readonly ManualResetEvent _taskSignal = new ManualResetEvent(false);
    private bool _isRunning = true;
    private bool _disposed = false;
    public bool IsDisposed => _disposed;
    public SimpleThreadPool(int workerCount)
    {
        _workers = new Thread[workerCount];
        for (int i = 0; i < workerCount; i++)
        {
            _workers[i] = new Thread(WorkerThread) { IsBackground = true };
            _workers[i].Start();
        }
    }

    public void EnqueueTask(Action task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        _taskQueue.Enqueue(task);
        _taskSignal.Set(); // Signal that a new task is available
    }

    private void WorkerThread()
    {
        while (_isRunning)
        {
            _taskSignal.WaitOne(); // Wait until a task is available

            while (_taskQueue.TryDequeue(out var task))
            {
                task();
            }

            _taskSignal.Reset(); // Reset signal if no more tasks are available
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _isRunning = false;
        _taskSignal.Set(); // Wake up all threads to exit
        foreach (var worker in _workers)
        {
            worker.Join();
        }
        _taskSignal.Dispose();
        _disposed = true;
    }
}

public static class ThreadPoolManager
{
    public static SimpleThreadPool Pool { get; private set; }

    public static void Initialize(int workerCount)
    {
        if (Pool == null)
        {
            Pool = new SimpleThreadPool(workerCount);
        }
    }

    public static void Dispose()
    {
        Pool?.Dispose();
        Pool = null;
    }
}