namespace ILCalc
{
  enum Code
  {
    // operators:
    Sub = 0, Add = 1,
    Mul = 2, Div = 3,
    Mod = 4, Pow = 5,
    Neg = 6,

    // elements:
    Number    = 8,
    Argument  = 9,
    Function  = 10,
    Separator = 11,
    ParamCall = 12,
    BeginCall = 13,

    // for Interpret:
    Delegate0 = 16,
    Delegate1 = 17,
    Delegate2 = 18,

    Return = int.MaxValue,
  }

  static class CodeHelper
  {
    public static bool IsOp(Code code)
    {
      return code < Code.Number;
    }
  }
}