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
	// Holds if primary jump point has direction COMING FROM the Cardianal direction,
	// so jumpPointDirection[ DIR_EAST ] means it's a jump point for paths COMING FROM THE EAST 
	// Note: This would be "Moving Left" in Steve Rabin's implementation
	public bool[] jumpPointDirection = new bool[8];

	public bool isJumpPointComingFrom( eDirections dir )
	{
		return this.isJumpPoint && this.jumpPointDirection[ (int) dir ];
	}
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

	private int rowColumnToIndex ( int row, int column )
	{
		return column + ( row * rowSize );
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

	private bool isJumpPoint( int row, int column, eDirections dir )
	{
		if ( isInBounds( row, column ) )
		{
			Node node = gridNodes[ column + ( row * rowSize ) ];
			return node.isJumpPoint && node.jumpPointDirection[ (int) dir ];
		}

		return false;  // If we are out of bounds, then we are def a wall
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

	public void buildStraightJumpPoints()
	{
		// Calcin' Jump Distance, left and right
		// For all the rows in the grid
		for ( int row = 0 ; row < maxRows ; ++row )
		{
			// Calc moving left to right
			int  jumpDistanceSoFar = -1;
			bool jumpPointSeen = false;

			// Checking for jump disances where nodes are moving WEST
			for ( int column = 0 ; column < rowSize ; ++column )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
					node.jpDistances[ (int) eDirections.DIR_WEST ] = jumpDistanceSoFar;
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.DIR_EAST ) )
				{
					//Debug.Log("FOUND JUMP POINT COMING FROM EAST: " + row + ", " + column );
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}

			jumpDistanceSoFar = -1;
			jumpPointSeen = false;
			// Checking for jump disances where nodes are moving WEST
			for ( int column = rowSize - 1 ; column >= 0 ; --column )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his RIGTH ( EAST )
					node.jpDistances[ (int) eDirections.DIR_EAST ] = jumpDistanceSoFar;
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.DIR_WEST ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}
		}

		// Calcin' Jump Distance, up and down
		// For all the columns in the grid
		for ( int column = 0 ; column < rowSize ; ++column )
		{
			// Calc moving left to right
			int  jumpDistanceSoFar = -1;
			bool jumpPointSeen = false;

			// Checking for jump disances where nodes are moving NORTH
			for ( int row = 0 ; row < maxRows ; ++row )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading UP, then we can tell this node he's got a jump point coming up ABOVE ( NORTH )
					node.jpDistances[ (int) eDirections.DIR_NORTH ] = jumpDistanceSoFar;
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.DIR_SOUTH ) )
				{
					//Debug.Log("FOUND JUMP POINT COMING FROM EAST: " + row + ", " + column );
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}

			jumpDistanceSoFar = -1;
			jumpPointSeen = false;
			// Checking for jump disances where nodes are moving SOUTH
			for ( int row = maxRows - 1 ; row >= 0 ; --row )
			{
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];

				// If we've reach a wall, then reset everything :(
				if ( node.isObstacle )
				{
					jumpDistanceSoFar = -1;
					jumpPointSeen = false;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading down, then we can tell this node he's got a jump point coming up BELOW( SOUTH )
					node.jpDistances[ (int) eDirections.DIR_SOUTH ] = jumpDistanceSoFar;
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.DIR_NORTH ) )
				{
					jumpDistanceSoFar = 0;
					jumpPointSeen = true;
				}
			}
		}
	}

	private Node getNode( int row, int column )
	{
		Node node = null;

		if ( isInBounds( row, column ) )
		{
			node = gridNodes[ rowColumnToIndex( row, column ) ];
		}

		return node;
	}

	public void buildDiagonalJumpPoints()
	{
		// Calcin' Jump Distance, Diagonally Upleft and upright
		// For all the rows in the grid
		for ( int row = 0 ; row < maxRows ; ++row )
		{
			// foreach column
			for ( int column = 0 ; column < rowSize ; ++column )
			{
				// if this node is an obstacle, then skip
				if ( isObstacleOrWall( row, column ) ) continue;
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];    // Grab the node ( will not be NULL! )

				// Calculate NORTH WEST DISTNACES
				if ( row  == 0 || column == 0 || (                  // If we in the top left corner
					isObstacleOrWall( row - 1, column ) ||          // If the node above is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the left is an obstacle
					isObstacleOrWall( row - 1, column - 1 ) ) )     // if the node to the top left (North West) is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                 // if the node to the north is empty
					isEmpty(row, column - 1) &&                                                       // if the node to the west is empty
					(getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.DIR_NORTH ] > 0 || // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.DIR_WEST  ] > 0))  // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.DIR_NORTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate NORTH EAST DISTNACES
				if ( row  == 0 || column == rowSize -1 || (         // If we in the top right corner
					isObstacleOrWall( row - 1, column ) ||          // If the node above is an obstacle
					isObstacleOrWall( row, column + 1) ||           // If the node to the right is an obstacle
					isObstacleOrWall( row - 1, column + 1 ) ) )     // if the node to the top left (North East) is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                 // if the node to the north is empty
					isEmpty(row, column + 1) &&                                                       // if the node to the west is empty
					(getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.DIR_NORTH ] > 0 || // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.DIR_EAST  ] > 0))  // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.DIR_NORTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] = -1 + jumpDistance;
					}
				}
			}
		}

		// Calcin' Jump Distance, Diagonally DownLeft and Downright
		// For all the rows in the grid
		for ( int row = maxRows - 1 ; row >= 0 ; --row )
		{
			// foreach column
			for ( int column = 0 ; column < rowSize ; ++column )
			{
				// if this node is an obstacle, then skip
				if ( isObstacleOrWall( row, column ) ) continue;
				Node node = gridNodes[ rowColumnToIndex( row, column ) ];    // Grab the node ( will not be NULL! )

				// Calculate SOUTH WEST DISTNACES
				if ( row == maxRows - 1 || column == 0 || (                  // If we in the top left corner
					isObstacleOrWall( row + 1, column ) ||          // If the node above is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the left is an obstacle
					isObstacleOrWall( row + 1, column - 1 ) ) )     // if the node to the top left (North West) is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                 // if the node to the north is empty
					isEmpty(row, column - 1) &&                                                       // if the node to the west is empty
					(getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.DIR_SOUTH ] > 0 || // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.DIR_WEST  ] > 0))  // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.DIR_SOUTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate SOUTH EAST DISTNACES
				if ( row  == maxRows - 1 || column == rowSize -1 || (         // If we in the top right corner
					isObstacleOrWall( row + 1, column ) ||          // If the node above is an obstacle
					isObstacleOrWall( row, column + 1) ||           // If the node to the right is an obstacle
					isObstacleOrWall( row + 1, column + 1 ) ) )     // if the node to the top left (North East) is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                 // if the node to the north is empty
					isEmpty(row, column + 1) &&                                                       // if the node to the west is empty
					(getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.DIR_SOUTH ] > 0 || // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.DIR_EAST  ] > 0))  // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.DIR_SOUTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] = -1 + jumpDistance;
					}
				}
			}
		}
	}
}
