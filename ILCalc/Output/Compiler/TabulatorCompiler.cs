//#define VISUALIZE
using System;
using System.Reflection.Emit;

namespace ILCalc
	{
	sealed class TabulatorCompiler : CompilerBase, IExpressionOutput
		{
		#region Fields

		private readonly bool _oneArg;

		private readonly LocalBuilder _idxLocal;
		private readonly LocalBuilder _idxLocal2;
		private readonly LocalBuilder _resLocal;
		private readonly LocalBuilder _beginLocal;
		private readonly LocalBuilder _arrayLocal;
		
		private readonly Label _condLabel,  _beginLabel;
		private readonly Label _condLabel2, _beginLabel2;

		#endregion
		#region Members

		public TabulatorCompiler( bool oneArg, bool check )
			: base(	RetTypes(oneArg),
					ArgTypes(oneArg), check )
			{
			_oneArg = oneArg;

			_condLabel  = _body.DefineLabel( );
			_beginLabel = _body.DefineLabel( );

			_idxLocal = _body.DeclareLocal(_indexType);
			
			if(oneArg)
				{
				_resLocal = _body.DeclareLocal(_arrType);
				
				// res = new double[count];
				_body.Emit(OpCodes.Ldarg_1);
				_body.Emit(OpCodes.Newarr, _valType);
				_body.Emit(OpCodes.Stloc, _resLocal);

				// int i = 0;
				_body.Emit(OpCodes.Ldc_I4_0);
				_body.Emit(OpCodes.Stloc, _idxLocal);

				// jump to condition
				_body.Emit(OpCodes.Br, _condLabel);
				
				// res[i] = 
				_body.MarkLabel(_beginLabel);
				_body.Emit(OpCodes.Ldloc, _resLocal);
				_body.Emit(OpCodes.Ldloc, _idxLocal);
				}
			else
				{
				_resLocal   = _body.DeclareLocal(_arrArrType);
				_idxLocal2  = _body.DeclareLocal(_indexType);
				_beginLocal = _body.DeclareLocal(_valType);
				_arrayLocal = _body.DeclareLocal(_arrType);
				
				_condLabel2  = _body.DefineLabel();
				_beginLabel2 = _body.DefineLabel();
				
				// res = new double[count1][];
				_body.Emit(OpCodes.Ldarg_1);
				_body.Emit(OpCodes.Newarr, _arrType);
				_body.Emit(OpCodes.Stloc, _resLocal);
				
				// begin = y
				_body.Emit(OpCodes.Ldarg_2);
				_body.Emit(OpCodes.Stloc, _beginLocal);

				// int i = 0;
				_body.Emit(OpCodes.Ldc_I4_0);
				_body.Emit(OpCodes.Stloc, _idxLocal);

				// jump to condition
				_body.Emit(OpCodes.Br, _condLabel);

				// arr = new double[count2];
				_body.MarkLabel(_beginLabel);
				_body.Emit(OpCodes.Ldarg_3);
				_body.Emit(OpCodes.Newarr, _valType);
				_body.Emit(OpCodes.Stloc, _arrayLocal);

				// j = 0;
				_body.Emit(OpCodes.Ldc_I4_0);
				_body.Emit(OpCodes.Stloc, _idxLocal2);

				// jump to condition
				_body.Emit(OpCodes.Br, _condLabel2);

				// arr[j] =
				_body.MarkLabel(_beginLabel2);
				_body.Emit(OpCodes.Ldloc, _arrayLocal);
				_body.Emit(OpCodes.Ldloc, _idxLocal2);
				}
			}

		public Tabulator CreateTabulator( string expr )
			{
			_body.Emit(_saveElem);

			if(!_oneArg)
				{
				// y += step2;
				_body.Emit(OpCodes.Ldarg_2);
				_body.Emit(OpCodes.Ldarg_S, (byte)5);
				_body.Emit(OpCodes.Add);
				_body.Emit(OpCodes.Starg_S, (byte)2);

				// j++;
				_body.Emit(OpCodes.Ldloc, _idxLocal2);
				_body.Emit(OpCodes.Ldc_I4_1);
				_body.Emit(OpCodes.Add);
				_body.Emit(OpCodes.Stloc, _idxLocal2);

				// while(j < count2)
				_body.MarkLabel(_condLabel2);
				_body.Emit(OpCodes.Ldloc, _idxLocal2);
				_body.Emit(OpCodes.Ldarg_3);
				_body.Emit(OpCodes.Blt, _beginLabel2);

				// res[i] = arr;
				_body.Emit(OpCodes.Ldloc, _resLocal);
				_body.Emit(OpCodes.Ldloc, _idxLocal);
				_body.Emit(OpCodes.Ldloc, _arrayLocal);
				_body.Emit(OpCodes.Stelem_Ref);

				// y = begin
				_body.Emit(OpCodes.Ldloc, _beginLocal);
				_body.Emit(OpCodes.Starg_S, (byte)2);
				}

			// x += step
			_body.Emit(OpCodes.Ldarg_0);
			
			if(_oneArg)
				 _body.Emit(OpCodes.Ldarg_2);
			else _body.Emit(OpCodes.Ldarg_S, (byte)4);

			_body.Emit(OpCodes.Add);
			_body.Emit(OpCodes.Starg_S, (byte)0);
			
			// i++;
			_body.Emit(OpCodes.Ldloc, _idxLocal);
			_body.Emit(OpCodes.Ldc_I4_1);
			_body.Emit(OpCodes.Add);
			_body.Emit(OpCodes.Stloc, _idxLocal);

			// while(i < count)
			_body.MarkLabel(_condLabel);
			_body.Emit(OpCodes.Ldloc, _idxLocal);
			_body.Emit(OpCodes.Ldarg_1);
			_body.Emit(OpCodes.Blt, _beginLabel);

			// return res
			_body.Emit(OpCodes.Ldloc, _resLocal);
			_body.Emit(OpCodes.Ret);

			#if VISUALIZE
			DynamicMethodVisualizer.Visualizer.Show(_eval);
			#endif

			Delegate method = _eval.CreateDelegate(_oneArg? _tabType1: _tabType2);

			return new Tabulator(expr, method, _oneArg);
			}

		#endregion
		#region IExpressionOutput

		public void PutArgument( int id )
			{
			if(id == 0)
				 _body.Emit(OpCodes.Ldarg_0);
			else _body.Emit(OpCodes.Ldarg_2);
			}

		#endregion
		#region Static Data

		// Helpers ================================================

		public static Type RetTypes( bool oneArg )
			{
			return oneArg ? _arrType : _arrArrType;
			}

		public static Type[] ArgTypes( bool oneArg )
			{
			return oneArg ? _argsTypes1 : _argsTypes2;
			}

		// Types ==================================================

		private static readonly Type _indexType = typeof( int );

		private static readonly Type _arrArrType = typeof( double[][] );

		private static readonly Type _tabType1 = typeof( Tabulator.TabFunc1 );
		private static readonly Type _tabType2 = typeof( Tabulator.TabFunc2 );
		
		private static readonly Type[] _argsTypes1 = new[]
			{
				_valType,
				_indexType,
				_valType
			};

		private static readonly Type[] _argsTypes2 = new[]
			{
				_valType, _indexType,
				_valType, _indexType,
				_valType, _valType
			};

		#endregion
		}
	}