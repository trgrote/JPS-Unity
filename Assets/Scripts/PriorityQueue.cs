using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue< KeyType, PriorityType > where PriorityType : System.IComparable
{
	struct Element< SubClassKeyType, SubClassPriorityType > where SubClassPriorityType : System.IComparable
	{
		public SubClassKeyType key;
		public SubClassPriorityType priority;

		public Element( SubClassKeyType key, SubClassPriorityType priority )
		{
			this.key = key;
			this.priority = priority;
		}
	}

	List< Element<KeyType, PriorityType> > queue = new List< Element<KeyType, PriorityType> >();

	public void push( KeyType arg_key, PriorityType arg_priority )
	{
		Element<KeyType, PriorityType> new_elem = new Element<KeyType, PriorityType>( arg_key, arg_priority );

		int index = 0;
		foreach ( var element in queue )
		{
			// if my new element's priority is less than than the element in this location
			if ( new_elem.priority.CompareTo( element.priority ) < 0 )
			{
				break;
			}

			++index;
		}

		// Insert at the found index
		queue.Insert( index, new_elem );
	}

	public KeyType pop()
	{
		if ( isEmpty() )
		{
			throw new UnityException("Attempted to pop off an empty queue");
		}

		Element<KeyType, PriorityType> top = queue[ 0 ];

		queue.RemoveAt( 0 );

		return top.key;
	}

	public bool isEmpty()
	{
		return queue.Count == 0;
	}
}
