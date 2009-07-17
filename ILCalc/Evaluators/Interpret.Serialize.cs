using System.Runtime.Serialization;

namespace ILCalc
{
	public sealed partial class Interpret
		: IEvaluator, IDeserializationCallback
	{
		void IDeserializationCallback.OnDeserialization( object sender )
		{
			this.stackArray = new double[stackMax];
			this.paramArray = new double[argsCount];
			this.syncRoot = new object();
		}
	}
}