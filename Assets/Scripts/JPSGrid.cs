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
	public bool[] jumpPointDirection = new bool[8];  // God, I hope these all default to false
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

	private bool isEmpty( int index )
	{
		if ( index < 0 ) return false;

		int row, column;
		row    = index / rowSize;
		column = index % rowSize;

		return isEmpty( row, column );
	}

	private bool isObstacleOrWall( int index )
	{
		if ( index < 0 ) return true;

		int row, column;
		row    = index / rowSize;
		column = index % rowSize;

		return isObstacleOrWall( row, column );
	}

	private bool isEmpty( int row, int column )
	{
		//if ( row >= 0 && row < rowsize && column >= 0 && column < maxRows )
		//{
		//	return ! gridNodes[ column + ( row * rowsize ) ].isObstacle;
		//}

		//return false;

		return ! isObstacleOrWall( row, column );
	}

	private bool isObstacleOrWall( int row, int column )
	{
		if ( isInBounds( row, column ) )
		{
			return gridNodes[ column + ( row * rowSize ) ].isObstacle;
		}

		return true;  // If we are out of bounds, then we are def a wall
	}

	private bool isInBounds( int index )
	{
		if ( index < 0 || index >= gridNodes.Length ) return false;

		int row, column;
		row    = index / rowSize;
		column = index % rowSize;

		return isInBounds( row, column );
	}

	private bool isInBounds( int row, int column )
	{
		return row >= 0 && row < maxRows && column >= 0 && column < rowSize;
	}

	// Returns Grid Index of node in the given direction
	// Returns -1 if index is out of bounds
	private int getIndexOfNodeTowardsDirection( int index, eDirections direction )
	{
		int row, column;
		row    = index / rowSize;
		column = index % rowSize;

		int change_row    = 0;
		int change_column = 0;

		// Change in the Row Direction
		switch ( direction )
		{
			case eDirections.DIR_NORTH_EAST:
			case eDirections.DIR_NORTH:
			case eDirections.DIR_NORTH_WEST:
				change_row = -1;
				break;

			case eDirections.DIR_SOUTH_EAST:
			case eDirections.DIR_SOUTH:
			case eDirections.DIR_SOUTH_WEST:
				change_row = 1;
				break;

			default:
				break;
		}

		// Change in the Column Direction
		switch ( direction )
		{
			case eDirections.DIR_NORTH_EAST:
			case eDirections.DIR_EAST:
			case eDirections.DIR_SOUTH_EAST:
				change_column = 1;
				break;

			case eDirections.DIR_SOUTH_WEST:
			case eDirections.DIR_WEST:
			case eDirections.DIR_NORTH_WEST:
				change_column = -1;
				break;

			default:
				break;
		}

		// Calc new rows and columns
		int new_row    = row    + change_row;
		int new_column = column + change_column;

		// Check bounds
		if ( isInBounds( new_row, new_column ) )
		{
			return new_column + ( new_row * rowSize );
		}

		return -1;    // Out of bounds is -1
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
						// Is the Southern Node an Obstacle? if not, then this north East node will be a jump point from the south
						// Is the Wester Node an Obstacle? If not, then this NE node will be a jump from the west
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.DIR_SOUTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.DIR_WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.DIR_SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.DIR_WEST  ] = true;
						}
					}
				}

				// South East
				south_east_node = getSouthEastIndex( row, column );

				if ( south_east_node != -1  )
				{
					Node node = gridNodes[ south_east_node ];

					if ( ! node.isObstacle )
					{
						// Is the Southern Node an Obstacle? if not, then this north East node will be a jump point from the south
						// Is the Wester Node an Obstacle? If not, then this NE node will be a jump from the west
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.DIR_NORTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.DIR_WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.DIR_NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.DIR_WEST  ] = true;
						}
					}
				}

				// South West
				south_west_node = getSouthWestIndex( row, column );

				if ( south_west_node != -1  )
				{
					Node node = gridNodes[ south_west_node ];

					if ( ! node.isObstacle )
					{
						// Is the Southern Node an Obstacle? if not, then this north East node will be a jump point from the south
						// Is the Wester Node an Obstacle? If not, then this NE node will be a jump from the west
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.DIR_NORTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.DIR_EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.DIR_NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.DIR_EAST  ] = true;
						}
					}
				}

				// North West
				north_west_node = getNorthWestIndex( row, column );

				if ( north_west_node != -1  )
				{
					Node node = gridNodes[ north_west_node ];

					if ( ! node.isObstacle )
					{
						// Is the Southern Node an Obstacle? if not, then this north East node will be a jump point from the south
						// Is the Wester Node an Obstacle? If not, then this NE node will be a jump from the west
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.DIR_SOUTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.DIR_EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.DIR_SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.DIR_EAST  ] = true;
						}
					}
				}

			}
		}
	}
}
