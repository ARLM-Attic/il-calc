using System.Runtime.Serialization;

namespace ILCalc
{
	// TODO: add to other targets
	// TODO: test!!!!!!

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
