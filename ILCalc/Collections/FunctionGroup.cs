using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

// TODO: fix DebugView
// TODO: ICollection?

namespace ILCalc
{
  using State = DebuggerBrowsableState;

  /// <summary>
  /// Represents the function overload group that contains
  /// <see cref="FunctionItem{T}"/> items with different
  /// values of <see cref="FunctionItem{T}.ArgsCount"/> and
  /// <see cref="FunctionItem{T}.HasParamArray"/> properties.<br/>
  /// This class cannot be inherited.</summary>
  /// <typeparam name="T">Functions parameters
  /// and return value type.</typeparam>
  /// <threadsafety instance="false"/>
  [DebuggerDisplay("{Count} functions")]
  [Serializable]
  public sealed class FunctionGroup<T>
    : IEnumerable<FunctionItem<T>>
  {
    #region Fields

    [DebuggerBrowsable(State.RootHidden)]
    readonly List<FunctionItem<T>> funcList;

    #endregion
    #region Constructors

    internal FunctionGroup(FunctionItem<T> function)
    {
      Debug.Assert(function != null);

      this.funcList = new
        List<FunctionItem<T>>(1) { function };
    }

    // For clone
    internal FunctionGroup(FunctionGroup<T> other)
    {
      Debug.Assert(other != null);

      this.funcList = new List<
        FunctionItem<T>>(other.funcList);
    }

    #endregion
    #region Properties

    /// <summary>
    /// Gets the count of <see cref="FunctionItem{T}">
    /// functions</see> that this group represents.</summary>
    [DebuggerBrowsable(State.Never)]
    public int Count
    {
      get { return this.funcList.Count; }
    }

    /// <summary>
    /// Gets the <see cref="FunctionItem{T}"/>
    /// at the specified index.</summary>
    /// <param name="index">The index of the
    /// <see cref="FunctionItem{T}"/> to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.<br/>-or-<br/>
    /// <paramref name="index"/> is equal to or
    /// greater than <see cref="Count"/></exception>
    /// <returns>The <see cref="FunctionItem{T}"/>
    /// at the specified index.</returns>
    public FunctionItem<T> this[int index]
    {
      get { return this.funcList[index]; }
    }

    #endregion
    #region Methods

    // TODO: maybe Add & ICollection<T>?

    /// <summary>
    /// Removes the <see cref="FunctionItem{T}"/> with
    /// the specified <paramref name="argsCount"/> and
    /// <paramref name="hasParamArray"/> values from
    /// the <see cref="FunctionGroup{T}"/>.</summary>
    /// <param name="argsCount">
    /// <see cref="FunctionItem{T}"/> arguments count.</param>
    /// <param name="hasParamArray">Indicates that
    /// <see cref="FunctionItem{T}"/> has an parameters array.</param>
    /// <returns><b>true</b> if specified <see cref="FunctionItem{T}"/>
    /// is founded in the group and was removed;
    /// otherwise, <b>false</b>.</returns>
    public bool Remove(int argsCount, bool hasParamArray)
    {
      for (int i = 0; i < Count; i++)
      {
        FunctionItem<T> func = this.funcList[i];

        if (func.ArgsCount == argsCount &&
            func.HasParamArray == hasParamArray)
        {
          this.funcList.RemoveAt(i);
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Removes the <see cref="FunctionItem{T}"/> at the specified
    /// index of the <see cref="FunctionGroup{T}"/>.</summary>
    /// <param name="index">The zero-based index
    /// of the <see cref="FunctionItem{T}"/> to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0,
    /// equal to or greater than Count.</exception>
    public void RemoveAt(int index)
    {
      this.funcList.RemoveAt(index);
    }

    /// <summary>
    /// Removes all <see cref="FunctionItem{T}">functions</see>
    /// from the <see cref="FunctionGroup{T}"/>.</summary>
    public void Clear()
    {
      this.funcList.Clear();
    }

    /// <summary>
    /// Determines whether a <see cref="FunctionItem{T}"/>
    /// with the specified <paramref name="argsCount"/> and
    /// <paramref name="hasParamArray"/> values is contains
    /// in the <see cref="FunctionGroup{T}"/>.</summary>
    /// <param name="argsCount">
    /// <see cref="FunctionItem{T}"/> arguments count.</param>
    /// <param name="hasParamArray">Indicates that
    /// <see cref="FunctionItem{T}"/> has an parameters array.</param>
    /// <returns><b>true</b> if function is found in the group;
    /// otherwise, <b>false</b>.</returns>
    public bool Contains(int argsCount, bool hasParamArray)
    {
      foreach (FunctionItem<T> func in this.funcList)
      {
        if (func.ArgsCount == argsCount &&
            func.HasParamArray == hasParamArray)
        {
          return true;
        }
      }

      return false;
    }

    #endregion
    #region IEnumerable<>

    /// <summary>
    /// Returns an enumerator that iterates through
    /// the <see cref="FunctionItem{T}">functions</see>
    /// in <see cref="FunctionGroup{T}"/>.</summary>
    /// <returns>An enumerator for the all <see cref="FunctionItem{T}">
    /// functions</see> in <see cref="FunctionGroup{T}"/>.</returns>
    public IEnumerator<FunctionItem<T>> GetEnumerator()
    {
      return this.funcList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.funcList.GetEnumerator();
    }

    #endregion
    #region Internals

    internal bool Append(FunctionItem<T> func)
    {
      Debug.Assert(func != null);

      foreach (FunctionItem<T> f in this.funcList)
      {
        if (func.ArgsCount == f.ArgsCount &&
            func.HasParamArray == f.HasParamArray)
        {
          return false;
        }
      }

      this.funcList.Add(func);
      return true;
    }

    internal string MakeMethodsArgsList()
    {
      switch (this.funcList.Count)
      {
        case 0: return string.Empty;
        case 1: return this.funcList[0].ArgsString;
      }

      var buf = new StringBuilder();
      this.funcList.Sort(ArgsCountComparator);

      // output first:
      buf.Append(this.funcList[0].ArgsString);

      // and others:
      for (int i = 1, last = Count - 1; i < Count; i++)
      {
        FunctionItem<T> func = this.funcList[i];

        if (i == last)
        {
          buf.Append(' ')
             .Append(Resource.sAnd)
             .Append(' ');
        }
        else buf.Append(", ");

        buf.Append(func.ArgsString);
      }

      return buf.ToString();
    }

    internal FunctionItem<T> GetOverload(int argsCount)
    {
      Debug.Assert(argsCount >= 0);

      FunctionItem<T> best = null;
      int fixCount = -1;

      foreach (var func in this.funcList)
      {
        if (func.HasParamArray)
        {
          if (func.ArgsCount <= argsCount &&
              func.ArgsCount > fixCount)
          {
            best = func;
            fixCount = func.ArgsCount;
          }
        }
        else if (func.ArgsCount == argsCount)
        {
          return func;
        }
      }

      return best;
    }

    static int ArgsCountComparator(
      FunctionItem<T> a, FunctionItem<T> b)
    {
      if (a.ArgsCount == b.ArgsCount)
      {
        if (a.HasParamArray == b.HasParamArray)
          return 0;

        return a.HasParamArray ? 1 : -1;
      }

      return a.ArgsCount < b.ArgsCount ? -1 : 1;
    }

    #endregion
  }
}