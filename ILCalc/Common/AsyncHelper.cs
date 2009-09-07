using System;
using System.Diagnostics;
using System.Threading;

namespace ILCalc
{
  sealed class AsyncHelper<T>
    : IAsyncResult, IDisposable
  {
    #region Fields

    volatile bool completed;
    readonly object state;
    readonly AsyncCallback callback;
    readonly object syncWait;
    readonly object syncTask;
    Exception except;
    ManualResetEvent waitHandle;
    T result;

    //TODO: correct message
    static readonly InvalidOperationException
      EndedTwice = new InvalidOperationException("twice end");

    #endregion
    #region Constructor

    public AsyncHelper(Func<T> task)
      : this(task, null, null) { }

    public AsyncHelper(
      Func<T> task, AsyncCallback callback, object state)
    {
      if (task == null)
        throw new ArgumentNullException("task");

      this.state = state;
      this.callback = callback;
      this.syncWait = new object();
      this.syncTask = new object();

      BeginInvoke(task);
    }

    #endregion
    #region IAsyncResult

    public object AsyncState
    {
      get { return this.state; }
    }

    public WaitHandle AsyncWaitHandle
    {
      get
      {
        lock (this.syncWait)
          if (this.waitHandle == null)
          {
            this.waitHandle = new ManualResetEvent(false);
            if (this.completed)
              this.waitHandle.Set();
          }

        return this.waitHandle;
      }
    }

    public bool CompletedSynchronously
    {
      get { return false; }
    }

    public bool IsCompleted
    {
      get { return this.completed; }
    }

    #endregion
    #region Methods

    void BeginInvoke(Func<T> task)
    {
      ThreadPool.QueueUserWorkItem(o =>
      {
        lock (this.syncTask)
        {
          try { this.result = task(); }
          catch (Exception e) { this.except = e; }
          finally { this.completed = true; }
        }

        // notify the wait handle:
        lock (this.syncWait)
        {
          if (this.waitHandle != null)
            this.waitHandle.Set();
        }

        // invoke the callback:
        if (this.callback != null)
          this.callback(this);
      });
    }

    public T EndInvoke()
    {
      while (!this.completed)
      {
        // wait for task completion
#if !CF
        Thread.SpinWait(1);
#endif
        Debug.WriteLine(">");
        lock (this.syncTask) { }
      }

      if (this.except != null)
        throw this.except;

      this.except = EndedTwice;
      return this.result;
    }

    #endregion
    #region IDisposable

    public void Dispose()
    {
      lock (this.syncWait)
        if (this.waitHandle != null)
          this.waitHandle.Close();
    }

    #endregion
  }
}
