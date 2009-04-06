using System;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ILCalc
	{
	//NOTE: move in public scope?

	using State = DebuggerBrowsableState;

	[Serializable]
	sealed class MethodGroup
		{
		#region Item Class

		[DebuggerDisplay( "{ArgCount} args", Name = "{Func}" )]
		[Serializable]
		public sealed class Item
			{
			#region Fields

			[DebuggerBrowsable( State.Never )]
			private readonly MethodInfo _method;
			[DebuggerBrowsable( State.Never )]
			private readonly bool _isParams;
			[DebuggerBrowsable( State.Never )]
			private readonly int _argCount;

			#endregion
			#region Properties

			[DebuggerBrowsable( State.Never )]
			public MethodInfo Func
				{
				[DebuggerHidden]
				get { return _method; }
				}

			[DebuggerBrowsable( State.Never )]
			public bool IsParams
				{
				[DebuggerHidden]
				get { return _isParams; }
				}

			[DebuggerBrowsable( State.Never )]
			public int ArgCount
				{
				[DebuggerHidden]
				get { return _argCount; }
				}

			#endregion
			#region Constructor

			public Item( MethodInfo func )
				{
				var args = func.GetParameters( );

				_argCount = args.Length;
				_method = func;

				_isParams = (_argCount != 0)
					 && args[_argCount - 1].ParameterType.IsArray;

				if( _isParams ) _argCount--;
				}

			#endregion
			}

		#endregion
		#region Fields

		[DebuggerBrowsable( State.RootHidden )]
		private readonly List<Item> _list;

		[DebuggerBrowsable( State.Never )]
		private bool _params;

		#endregion
		#region Properties

		[DebuggerBrowsable( State.Never )]
		public int Count
			{
			[DebuggerHidden]
			get { return _list.Count; }
			}

		[DebuggerBrowsable( State.Never )]
		public bool HasParams
			{
			[DebuggerHidden]
			get { return _params; }
			}

		#endregion
		#region Methods

		public bool Append( MethodInfo func )
			{
			var item = new Item(func);
			if( item.IsParams ) _params = true;

			foreach( Item method in _list )
				{
				if( method.ArgCount == item.ArgCount &&
				    method.IsParams == item.IsParams )
					return false;
				}

			_list.Add(item);
			return true;
			}

		public bool Remove( int argsCount, bool isParams )
			{
			int i = 0;
			foreach( Item func in _list )
				{
				if( func.ArgCount == argsCount &&
				    func.IsParams == isParams )
					{
					_list.RemoveAt(i);

					_params = false;
					foreach( Item info in _list )
						{
						if( info.IsParams )
							{
							_params = true;
							break;
							}
						}

					return true;
					}
				i++;
				}
			return false;
			}

		// TODO: don't like it

		public Item GetStdFunc( int argsCount )
			{
			foreach( Item item in _list )
				{
				if( item.ArgCount == argsCount )
					return item;
				}

			return null;
			}

		public Item GetParamsFunc( int argsCount )
			{
			int fixCount = -1;
			Item func = null;

			foreach( Item item in _list )
				{
				if( item.IsParams )
					{
					if( item.ArgCount <= argsCount &&
					    item.ArgCount > fixCount )
						{
						func = item;
						fixCount = item.ArgCount;
						}
					}
				else if( item.ArgCount == argsCount )
					return item;
				}

			return func;
			}

		public Item GetFunc( int argsCount )
			{
			return _params?
				GetParamsFunc(argsCount):
				GetStdFunc(argsCount);
			}

		public string OverloadsList( )
			{
			var buf = new StringBuilder( );
			bool first = true;

			foreach( Item method in _list )
				{
				if( first ) first = false;
				else buf.Append(", ");

				buf.Append(method.ArgCount);

				if( method.IsParams ) buf.Append('+');
				}

			return buf.ToString( );
			}

		#endregion
		#region Constructors

		public MethodGroup( MethodInfo func )
			{
			var info = new Item(func);

			_list = new List<Item> {info};
			_params = info.IsParams;
			}

		public MethodGroup( MethodGroup list )
			{
			_list = new List<Item>(list._list);
			_params = list.HasParams;
			}

		#endregion
		}
	}