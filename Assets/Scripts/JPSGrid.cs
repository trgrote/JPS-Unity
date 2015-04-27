using UnityEngine;
using System.Collections;

public enum eDirections
{
	DIR_NORTH      = 0,
	DIR_NORTH_EAST = 1,
	DIR_EAST       = 2,
	DIR_SOUTH_EAST = 3,
	DIR_SOUTH      = 4,
	DIR_SOUTH_WEST = 5,
	DIR_WEST       = 6,
	DIR_NORTH_WEST = 7,
}

public class Node
{
	public bool isObstacle = false;
	public int[] jpDistances = new int[8];

	public bool isJumpPoint = false;
	public bool[] jumpPointDirection = new bool[8];
}

public class Grid
{
	public Node[] gridNodes = new Node[0];

	private int maxRows 
	{ 
		get 
		{
			return gridNodes.Length / rowSize;
		} 
	}

	public int rowSize = 0;

	// Get index of north east value, or -1 is one doesn't exist
	private int getNorthEastIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column + 1 >= rowSize || row - 1 < 0 ) return -1;

		return ( column + 1 ) +
			( row - 1 ) * rowSize;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getSouthEastIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column + 1 >= rowSize || row + 1 >= maxRows ) return -1;

		return ( column + 1 ) +
			( row + 1 ) * rowSize;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getSouthWestIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column - 1 < 0 || row + 1 >= maxRows ) return -1;

		return ( column - 1 ) +
			( row + 1 ) * rowSize;
	}

	// Get index of north east value, or -1 is one doesn't exist
	private int getNorthWestIndex( int row, int column )
	{
		// Skip positions that are on the edges
		if ( column - 1 < 0 || row - 1 < 0 ) return -1;

		return ( column - 1 ) +
			( row - 1 ) * rowSize;
	}

	public void buildPrimaryJumpPoints()
	{
		// foreach obstacle
		for ( int i = 0 ; i < gridNodes.Length ; ++i )
		{
			Node current_node = gridNodes[ i ];

			// find forced neighbor scenarios
			if ( current_node.isObstacle )
			{
				int row, column;
				row    = i / rowSize;
				column = i % rowSize;

				// Check Diagonal Nodes to see if they are also obstacles
				int north_east_index, south_east_node, south_west_node, north_west_node;

				// North East
				north_east_index = getNorthEastIndex( row, column );

				if ( north_east_index != -1 )
				{
					Node node = gridNodes[ north_east_index ];

					if ( ! node.isObstacle )
					{
						node.isJumpPoint = true;		// test
					}
				}

				// South East
				south_east_node = getSouthEastIndex( row, column );

				if ( south_east_node != -1  )
				{
					Node node = gridNodes[ south_east_node ];

					if ( ! node.isObstacle )
					{
						node.isJumpPoint = true;		// test
					}
				}

				// South West
				south_west_node = getSouthWestIndex( row, column );

				if ( south_west_node != -1  )
				{
					Node node = gridNodes[ south_west_node ];

					if ( ! node.isObstacle )
					{
						node.isJumpPoint = true;		// test
					}
				}

				// North West
				north_west_node = getNorthWestIndex( row, column );

				if ( north_west_node != -1  )
				{
					Node node = gridNodes[ north_west_node ];

					if ( ! node.isObstacle )
					{
						node.isJumpPoint = true;		// test
					}
				}

			}
		}
	}
}
