using System;

namespace ILCalc
{
  public sealed partial class CalcContext<T>
  {
    #region Methods

    /// <summary>
    /// Compiles the <see cref="Evaluator{T}"/> object for evaluating
    /// the specified <paramref name="expression"/>.</summary>
    /// <param name="expression">Expression to compile.</param>
    /// <exception cref="SyntaxException"><paramref name="expression"/>
    /// contains syntax error(s) and can't be compiled.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="expression"/> is null.</exception>
    /// <remarks>Not available in the .NET CF versions.</remarks>
    /// <returns><see cref="Evaluator{T}"/> object
    /// for evaluating expression.</returns>
    public Evaluator<T> CreateEvaluator(string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      var compiler = new EvaluatorCompiler<T>(ArgsCount, OverflowCheck);
      ParseOptimized(expression, compiler);

      return new Evaluator<T>(
        expression, compiler.CreateDelegate(), ArgsCount);
    }

    /// <summary>
    /// Compiles the <see cref="Tabulator{T}"/> object
    /// for evaluating the specified <paramref name="expression"/>
    /// in some ranges of arguments.</summary>
    /// <param name="expression">Expression to compile.</param>
    /// <exception cref="SyntaxException">
    /// <paramref name="expression"/> contains syntax error(s)
    /// and can't be compiled.</exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="expression"/> is null.</exception>
    /// <exception cref="ArgumentException">
    /// Current expression's arguments <see cref="Arguments">count</see>
    /// is not supported (only 1 or 2 arguments supported by now).</exception>
    /// <remarks>Not available in the .NET CF versions.</remarks>
    /// <returns><see cref="Tabulator{T}"/> object
    /// for evaluating expression.</returns>
    public Tabulator<T> CreateTabulator(string expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");
      if (ArgsCount == 0)
        throw new ArgumentException(Resource.errTabulatorWrongArgs);

      var compiler = new TabulatorCompiler<T>(ArgsCount, OverflowCheck);
      ParseOptimized(expression, compiler);

      var del = compiler.CreateDelegate();

      if (ArgsCount > 2)
      {
        var alloc = TabulatorCompiler<T>.GetAllocator(ArgsCount);
        return new Tabulator<T>(expression, del, ArgsCount, alloc);
      }

      return new Tabulator<T>(expression, del, ArgsCount);
    }

    #endregion
  }
}