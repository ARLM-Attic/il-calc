using System.Runtime.Serialization;

namespace ILCalc
	{
	public sealed partial class Interpret : IEvaluator, IDeserializationCallback
		{
		void IDeserializationCallback.OnDeserialization( object sender )
			{
			stackArray = new double[stackMax];
			paramArray = new double[argsCount];
			syncRoot = new object( );
			}
		}
	}
