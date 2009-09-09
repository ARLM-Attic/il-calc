using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ILCalc
{
  using State = DebuggerBrowsableState;
  using Browsable = DebuggerBrowsableAttribute;

  // TODO: derived for targeting functions?

  /// <summary>
  /// Represents the imported function that has
  /// ability to be used in some expression.</summary>
  /// <typeparam name="T">Function's parameters
  /// and return value type.</typeparam>
  /// <remarks>
  /// Instance of this class is an immutable.<br/>
  /// Class has no public constructors.</remarks>
  /// <threadsafety instance="true"/>
  [DebuggerDisplay("{ArgsCount} args", Name = "{Method}")]
  [Serializable]
  public sealed class FunctionItem<T>
  {
    #region Fields

    [Browsable(State.Never)] readonly int fixCount;
    [Browsable(State.Never)] readonly bool hasParams;
    [Browsable(State.Never)] readonly MethodInfo method;
    [Browsable(State.Never)] readonly object target;

    #endregion
    #region Constructor

    internal FunctionItem(
      MethodInfo method, object target, int argsCount, bool hasParams)
    {
      Debug.Assert(method != null);
      Debug.Assert(method.IsStatic == (target == null));
      Debug.Assert(argsCount >= 0);

      this.hasParams = hasParams;
      this.fixCount = argsCount;
      this.method = method;
      this.target = target;
    }

    #endregion
    #region Properties

    /// <summary>
    /// Gets the function arguments count.
    /// </summary>
    public int ArgsCount
    {
      get { return this.fixCount; }
    }

    /// <summary>
    /// Gets a value indicating whether
    /// function has an parameters array.
    /// </summary>
    public bool HasParamArray
    {
      get { return this.hasParams; }
    }

    /// <summary>
    /// Gets the method reflection this
    /// <see cref="FunctionItem{T}"/> represents.
    /// </summary>
    public MethodInfo Method
    {
      get { return this.method; }
    }

    /// <summary>
    /// Gets the method name.
    /// </summary>
    public string MethodName
    {
      get { return this.method.Name; }
    }

    /// <summary>
    /// Gets the method full name
    /// (including declaring type name).
    /// </summary>
    public string FullName
    {
      get
      {
        var buf = new StringBuilder();

        buf.Append(this.method.DeclaringType.FullName);
        buf.Append('.');
        buf.Append(this.method.Name);

        return buf.ToString();
      }
    }

    /// <summary>
    /// Gets the method target for instance methods.
    /// For static methods this property will return null.
    /// </summary>
    public object Target
    {
      get { return this.target; }
    }

    [Browsable(State.Never)]
    internal string ArgsString
    {
      get
      {
        return HasParamArray ?
          this.fixCount.ToString() + '+' :
          this.fixCount.ToString();
      }
    }

    #endregion
    #region Methods

    /// <summary>
    /// Determine the ability of function to take specified
    /// <paramref name="count"/> of arguments.</summary>
    /// <param name="count">Arguments count.</param>
    /// <returns><b>true</b> if function can takes
    /// <paramref name="count"/> arguments;
    /// otherwise <b>false</b>.</returns>
    public bool CanTake(int count)
    {
      return count >= 0 &&
        HasParamArray ?
        count >= this.fixCount :
        count == this.fixCount;
    }

    /// <summary>
    /// Invokes this <see cref="FunctionItem{T}"/>
    /// via reflection.</summary>
    /// <param name="arguments">Function arguments.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="arguments"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="arguments"/> doesn't contains
    /// needed arguments count.</exception>
    /// <returns>Returned value.</returns>
    public T Evaluate(params T[] arguments)
    {
      if (arguments == null)
        throw new ArgumentNullException("arguments");

      if (!CanTake(arguments.Length))
      {
        throw new ArgumentException(string.Format(
          Resource.errWrongArgsCount,
          arguments.Length,
          ArgsString));
      }

      return Invoke(arguments, arguments.Length);
    }

    #endregion
    #region Invoke

    internal T Invoke(T[] array, int argsCount)
    {
      return Invoke(array, argsCount - 1, argsCount);
    }

    internal T Invoke(T[] stack, int pos, int argsCount)
    {
      Debug.Assert(stack != null);
      Debug.Assert(stack.Length > pos);
      Debug.Assert(stack.Length >= argsCount);

      object[] fixArgs;

      if (this.hasParams)
      {
        int varCount = argsCount - this.fixCount;
        var varArgs = new T[varCount];

        // TODO: perform loop, based on varArgs.Length

        // fill params array:
        for (int i = varCount - 1; i >= 0; i--)
        {
          varArgs[i] = stack[pos--];
        }

        fixArgs = new object[this.fixCount + 1];
        fixArgs[this.fixCount] = varArgs;
      }
      else
      {
        fixArgs = new object[this.fixCount];
      }

      // fill arguments array:
      for (int i = this.fixCount - 1; i >= 0; i--)
      {
        fixArgs[i] = stack[pos--];
      }

      // invoke via reflection
      try //TODO: is it right?
      {
        return (T)
          this.method.Invoke(this.target, fixArgs);
      }
      catch (TargetInvocationException ex)
      {
        throw ex.InnerException;
      }
    }

    internal T Invoke(object[] fixedArgs)
    {
      Debug.Assert(fixedArgs != null);

      return (T) this.method
        .Invoke(this.target, fixedArgs);
    }

    #endregion
  }
}