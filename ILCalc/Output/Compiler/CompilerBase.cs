using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace ILCalc
	{
	abstract class CompilerBase
		{
		#region Fields

		protected readonly DynamicMethod _eval;
		protected readonly ILGenerator _body;
		protected readonly bool _checkOvf;

		private readonly Stack<CallInfo> _calls;
		private bool _useParams;

		#endregion
		#region Constructor

		protected CompilerBase( Type returnType, Type[] paramTypes, bool check )
			{
			_checkOvf = check;
			_calls = new Stack<CallInfo>(4);
			_eval = new DynamicMethod("ilcalc", returnType, paramTypes, _valType, true);
			_body = _eval.GetILGenerator( );
			}

		#endregion
		#region IExpressionOutput

		public void PutNumber( double value )
			{
			_body.Emit(OpCodes.Ldc_R8, value);
			}

		public void PutOperator( int oper )
			{
			if( oper != Code.Pow )
				 _body.Emit(_stdOps[oper]);
			else _body.Emit(OpCodes.Call, _powMethod);
			}

		public void PutSeparator( )
			{
			if( !_useParams ) return;

			CallInfo c = _calls.Peek( );
			if( c.NextIsLastFixed( ) )
				{
				EmitParamArr(c.ParamsCount, c.Local);
				}
			else if( c.Current > 0 )
				{
				_body.Emit(_saveElem);
				_body.Emit(OpCodes.Ldloc, c.Local);
				_body_EmitLoadI4(c.Current);
				}
			}

		public void PutFunction( MethodInfo func )
			{
			CallInfo c = _calls.Pop( );
			if( c != null )
				{
				if( c.ParamsCount > 0 )
					{
					_body.Emit(_saveElem);
					_body.Emit(OpCodes.Ldloc, c.Local);
					}
				else
					{
					_body_EmitLoadI4(0);
					_body.Emit(OpCodes.Newarr, _valType);
					}
				}

			_body.Emit(OpCodes.Call, func);

			if( _calls.Count > 0 )
				{
				_useParams = (_calls.Peek( ) != null);
				}
			}

		public void BeginCall( int fixCount, int varCount )
			{
			_useParams = (fixCount >= 0);
			if(_useParams)
				{
				LocalBuilder local = (varCount > 0) ?
					_body.DeclareLocal(_arrType) : null;

				_calls.Push(
					new CallInfo(fixCount, varCount, local)
					);

				if( fixCount == 0 && varCount > 0 )
					{
					EmitParamArr(varCount, local);
					}
				}
			else _calls.Push(null);
			}

		public void PutExprEnd( )
			{
			//TODO: убрать
			//_body.Emit(OpCodes.Call, typeof( System.Diagnostics.Debugger ).GetMethod("Break"));

			//System.Diagnostics.Debugger.Break( );

			if( _checkOvf )
				{
				_body.Emit(OpCodes.Ckfinite);
				}
			}

		#endregion
		#region Helpers

		private sealed class CallInfo
			{
			#region Properties

			public bool NextIsLastFixed( )
				{
				return ++Current == 0;
				}

			public LocalBuilder Local { get; private set; }
			public int ParamsCount { get; private set; }
			public int Current { get; private set; }

			#endregion
			#region Constructor

			public CallInfo( int fixCount, int varCount, LocalBuilder local )
				{
				ParamsCount = varCount;
				Current = -fixCount;
				Local = local;
				}

			#endregion
			}

		private void EmitParamArr( int size, LocalBuilder local )
			{
			_body_EmitLoadI4(size);
			_body.Emit(OpCodes.Newarr, _valType);
			_body.Emit(OpCodes.Stloc, local);
			_body.Emit(OpCodes.Ldloc, local);
			_body_EmitLoadI4(0);
			}

		protected void _body_EmitLoadI4( int value )
			{
			if( value < sbyte.MinValue ||
				value > sbyte.MaxValue )
				{
				_body.Emit(OpCodes.Ldc_I4, value);
				}

			else if( value < -1 || value > 8 )
				{
				_body.Emit(OpCodes.Ldc_I4_S, ( byte ) value);
				}

			else if( value == -1 )
				 _body.Emit(OpCodes.Ldc_I4_M1);
			else _body.Emit(_loadConst[value]);
			}

		#endregion
		#region Static Data

		// Types ==================================================

		protected static readonly Type _valType = typeof( double );
		protected static readonly Type _arrType = typeof( double[] );

		// OpCodes ================================================

		private static readonly OpCode[] _loadConst =
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

		protected static readonly OpCode _loadElem = OpCodes.Ldelem_R8;
		protected static readonly OpCode _saveElem = OpCodes.Stelem_R8;

		private static readonly OpCode[] _stdOps =
			{
				OpCodes.Sub,
				OpCodes.Add,
				OpCodes.Mul,
				OpCodes.Div,
				OpCodes.Rem,
				OpCodes.Nop,
				OpCodes.Neg
			};

		private static readonly MethodInfo _powMethod 
			= typeof( Math ).GetMethod("Pow");

		#endregion
		}
	}