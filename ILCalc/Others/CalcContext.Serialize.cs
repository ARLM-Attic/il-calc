using System.Runtime.Serialization;

namespace ILCalc
{
  public sealed partial class CalcContext<T>
    : IDeserializationCallback
  {
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      this.literalsList = new IListEnumerable[]
      {
        this.arguments,
        this.constants,
        this.functions
      };
    }
  }
}