﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ILCalc
	{
	/// <summary>
	/// Manages the unique arguments names available to an expression.<br/>
	/// This class cannot be inherited.
	/// </summary>
	/// <remarks>
	/// When any of methods for evaluating expression calling, the arguments
	/// should be passed in the same order as their names presented in the list.
	/// </remarks>
	/// <threadsafety instance="false"/>
	
	[DebuggerDisplay( "Count = {Count}" )]
	[Serializable]
	public sealed class ArgumentCollection : IList<string>
		{
		#region Fields

		[DebuggerBrowsable( DebuggerBrowsableState.RootHidden )]
		private readonly List<string> _args;

		#endregion
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/>
		/// class that is empty and has the default initial capacity.
		/// </summary>
		/// <overloads>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/> class.
		/// </overloads>
		[DebuggerHidden]
		public ArgumentCollection( )
			{
			_args = new List<string>( );
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/>
		/// class that has one specific argument name inside.
		/// </summary>
		/// <param name="name">Argument name.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="name"/> is not valid identifier name.
		/// </exception>
		[DebuggerHidden]
		public ArgumentCollection( string name )
			{
			Validator.IdentifierName(name);

			_args = new List<string>(1) {name};
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/>
		/// class that has specified arguments names inside.
		/// </summary>
		/// <param name="names">Arguments names.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="names"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some item of <paramref name="names"/> is not valid identifier name.<br/>-or-<br/>
		/// Some item of <paramref name="names"/> is already exist in the list.
		/// </exception>
		public ArgumentCollection( params string[] names )
			{
			if( names == null )
				throw new ArgumentNullException("names");

			_args = new List<string>(names.Length);
			foreach( string name in names )
				Add(name);
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/>
		/// class that has specified arguments names inside.
		/// </summary>
		/// <param name="names">Enumerable with arguments names.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="names"/> is null.</exception>
		/// <exception cref="ArgumentException">
		/// Some item of <paramref name="names"/> is not valid identifier name.<br/>-or-<br/>
		/// Some item of <paramref name="names"/> is already exist in the list.
		/// </exception>
		public ArgumentCollection( IEnumerable<string> names )
			{
			if( names == null )
				throw new ArgumentNullException("names");

			_args = new List<string>( );
			foreach( string name in names )
				Add(name);
			}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArgumentCollection"/>
		/// class from the other <see cref="ArgumentCollection"/> instance.
		/// </summary>
		/// <param name="list">ArgumentCollection instance.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="list"/> is null.</exception>
		[DebuggerHidden]
		public ArgumentCollection( ArgumentCollection list )
			{
			if( list == null )
				throw new ArgumentNullException("list");

			_args = new List<string>(list._args);
			}

		#endregion
		#region IList<>

		/// <summary>
		/// Searches for the specified name and returns the zero-based
		/// index of name in the <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <param name="item">The name to locate in the <see cref="ArgumentCollection"/>.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of name within
		/// the entire <see cref="ArgumentCollection"/>, if found; otherwise, –1.
		/// </returns>
		[DebuggerHidden]
		public int IndexOf( string item )
			{
			return _args.IndexOf(item);
			}

		/// <summary>
		/// Inserts an element into the <see cref="ArgumentCollection"/>
		/// at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index at which name should be inserted.
		/// </param>
		/// <param name="item">The name to insert</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> is not valid identifier name.<br/>-or-<br/>
		/// Argument with <paramref name="item"/> name is already exist.
		/// </exception>
		public void Insert( int index, string item )
			{
			Validator.IdentifierName(item);

			if( _args.Contains(item) )
				{
				throw new ArgumentException(
					string.Format(Resources.errArgumentExist, item)
					);
				}

			_args.Insert(index, item);
			}

		/// <summary>
		/// Removes the name at the specified index of the <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the name to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0, equal to or greater than Count.
		/// </exception>
		[DebuggerHidden]
		public void RemoveAt( int index )
			{
			_args.RemoveAt(index);
			}

		/// <summary>
		/// Gets or sets the argument name at the specified index. 
		/// </summary>
		/// <param name="index">
		/// The zero-based index of the name to get or set.
		/// </param>
		/// <returns>The name at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than 0, equal to
		/// or greater than <see cref="Count"/> value.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> is not valid identifier name<br/>-or-<br/>
		/// <paramref name="value"/> name is already exist in the list.
		/// </exception>
		public string this[ int index ]
			{
			[DebuggerHidden]
			get { return _args[index]; }
			set
				{
				if( _args[index] == value ) return;

				Validator.IdentifierName(value);

				if( _args.Contains(value) )
					{
					throw new ArgumentException(
						string.Format(Resources.errArgumentExist, value)
						);
					}

				_args[index] = value;
				}
			}

		#endregion
		#region ICollection<>

		/// <summary>
		/// Adds name to the end of the <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <param name="item">Argument name to add.</param>
		/// <exception cref="ArgumentException">
		/// <paramref name="item"/> is not valid identifier name.<br/>-or-<br/>
		/// <paramref name="item"/> name is already exist in the list.
		/// </exception>
		public void Add( string item )
			{
			Validator.IdentifierName(item);

			if( _args.Contains(item) )
				{
				throw new ArgumentException(
					string.Format(Resources.errArgumentExist, item)
					);
				}

			_args.Add(item);
			}

		/// <summary>
		/// Removes all names from the <see cref="ArgumentCollection"/>.
		/// </summary>
		[DebuggerHidden]
		public void Clear( )
			{
			_args.Clear( );
			}

		/// <summary>
		/// Determines whether an name is in the <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <param name="item">
		/// Argument name to locate in <see cref="ArgumentCollection"/>.
		/// </param>
		/// <returns>
		/// <b>true</b> if name is found in the list; otherwise, <b>false</b>.
		/// </returns>
		[DebuggerHidden]
		public bool Contains( string item )
			{
			return _args.Contains(item);
			}

		/// <summary>
		/// Copies the entire list of arguments names to a one-dimensional array
		/// of strings, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="Array"/> of strings that is the
		/// destination of the names copied from <see cref="ArgumentCollection"/>.
		/// The <see cref="Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		/// The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="array"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="arrayIndex"/> is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="arrayIndex"/> is equal to or greater
		/// than the length of array.<br/>-or-<br/>
		/// Number of names in the source <see cref="ArgumentCollection"/>
		/// is greater than the available space from <paramref name="arrayIndex"/>
		/// to the end of the destination <paramref name="array"/>.
		/// </exception>
		[DebuggerHidden]
		public void CopyTo( string[] array, int arrayIndex )
			{
			_args.CopyTo(array, arrayIndex);
			}

		/// <summary>
		/// Gets the number of names actually contained in the <see cref="ArgumentCollection"/>.
		/// </summary>
		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public int Count
			{
			[DebuggerHidden]
			get { return _args.Count; }
			}

		/// <summary>
		/// Gets a value indicating whether the 
		/// <see cref="ICollection{T}"/> is read-only.
		/// </summary>
		/// <value>Always <b>false</b>.</value>
		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public bool IsReadOnly
			{
			[DebuggerHidden]
			get { return false; }
			}

		/// <summary>
		/// Removes the specific name from the <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <param name="item">The name to be removed.</param>
		/// <returns>
		/// <b>true</b> if name is successfully removed; otherwise, <b>false</b>.
		/// </returns>
		[DebuggerHidden]
		public bool Remove( string item )
			{
			return _args.Remove(item);
			}

		#endregion
		#region IEnumerable<>

		/// <summary>
		/// Returns an enumerator that iterates through
		/// the names in <see cref="ArgumentCollection"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="List{T}.Enumerator"/> for the names
		/// in <see cref="ArgumentCollection"/>.
		/// </returns>
		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator( )
			{
			foreach( string name in _args )
				yield return name;

			yield break;
			}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator( )
			{
			return _args.GetEnumerator( );
			}

		#endregion
		}
	}