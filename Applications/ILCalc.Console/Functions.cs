using System;

namespace ILCalc.Console
	{
	public static class Functions
		{
		public static double Sum(double arg, params double[] args)
			{
			var sum = arg;
			foreach(var a in args) sum += a;
			return sum;
			}

		public static double MAX( double arg, params double[] args )
			{
			var max = arg;
			foreach(var a in args) if(a > max) max = a;
			return max;
			}

		public static double Min( double arg, params double[] args )
			{
			var min = arg;
			foreach(var a in args) if(a < min) min = a;
			return min;
			}

		public static double Avg( double arg, params double[] args )
			{
			var sum = arg;
			var n = 1;
			foreach(var a in args) { sum += a; n++; }
			return sum / n;
			}

		public static double GeomMean( double arg, params double[] args )
			{
			var num = arg;
			var n = 1;
			foreach(var a in args) { num *= a; n++; }
			return Math.Pow(num, 1 / (double)n);
			}

		public static double HarmMean( double arg, params double[] args )
			{
			var num = 1 / arg;
			var n = 1;
			foreach(var a in args) { num += 1 / a; n++; }
			return n / num;
			}

		public static double QuadMean( double arg, params double[] args )
			{
			var num = arg * arg;
			var n = 1;
			foreach(var a in args) { num += a * a; n++; }
			return Math.Sqrt(n / num);
			}

		public static double Midrange( double arg, params double[] args )
			{
			var max = arg;
			var min = arg;
			foreach(var a in args)
				{
				if( a < min ) min = a;
				if( a > max ) max = a;
				}
			return (max + min)/2;
			}
		}
	}