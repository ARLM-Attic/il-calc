using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace ILCalc
	{
	abstract class CompilerBase
		{
		#region Fields

		protected readonly DynamicMethod dynMethod;
		protected readonly ILGenerator body;
		
		private readonly bool checkedMode;
		private readonly Stack<CallInfo> callsStack;
		private bool useParams;

		#endregion
		#region Constructor

		protected CompilerBase( Type returnType, Type[] paramTypes, bool check )
			{
			checkedMode = check;
			callsStack = new Stack<CallInfo>(4);
			dynMethod = new DynamicMethod("ilcalc", returnType, paramTypes, valueType, true);
			body = dynMethod.GetILGenerator( );
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			body.Emit(OpCodes.Ldc_R8, value);
			}

		public void PutOperator( int oper )
			{
			if( oper != Code.Pow )
				 body.Emit(opOperators[oper]);
			else body.Emit(OpCodes.Call, powMethod);
			}

		public void PutSeparator( )
			{
			if( !useParams ) return;

			var c = callsStack.Peek( );
			if( c.NextIsLastFixed( ) )
				{
				body_EmitParamArr(c.VarCount, c.Local);
				}
			else if( c.Current > 0 )
				{
				body.Emit(opSaveElem);
				body.Emit(OpCodes.Ldloc, c.Local);
				body_EmitLoadI4(c.Current);
				}
			}

		public void PutBeginCall( )
			{
			callsStack.Push(null);
			useParams = false;
			}

		public void PutBeginParams( int fixCount, int varCount )
			{
			LocalBuilder local = (varCount > 0)?
				body.DeclareLocal(arrayType): null;

			var c = new CallInfo(fixCount, varCount, local);
			callsStack.Push(c);

			if( fixCount == 0 && varCount > 0 )
				{
				body_EmitParamArr(varCount, local);
				}

			useParams = true;
			}

		public void PutMethod( MethodInfo method, int argsCount )
			{
			if( argsCount < 0 )
				{
				var c = callsStack.Pop( );
				if( c.VarCount > 0 )
					{
					body.Emit(opSaveElem);
					body.Emit(OpCodes.Ldloc, c.Local);
					}
				else
					{
					body_EmitLoadI4(0);
					body.Emit(OpCodes.Newarr, valueType);
					}
				}
			else
				callsStack.Pop( );

			body.Emit(OpCodes.Call, method);

			if( callsStack.Count > 0 )
				{
				useParams = (callsStack.Peek( ) != null);
				}
			}

		public void PutExprEnd( )
			{
//			body.Emit(OpCodes.Call,
//				typeof(System.Diagnostics.Debugger)
//				.GetMethod("Break")
//				);

			if( checkedMode )
				{
				body.Emit(OpCodes.Ckfinite);
				}
			}

		#endregion
		#region Helpers

		private sealed class CallInfo
			{
			#region Members

			private readonly LocalBuilder local;
			private readonly int varCount;
			private int current;

			public LocalBuilder Local { get { return local;  } }
			public int VarCount	{ get { return varCount; } }
			public int Current	{ get { return current;  } }

			public bool NextIsLastFixed( )
				{
				return ++current == 0;
				}

			#endregion
			#region Constructor

			public CallInfo( int fixCount, int varCount, LocalBuilder local )
				{
				this.varCount = varCount;
				this.local = local;

				current = -fixCount;
				}

			#endregion
			}

		private void body_EmitParamArr( int size, LocalBuilder local )
			{
			body_EmitLoadI4(size);
			body.Emit(OpCodes.Newarr, valueType);
			body.Emit(OpCodes.Stloc, local);
			body.Emit(OpCodes.Ldloc, local);
			body_EmitLoadI4(0);
			}

		protected void body_EmitLoadI4( int value )
			{
			if( value < sbyte.MinValue ||
				value > sbyte.MaxValue )
				{
				body.Emit(OpCodes.Ldc_I4, value);
				}

			else if( value < -1 || value > 8 )
				{
				body.Emit(OpCodes.Ldc_I4_S, ( byte ) value);
				}

			else if( value == -1 )
				 body.Emit(OpCodes.Ldc_I4_M1);
			else body.Emit(opLoadConst[value]);
			}

		#endregion
		#region Static Data

		// Types ==================================================

		protected static readonly Type valueType = typeof( double );
		protected static readonly Type arrayType = typeof( double[] );

		// OpCodes ================================================

		private static readonly OpCode[] opLoadConst =
			{
				OpCodes.Ldc_I4_0,
				OpCodes.Ldc_I4_1,
				OpCodes.Ldc_I4_2,
				OpCodes.Ldc_I4_3,
				OpCodes.Ldc_I4_4,
				OpCodes.Ldc_I4_5,
				OpCodes.Ldc_I4_6,
				OpCodes.Ldc_I4_7,
				OpCodes.Ldc_I4_8
			};

		protected static readonly OpCode opLoadElem = OpCodes.Ldelem_R8;
		protected static readonly OpCode opSaveElem = OpCodes.Stelem_R8;

		private static readonly OpCode[] opOperators =
			{
				OpCodes.Sub,
				OpCodes.Add,
				OpCodes.Mul,
				OpCodes.Div,
				OpCodes.Rem,
				OpCodes.Nop,
				OpCodes.Neg
			};

		private static readonly MethodInfo powMethod
			= typeof( Math ).GetMethod("Pow");

		#endregion
		}
	}