using System.Runtime.Serialization;

namespace ILCalc
	{
	public sealed partial class Interpret : IDeserializationCallback
		{
		#region IDeserialization

		void IDeserializationCallback.OnDeserialization( object sender )
			{
			_stack = new double[_stackMax];
			_param = new double[_argCount];
			}

		#endregion
		}
	}
