using System.Runtime.Serialization;

namespace ILCalc
{
	public sealed partial class CalcContext : IDeserializationCallback
	{
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			this.literalsList = new IQuickEnumerable[]
			{
				this.arguments,
				this.constants,
				this.functions
			};
		}
	}
}
