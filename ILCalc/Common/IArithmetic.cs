namespace ILCalc.Custom
{
  interface IArithmetic<T>
  {
    T Zero { get; }
    T One  { get; }

    T Neg(T x);
    T Add(T x, T y); T Sub(T x, T y);
    T Mul(T x, T y); T Div(T x, T y);
    T Mod(T x, T y); T Pow(T x, T y);

    bool IsEqual(T x, T y);
    bool IsGreather(T x, T y);
    bool IsGrOrEqual(T x, T y);
    int? IsIntergal(T value);
  }

  interface IRangeSupport<T>
  {
    T StepFromCount(ValueRange<T> r, int count);
    int GetCount(ValueRange<T> r);
    ValueRangeValidness Validate(ValueRange<T> r);
  }
}