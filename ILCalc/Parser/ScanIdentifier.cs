using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ILCalc
{
	internal sealed partial class Parser
	{
		private Item ScanIdenifier(ref int i)
		{
			int len = 0;

			List<Capture> matches = 
				(this.context.parseCulture != null) ?
				this.GetMatchesCulture(ref len) :
				this.GetMatchesOrdinal(ref len);

			if (len == 0)
			{
				throw this.UnresolvedIdentifier(1);
			}

			if (this.curPos + len < this.exprLen)
			{
				char c = this.expr[this.curPos + len];
				if (Char.IsLetterOrDigit(c) || c == '_')
				{
					throw this.UnresolvedIdentifier(len + 1);
				}
			}

			i += len - 1;

			return (matches.Count == 1) ?
				this.SimpleMatch(matches[0], ref i, len) :
				this.AmbiguousMatch(matches, ref i, len);
			}

		#region Matches

		private Item SimpleMatch(Capture match, ref int i, int len)
		{
			switch (match.Type)
			{
				case IdenType.Argument: // ===============================
				{
					this.output.PutArgument(match.Index);
					return Item.Identifier;
				}

				case IdenType.Constant: // ===============================
				{
					Debug.Assert(this.context.constants != null);

					this.output.PutNumber(this.context.constants[match.Index]);
					return Item.Identifier;
				}

				case IdenType.Function: // ===============================
				{
					int funcPos = this.curPos;
					if (!this.SkipBrace(ref i))
					{
						throw this.NoOpenBrace(funcPos, len);
					}

					FunctionGroup group = this.GetGroup(match);

					if (group.HasParamsFunctions)
					{
						IExpressionOutput oldOutput = this.output;
						var bufOutput = new BufferOutput();

						this.output = bufOutput;
						int argsCount = this.ParseNested(ref i, true);
						this.output = oldOutput;

						FunctionItem func = group.GetOverload(argsCount);
						if (func == null)
						{
							throw this.WrongArgsCount(funcPos, len, argsCount, group);
						}
						
						this.OutputBufferCall(bufOutput, func, argsCount);
					}
					else
					{
						this.output.PutBeginCall();

						int argsCount = this.ParseNested(ref i, true);

						FunctionItem func = group.GetOverload(argsCount);
						if (func == null)
						{
							throw this.WrongArgsCount(funcPos, len, argsCount, group);
						}
						
						this.output.PutFunction(func, argsCount);
					}

					return Item.End;
				}

				default:
					throw new NotSupportedException();
				}
			}

		private Item AmbiguousMatch(List<Capture> matches, ref int i, int len)
		{
			Debug.Assert(matches != null);

			var funcs = new List<Capture>();
			var idens = new List<Capture>();

			foreach (Capture match in matches)
			{
				if (IsFunc(match))
				{
					funcs.Add(match);
				}
				else
				{
					idens.Add(match);
				}
			}

			// ===================================== > 0 Identifiers ==
			if (funcs.Count == 0)
			{
				throw this.AmbiguousMatch(this.curPos, idens);
			}

			int funcPos = this.curPos;

			// ======================================= > 0 Functions ==
			if (idens.Count == 0)
			{
				if (!this.SkipBrace(ref i))
				{
					throw this.NoOpenBrace(funcPos, len);
				}

				return this.Resolve(funcs, null, ref i, funcPos, len);
			}
			
			// ==================== > 0 Functions and > 0 Identifiers ==
			int prevPos = i;
			if (!this.SkipBrace(ref i)) // no brace ahead
			{
				i = prevPos;
				if (idens.Count == 1)
				{
					return this.SimpleMatch(idens[0], ref i, len);
				}

				throw this.AmbiguousMatch(funcPos, idens);
			}

			return this.Resolve(funcs, idens, ref i, funcPos, len);
		}

		#endregion
		#region Methods

		private Item Resolve(
			List<Capture> funcs,
			List<Capture> idens,
			ref int i,
			int funcPos,
			int len)
		{
			Debug.Assert(funcs != null);

			IExpressionOutput old = this.output;
			var bufOutput = new BufferOutput();

			this.output = bufOutput;
			int argsCount = this.ParseNested(ref i, true);
			this.output = old;

			var overloads = this.GetOverloads(funcs, argsCount);

			// One argument: maybe identifier?
			if (idens != null
			 && argsCount == 1
			 && this.context.implicitMul)
			{
				// can't deduce if matched a function with 1 argument:
				if (overloads.Count != 0)
				{
					// TODO: wrong?
					throw this.AmbiguousMatch(funcPos, funcs);
				}

				// ore more than one idenifier match:
				if (idens.Count != 1)
				{
					throw this.AmbiguousMatch(funcPos, idens);
				}

				return this.SimpleMatch(idens[0], ref i, len);
			}

			if (overloads.Count == 1)
			{
				this.OutputBufferCall(bufOutput, overloads[0], argsCount);
				return Item.End;
			}
				
			if (overloads.Count == 0)
			{
				// TODO: provide more detailed error message
				throw this.WrongArgsCount(funcPos, len, argsCount, null);
			}

			throw this.AmbiguousMatch(funcPos, funcs);
		}

		private List<FunctionItem> GetOverloads(List<Capture> matches, int argsCount)
		{
			Debug.Assert(matches != null);
			Debug.Assert(argsCount >= 0);

			bool hasParams = false;
			int fixCount = -1;

			var overloads = new List<FunctionItem>();

			foreach (Capture match in matches)
			{
				FunctionGroup group = this.GetGroup(match);
				FunctionItem func = group.GetOverload(argsCount);

				if (func == null)
				{
					continue;
				}

				if (func.ArgsCount > fixCount)
				{
					overloads.Clear();
					overloads.Add(func);

					fixCount  = func.ArgsCount;
					hasParams = func.HasParamArray;
				}
				else if (func.ArgsCount == fixCount)
				{
					if (func.HasParamArray)
					{
						if (hasParams)
						{
							overloads.Add(func);
						}
					}
					else
					{
						if (hasParams)
						{
							hasParams = false;
							overloads.Clear();
						}
						
						overloads.Add(func);
					}
				}
			}

			return overloads;
		}

		private void OutputBufferCall(BufferOutput bufOutput, FunctionItem func, int argsCount)
		{
			Debug.Assert(bufOutput != null);
			Debug.Assert(argsCount >= 0);
			Debug.Assert(func != null);

			if (func.HasParamArray)
			{
				this.output.PutBeginParams(
					func.ArgsCount,
					argsCount - func.ArgsCount);
			}
			else
			{
				this.output.PutBeginCall();
			}

			bufOutput.WriteTo(this.output);

			this.output.PutFunction(func, argsCount);
		}

		#endregion
		#region Helpers

		private bool SkipBrace(ref int i)
		{
			while (i < this.exprLen
				&& char.IsWhiteSpace(this.expr[i]))
			{
				i++;
			}

			if (i >= this.exprLen)
			{
				return false;
			}

			this.curPos = i;
			this.prePos = this.curPos;

			char c = this.expr[i++];

			return c == '(';
		}

		// TODO: merge with Parse
		private int ParseNested(ref int i, bool func)
		{
			int beginPos = this.curPos;

			this.prePos = this.curPos;
			this.exprDepth++;

			int args = Parse(ref i, func);
			if (args == -1 && this.exprDepth > 0)
			{
				throw BraceDisbalance(beginPos, false);
			}
			
			this.exprDepth--;
			return args;
		}

		private FunctionGroup GetGroup(Capture match)
		{
			return context.functions[match.Index];
		}

		#endregion
		#region Search

		private enum IdenType
		{
			Argument,
			Constant,
			Function
		}

		private static bool IsFunc(Capture match)
		{
			return match.Type == IdenType.Function;
		}

		private List<Capture> GetMatchesCulture(ref int max)
		{
			var matches = new List<Capture>();

			CultureInfo culture = context.parseCulture;

#if SILVERLIGHT
			var compare = context.ignoreCase?
				CompareOptions.IgnoreCase:
				CompareOptions.None;
#else
			bool ignoreCase = context.ignoreCase;
#endif

			foreach (SearchItem list in idenList)
			{
				int id = 0;
				foreach (string name in list.Names)
				{
					int length = name.Length;
					if (length >= max &&
#if SILVERLIGHT
						String.Compare(this.expr, this.curPos, name, 0, length, culture, compare) == 0)
#else
						String.Compare(this.expr, this.curPos, name, 0, length, ignoreCase, culture) == 0)
#endif
					{
						if (length != max)
						{
							matches.Clear();
						}

						matches.Add(new Capture(list.Type, id));
						max = length;
					}

					id++;
				}
			}

			return matches;
		}

		private List<Capture> GetMatchesOrdinal(ref int max)
		{
			var match = new List<Capture>();

			var strCmp = this.context.ignoreCase ?
				StringComparison.OrdinalIgnoreCase :
				StringComparison.Ordinal;

			foreach (SearchItem list in idenList)
			{
				int id = 0;
				foreach (string name in list.Names)
				{
					int length = name.Length;
					if (length >= max &&
						String.Compare(this.expr, this.curPos, name, 0, length, strCmp) == 0)
					{
						if (length != max)
						{
							match.Clear();
						}

						match.Add(new Capture(list.Type, id));
						max = length;
					}

					id++;
				}
			}

			return match;
		}

		private struct SearchItem
		{
			private readonly IEnumerable<string> names;
			private readonly IdenType type;

			public SearchItem(IdenType type, IEnumerable<string> names)
			{
				Debug.Assert(names != null);

				this.names = names;
				this.type = type;
			}

			public IEnumerable<string> Names
			{
				get { return this.names; }
			}

			public IdenType Type
			{
				get { return this.type; }
			}
		}

		private struct Capture
		{
			private readonly IdenType type;
			private readonly int index;

			public Capture(IdenType type, int index)
			{
				Debug.Assert(index >= 0);

				this.index = index;
				this.type = type;
			}

			public IdenType Type
			{
				get { return this.type; }
			}

			public int Index
			{
				get { return this.index; }
			}
		}

		#endregion
		}
	}