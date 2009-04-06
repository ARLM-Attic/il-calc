using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using ILCalc;
using ILCalc.Console;

namespace ILCalc_Console
	{
	public class Program
		{
		static void Main( )
			{
			#region Initialize

			Console.Title = "ILCalc Console";

			var asmName = Assembly.GetAssembly(typeof(CalcContext)).GetName();

			Console.WriteLine("{0} v{1} by Shvedov A. V. (type ? for help)", asmName.Name, asmName.Version);
			Console.WriteLine("Arithmetical expression evaluator\n");

			var calc = new CalcContext( );

			calc.Constants.ImportBuiltin( );
			calc.Functions.ImportBuiltin( );
			calc.Functions.Import(typeof(Program), typeof(Functions));
			calc.Constants.Add("max", 1.23);

			Mode mode = Mode.Interpret;
			var range = new TabRange(-100, 100, 2);

			#endregion

			while(true)
				{
				#region Input

				Console.ForegroundColor = ConsoleColor.Gray;

				string input = InputLine().TrimEnd(' ');

				Console.CursorLeft = input.Length % Console.WindowWidth;
				Console.CursorTop--;

				#endregion
				#region Commands

				Console.ForegroundColor = ConsoleColor.Yellow;

				if( input == "exit" ) break;
				if( input == "args" )		 ShowCollection(calc.Arguments, ' ');
				else if( input == "consts" ) ShowCollection(calc.Constants.Keys, ' ');
				else if( input == "funcs"  ) ShowCollection(calc.Functions, '\t');
				else if( input == "case" )
					{
					calc.IgnoreCase = !calc.IgnoreCase;
					Console.WriteLine(calc.IgnoreCase ? " = insensitive" : " = sensitive");
					}
				else if( input == "check" )
					{
					calc.OverflowCheck = !calc.OverflowCheck;
					Console.WriteLine(" = {0}", calc.OverflowCheck ? "true" : "false");
					}
				else if(input == "range")
					{
					Console.WriteLine();
					Console.Write(" begin = "); range.Begin = InputDouble(calc.Culture);
					Console.Write(" end   = "); range.End   = InputDouble(calc.Culture);
					Console.Write(" step  = "); range.Step  = InputDouble(calc.Culture);
					}
				else if( input == "gc" )
					{
					GC.Collect();
					GC.WaitForPendingFinalizers();
					Console.WriteLine(" collected");
					}
				else if( input == "save" ) SaveCalc(calc);
				else if( input == "load" ) calc = LoadCalc( );
				else if( input == "addconst" ) AddConstant(calc);
				else if( input == "culture" ) SetCulture(calc);
				else if( input == "compile" )
					{
					mode = Mode.Compile;
					calc.Arguments.Clear( );
					Console.WriteLine(" : using evaluator");
					}
				else if(input == "interp")
					{
					mode = Mode.Interpret;
					calc.Arguments.Clear();
					Console.WriteLine(" : using interpret");
					}
				else if(input == "tab")
					{
					mode = Mode.Tabulate;
					calc.Arguments = new ArgumentCollection("x");
					Console.WriteLine(" : using tabulator");
					}
				else if(input == "help" || input == "?")
					{
					ShowHelp( );
					}
				else if( input == "cls" )
					{
					Console.Clear( );
					}
				
				#endregion
				#region Evaluation

				else
					{
					double res;
					Console.ForegroundColor = ConsoleColor.White;
					try
						{
						if(mode == Mode.Tabulate)
							{
							double[] arr = calc
								.CreateTabulator(input)
								.Tabulate(range);

							Console.Write(" => ");
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine(range);
							Console.WriteLine( );

							Console.ForegroundColor = ConsoleColor.Cyan;
							if(calc.Culture != null)
								{
								foreach( double value in arr )
									{
									Console.WriteLine(value.ToString(calc.Culture.NumberFormat));
									}
								}
							else
								{
								int pos = 0;
								Console.Write(' ');
								foreach( double value in arr )
									{
									if( value >= 0.0 ) Console.Write(' ');

									Console.Write(value.ToString("N5"));
									if( ++pos == 8 )
										{
										pos = 0;
										Console.WriteLine( );
										Console.Write(' ');
										}
									else Console.Write(" ");
									}

								Console.WriteLine('\n');
								}
							}
						else
							{
							res = (mode == Mode.Compile)?
								calc.CreateEvaluator(input).Evaluate() :
								calc.Evaluate(input);

							Console.Write(" = ");
							if(calc.Culture != null)
								 Console.WriteLine(res.ToString(calc.Culture.NumberFormat));
							else Console.WriteLine(res);
							}
						}
					catch( SyntaxException e )
						{
						ShowSyntaxException(input, e);
						}
					catch( Exception e )
						{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine(" => {0}", e.GetType().Name);
						Console.ForegroundColor = ConsoleColor.DarkGray;
						Console.WriteLine(e.Message);
						}
					}

				#endregion
				}
			}

		private static void ShowHelp( )
			{
			Console.WriteLine("\nAvalible Commands:");
			Console.WriteLine(" exit                 - quit from application");
			Console.WriteLine(" args, consts, funcs  - list available items");
			Console.WriteLine(" case, check          - switch context options");
			Console.WriteLine(" addconst             - add constant to the list");
			Console.WriteLine(" compile, interp, tab - select evaluation mode");
			Console.WriteLine(" load, save           - context serialization");
			Console.WriteLine(" culture              - set parse culture");
			}

		private static void AddConstant( CalcContext A )
			{
			Console.WriteLine( );
			Console.Write(" name  = "); string name  = InputLine();
			Console.Write(" value = "); double value = InputDouble(A.Culture);
					
			try 
				{
				A.Constants.Add(name, value);
				}
			catch( Exception e )
				{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(e.Message);
				}
			}

		private static void ShowSyntaxException( string input, SyntaxException e )
			{
			if( e.Length != 0 )
				{
				Console.CursorLeft = e.Position;
				Console.BackgroundColor = ConsoleColor.Red;
				Console.ForegroundColor = ConsoleColor.Black;
				Console.Write(e.Substring);
				Console.BackgroundColor = ConsoleColor.Black;
				Console.CursorLeft = input.Length % Console.WindowWidth;
				}
			
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(" => syntax error");
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine(e.Message);
			}

		private static void SetCulture( CalcContext A )
			{
			Console.Write(@" show\inv\cur\null\<name> : ");
			string ans = InputLine( );

			if(ans == "show")
				{
				var culs = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
				Array.Sort(culs, ( c1, c2 ) => string.CompareOrdinal(c1.Name, c2.Name));

				foreach(CultureInfo culture in culs)
					{
					Console.Write(culture.Name);
					Console.Write('\t');
					}
				Console.WriteLine();
				}
			else if(ans == "inv")  A.Culture = CultureInfo.InvariantCulture;
			else if(ans == "cur")  A.Culture = CultureInfo.CurrentCulture;
			else if(ans == "null") A.Culture = null;
			else
				{
				try {
					A.Culture = CultureInfo.GetCultureInfo(ans);
					}
				catch( ArgumentException )
					{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Error: culture \"{0}\" not founded!", ans);
					Console.ForegroundColor = ConsoleColor.Yellow;
					}
				}

			if(A.Culture != null)
				 Console.WriteLine("=> {0}", A.Culture.DisplayName);
			else Console.WriteLine("=> ordinal");
			}

		private static CalcContext LoadCalc( )
			{
			CalcContext calc;
			var formatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					TypeFormat = FormatterTypeStyle.TypesWhenNeeded
				};

			using(var file = new FileStream("ilcalc.dat", FileMode.Open, FileAccess.Read))
				{
				calc = (CalcContext) formatter.Deserialize(file);
				}

			Console.WriteLine(": loaded!");
			return calc;
			}

		private static void SaveCalc( CalcContext A )
			{
			var formatter = new BinaryFormatter
				{
					AssemblyFormat = FormatterAssemblyStyle.Simple,
					TypeFormat = FormatterTypeStyle.TypesWhenNeeded
				};

			using(var file = new FileStream("ilcalc.dat", FileMode.Create, FileAccess.Write))
				{
				formatter.Serialize(file, A);
				}

			Console.WriteLine(": saved!");
			}

		private static void ShowCollection( IEnumerable<string> list, char separator )
			{
			Console.WriteLine(':');
			foreach( string name in list )
				{
				Console.Write(name);
				Console.Write(separator);
				}
			Console.WriteLine();
			}

		private static string InputLine( )
			{
			return Console.ReadLine() ?? string.Empty;
			}

		private static double InputDouble( IFormatProvider culture )
			{
			double value;
			bool res;
			int left = Console.CursorLeft;
			int top  = Console.CursorTop;

			do	{
				string str = Console.ReadLine() ?? string.Empty;

				res = (culture == null)?
					double.TryParse(str, out value):
					double.TryParse(str, NumberStyles.Float, culture, out value);

				if( res ) continue;

				Console.CursorTop  = top;
				Console.CursorLeft = left;

				for(int i = 0; i < str.Length; i++)
					{
					Console.Write(' ');
					}
				
				Console.CursorTop = top;
				Console.CursorLeft = left;
				}
			while( !res );
			
			return value;
			}

		private enum Mode
			{
			Interpret,
			Compile,
			Tabulate
			}
		}
	}