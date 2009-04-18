using System.Collections;
using System.Collections.Generic;

namespace ILCalc.Tests
	{
	public class OptimizerModes
		{
		public static Enumerator All( )
			{
			return new Enumerator( );
			}

		public struct Enumerator : IEnumerable<OptimizeModes>,
								   IEnumerator<OptimizeModes>
			{
			private const int enumLast = ( int ) OptimizeModes.PerformAll + 1;
			private int i;

			public OptimizeModes Current	{ get { return (OptimizeModes) i; } }
			object IEnumerator.Current		{ get { return (OptimizeModes) i; } }

			public void Dispose( )	{ }
			public void Reset( )	{ i = 0; }

			public bool MoveNext( )
				{
				if( i < enumLast ) { i++; return true; }
				return false;
				}

			public IEnumerator<OptimizeModes> GetEnumerator( )	{ return this; }
			IEnumerator			  IEnumerable.GetEnumerator( )	{ return this; }
			}
		}
	}
