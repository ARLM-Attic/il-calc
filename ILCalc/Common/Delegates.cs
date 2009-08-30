namespace ILCalc
{
  /// <summary>
  /// Represents the compiled expression
  /// with no arguments.</summary>
  /// <typeparam name="T">Expression values type.</typeparam>
  /// <returns>Evaluated value.</returns>
  public delegate T EvalFunc0<T>();

  /// <summary>
  /// Represents the compiled expression
  /// with one argument.</summary>
  /// <typeparam name="T">Expression values type.</typeparam>
  /// <param name="arg">Expression argument.</param>
  /// <returns>Evaluated value.</returns>
  public delegate T EvalFunc1<T>(T arg);

  /// <summary>
  /// Represents the compiled expression
  /// with two arguments.</summary>
  /// <typeparam name="T">Expression values type.</typeparam>
  /// <param name="arg1">First expression argument.</param>
  /// <param name="arg2">Second expression argument.</param>
  /// <returns>Evaluated value.</returns>
  public delegate T EvalFunc2<T>(T arg1, T arg2);

  /// <summary>
  /// Represents the compiled expression
  /// with three or more arguments.</summary>
  /// <typeparam name="T">Expression values type.</typeparam>
  /// <param name="args">Expression arguments.</param>
  /// <returns>Evaluated value.</returns>
  public delegate T EvalFuncN<T>(params T[] args);
}