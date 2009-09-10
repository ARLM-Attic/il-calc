using System.Runtime.Serialization;

namespace ILCalc
{
  public abstract partial class Interpret<T>
    : IEvaluator<T>, IDeserializationCallback
  {
    void IDeserializationCallback.OnDeserialization(object sender)
    {
      this.stackArray = new T[stackMax];
      this.paramArray = new T[argsCount];
      this.syncRoot = new object();

      switch (this.argsCount)
      {
        case 1: this.asyncTab = (TabFunc1) Tab1Impl; break;
        case 2: this.asyncTab = (TabFunc2) Tab2Impl; break;
        default: this.asyncTab = (TabFuncN) TabNImpl; break;
      }
    }
  }
}