﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class SimplePool<T> where T : class
{
	// Objects stored in the pool
	private List<T> pool = null;

	// A function that can be used to override default NewObject( T ) function
	public Func<T> CreateFunction;

	// Actions that can be used to implement extra logic on pushed/popped objects
	public Action<T> OnPush, OnPop;

	public SimplePool( int initialSize, Func<T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null )
	{
		pool = new List<T>( initialSize );

		this.CreateFunction = CreateFunction;
		this.OnPush = OnPush;
		this.OnPop = OnPop;

		Populate( initialSize );
	}

	// Populate the pool with new items
	public void Populate( int capacity )
	{
		int count = capacity - pool.Count;
		if( count > 0 )
		{
			// Create a single object first to see if everything works fine
			// If not, don't continue
			T obj = CreateFunction();
			if( obj == null )
				return;

			Push( obj );

			// Everything works fine, populate the pool with the remaining items
			for( int i = 1; i < count; i++ )
				Push( CreateFunction() );
		}
	}

	// Fetch an item from the pool
	public T Pop()
	{
		T objToPop;

		if( pool.Count == 0 )
		{
			// Pool is empty, instantiate an object
			objToPop = CreateFunction();
		}
		else
		{
			// Pool is not empty, fetch a random item from the pool
			int index = pool.Count - 1;
			objToPop = pool[index];
			pool.RemoveAtFast( index );

			while( objToPop == null || objToPop.Equals( null ) )
			{
				// Some objects in the pool might have been destroyed (maybe during a scene transition),
				// consider that case
				if( pool.Count > 0 )
				{
					objToPop = pool[--index];
					pool.RemoveAtFast( index );
				}
				else
				{
					objToPop = CreateFunction();
					break;
				}
			}
		}

		if( OnPop != null )
			OnPop( objToPop );

		return objToPop;
	}

	// Pool an item
	public void Push( T obj )
	{
		if( obj == null ) return;

		if( OnPush != null )
			OnPush( obj );

		if( pool.Count == 0 )
			pool.Add( obj );
		else
		{
			// Add object to a random index
			int randomIndex = Random.Range( 0, pool.Count );
			pool.Add( pool[randomIndex] );
			pool[randomIndex] = obj;
		}
	}

	// Moves all the GameObjects in the pool to DontDestroyOnLoad scene
	public void DontDestroyOnLoad()
	{
		Scene dontDestroyOnLoadScene = FTemplate.Instance.gameObject.scene;

		bool isTypeComponent = typeof( Component ).IsAssignableFrom( typeof( T ) );
		if( !isTypeComponent && !typeof( GameObject ).IsAssignableFrom( typeof( T ) ) )
			return;

		for( int i = pool.Count - 1; i >= 0; i-- )
		{
			if( isTypeComponent )
			{
				Component component = pool[i] as Component;
				if( !component )
					pool.RemoveAtFast( i );
				else if( component.gameObject.scene != dontDestroyOnLoadScene )
				{
					if( component.transform.parent )
						component.transform.SetParent( null );

					GameObject.DontDestroyOnLoad( component.gameObject );
				}
			}
			else
			{
				GameObject go = pool[i] as GameObject;
				if( !go )
					pool.RemoveAtFast( i );
				else if( go.scene != dontDestroyOnLoadScene )
				{
					if( go.transform.parent )
						go.transform.SetParent( null );

					GameObject.DontDestroyOnLoad( go );
				}
			}
		}
	}
}