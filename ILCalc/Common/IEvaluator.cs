namespace ILCalc
{
  /// <summary>
  /// Represents the object
  /// for the expression evaluation.
  /// </summary>
  /// <typeparam name="T">
  /// Expression values type.</typeparam>
  /// <seealso cref="Evaluator{T}"/>
  /// <seealso cref="Interpret{T}"/>
  public interface IEvaluator<T>
  {
    /// <summary>
    /// Gets the arguments count, that this
    /// <see cref="IEvaluator{T}"/> implemented for.
    /// </summary>
    int ArgumentsCount { get; }

    /// <summary>
    /// Invokes the expression evaluation
    /// with providing no arguments.</summary>
    /// <overloads>Invokes the expression evaluation.</overloads>
    /// <returns>Evaluated value.</returns>
    T Evaluate();

    /// <summary>
    /// Invokes the expression evaluation
    /// with providing one argument.</summary>
    /// <param name="arg">Expression argument.</param>
    /// <returns>Evaluated value.</returns>
    T Evaluate(T arg);

    /// <summary>
    /// Invokes the expression evaluation
    /// with providing two arguments.</summary>
    /// <param name="arg1">First expression argument.</param>
    /// <param name="arg2">Second expression argument.</param>
    /// <returns>Evaluated value.</returns>
    T Evaluate(T arg1, T arg2);

    /// <summary>
    /// Invokes the expression evaluation
    /// with providing the specified arguments.</summary>
    /// <param name="args">Expression arguments.</param>
    /// <returns>Evaluated value.</returns>
    T Evaluate(params T[] args);

    /// <summary>
    /// Returns the expression string, that this
    /// <see cref="IEvaluator{T}"/> represents.</summary>
    /// <returns>Expression string.</returns>
    string ToString();
  }
}