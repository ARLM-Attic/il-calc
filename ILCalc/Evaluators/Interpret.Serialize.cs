using System.Runtime.Serialization;

namespace ILCalc
{
	public sealed partial class Interpret
		: IEvaluator, IDeserializationCallback
	{
		void IDeserializationCallback.OnDeserialization (object sender)
		{
			this.stackArray = new double[stackMax];
			this.paramArray = new double[argsCount];
			this.syncRoot = new object();

			switch(this.argsCount)
			{
				case 1:  this.asyncTab = (TabFunc1) Tab1Impl; break;
				case 2:  this.asyncTab = (TabFunc2) Tab2Impl; break;
				default: this.asyncTab = (TabFuncN) TabNImpl; break;
			}
		}
	}
}