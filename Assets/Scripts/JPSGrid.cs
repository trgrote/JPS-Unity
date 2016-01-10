using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public enum eDirections
{
	NORTH      = 0,
	NORTH_EAST = 1,
	EAST       = 2,
	SOUTH_EAST = 3,
	SOUTH      = 4,
	SOUTH_WEST = 5,
	WEST       = 6,
	NORTH_WEST = 7,
}

public class Node
{
	public Point pos;
	public bool isObstacle = false;
	public int[] jpDistances = new int[8];

	public bool isJumpPoint = false;
	// Holds if primary jump point has direction COMING FROM the Cardianal direction,
	// so jumpPointDirection[ EAST ] means it's a jump point for paths COMING FROM THE EAST 
	// Note: This would be "Moving Left" in Steve Rabin's implementation
	public bool[] jumpPointDirection = new bool[8];

	public bool isJumpPointComingFrom( eDirections dir )
	{
		return this.isJumpPoint && this.jumpPointDirection[ (int) dir ];
	}
}

public class PathfindReturn
{
	public enum PathfindStatus
	{
		SEARCHING,
		FOUND,
		NOT_FOUND
	}

	public PathfindingNode _current;
	public PathfindStatus _status = PathfindStatus.SEARCHING;
	public List< Point > path = new List< Point >();
}

public struct Point : IEquatable< Point >
{
	public int column, row;

	public Point( int row, int column )
	{
		this.row = row;
		this.column = column;
	}

	public bool Equals( Point other )
	{
		return this.column == other.column && this.row == other.row;
	}

	// Get Difference between two points, assuming only cardianal or diagonal movement is possible
	public static int diff( Point a, Point b )
	{
		// because diagonal
		// 0,0 diff 1,1 = 1 
		// 0,0 diff 0,1 = 1 
		// 0,0 diff 1,2 = 2 
		// 0,0 diff 2,2 = 2 
		// return max of the diff row or diff column
		int diff_columns = Mathf.Abs( b.column - a.column );
		int diff_rows    = Mathf.Abs( b.row - a.row );

		return Mathf.Max( diff_rows, diff_columns );
	}

	public override string ToString()
	{
		return "(" + this.column + "," + this.row + ")";
	}
}

public enum ListStatus
{
	ON_NONE,
	ON_OPEN,
	ON_CLOSED
}

public class PathfindingNode
{
	public PathfindingNode parent;
	public Point pos;
	public int givenCost;
	public int finalCost;
	public eDirections directionFromParent;
	public ListStatus listStatus = ListStatus.ON_NONE;

	public void Reset()
	{
		this.parent = null;
		this.givenCost = 0;
		this.finalCost = 0;
		this.listStatus = ListStatus.ON_NONE;
	}
}

public class Grid
{
	public Node[] gridNodes = new Node[0];
	public PathfindingNode[] pathfindingNodes = new PathfindingNode[0];

	private Dictionary< eDirections, eDirections[] > validDirLookUpTable = new Dictionary< eDirections, eDirections[] >
	{
		{ eDirections.SOUTH,      new []{ eDirections.WEST,  eDirections.SOUTH_WEST, eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST } },
		{ eDirections.SOUTH_EAST, new []{ eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST } },
		{ eDirections.EAST,       new []{ eDirections.SOUTH, eDirections.SOUTH_EAST, eDirections.EAST, eDirections.NORTH_EAST, eDirections.NORTH } },
		{ eDirections.NORTH_EAST, new []{ eDirections.EAST,  eDirections.NORTH_EAST, eDirections.NORTH } },
		{ eDirections.NORTH,      new []{ eDirections.EAST,  eDirections.NORTH_EAST, eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST } },
		{ eDirections.NORTH_WEST, new []{ eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST } },
		{ eDirections.WEST,       new []{ eDirections.NORTH, eDirections.NORTH_WEST, eDirections.WEST, eDirections.SOUTH_WEST, eDirections.SOUTH } },
		{ eDirections.SOUTH_WEST, new []{ eDirections.WEST,  eDirections.SOUTH_WEST, eDirections.SOUTH } }
	};

	private eDirections[] allDirections = Enum.GetValues(typeof(eDirections)).Cast<eDirections>().ToArray();

	private int maxRows 
	{
		get 
		{
			return gridNodes.Length / rowSize;
		}
	}

	static string dirToStr( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.NORTH:
				return "NORTH";
			case eDirections.NORTH_EAST:
				return "NORTH_EAST";
			case eDirections.EAST:
				return "EAST";
			case eDirections.SOUTH_EAST:
				return "SOUTH_EAST";
			case eDirections.SOUTH:
				return "SOUTH";
			case eDirections.SOUTH_WEST:
				return "SOUTH_WEST";
			case eDirections.WEST:
				return "WEST";
			case eDirections.NORTH_WEST:
				return "NORTH_WEST";
		}

		return "NONE";
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

	private int pointToIndex( Point pos )
	{
		return rowColumnToIndex( pos.row, pos.column );
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
		return ! isObstacleOrWall( row, column );
	}

	private bool isObstacleOrWall( int row, int column )
	{
		// If we are out of bounds, then we are def a wall
		return isInBounds( row, column ) && gridNodes[ column + ( row * rowSize ) ].isObstacle;  
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
			case eDirections.NORTH_EAST:
			case eDirections.NORTH:
			case eDirections.NORTH_WEST:
				change_row = -1;
				break;

			case eDirections.SOUTH_EAST:
			case eDirections.SOUTH:
			case eDirections.SOUTH_WEST:
				change_row = 1;
				break;
		}

		// Change in the Column Direction
		switch ( direction )
		{
			case eDirections.NORTH_EAST:
			case eDirections.EAST:
			case eDirections.SOUTH_EAST:
				change_column = 1;
				break;

			case eDirections.SOUTH_WEST:
			case eDirections.WEST:
			case eDirections.NORTH_WEST:
				change_column = -1;
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
						// If nodes to the south and west are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.SOUTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( north_east_index, eDirections.WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.WEST  ] = true;
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
						// If nodes to the north and west are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.NORTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( south_east_node, eDirections.WEST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.WEST  ] = true;
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
						// If nodes to the north and East are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.NORTH ) ) && isEmpty( getIndexOfNodeTowardsDirection( south_west_node, eDirections.EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.NORTH ] = true;
							node.jumpPointDirection[ (int) eDirections.EAST  ] = true;
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
						// If nodes to the south and East are empty, then this node will be a jump point for those directions
						if ( isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.SOUTH ) ) 
							&& isEmpty( getIndexOfNodeTowardsDirection( north_west_node, eDirections.EAST ) ) )
						{
							node.isJumpPoint = true;
							node.jumpPointDirection[ (int) eDirections.SOUTH ] = true;
							node.jumpPointDirection[ (int) eDirections.EAST  ] = true;
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
					node.jpDistances[ (int) eDirections.WEST ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his LEFT ( WEST )
					node.jpDistances[ (int) eDirections.WEST ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.WEST ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.EAST ) )
				{
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
					node.jpDistances[ (int) eDirections.EAST ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading left, then we can tell this node he's got a jump point coming up to his RIGTH ( EAST )
					node.jpDistances[ (int) eDirections.EAST ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.EAST ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.WEST ) )
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
					node.jpDistances[ (int) eDirections.NORTH ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading UP, then we can tell this node he's got a jump point coming up ABOVE ( NORTH )
					node.jpDistances[ (int) eDirections.NORTH ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.NORTH ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.SOUTH ) )
				{
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
					node.jpDistances[ (int) eDirections.SOUTH ] = 0;
					continue;
				}

				++jumpDistanceSoFar;

				if ( jumpPointSeen )
				{
					// If we've seen a jump point heading down, then we can tell this node he's got a jump point coming up BELOW( SOUTH )
					node.jpDistances[ (int) eDirections.SOUTH ] = jumpDistanceSoFar;
				}
				else
				{
					node.jpDistances[ (int) eDirections.SOUTH ] = -jumpDistanceSoFar;   // Set wall distance
				}

				// If we just found a new jump point, then set everything up for this new jump point
				if ( node.isJumpPointComingFrom( eDirections.NORTH ) )
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
				if ( row  == 0 || column == 0 || (                  // If we in the north west corner
					isObstacleOrWall( row - 1, column ) ||          // If the node to the north is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the left is an obstacle
					isObstacleOrWall( row - 1, column - 1 ) ) )     // if the node to the North west is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.NORTH_WEST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                    // if the node to the north is empty
					isEmpty(row, column - 1) &&                                                          // if the node to the west is empty
					(getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.NORTH ] > 0 ||    // If the node to the north west has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.WEST  ] > 0))     // If the node to the north west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.NORTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column - 1 ).jpDistances[ (int) eDirections.NORTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.NORTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.NORTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate NORTH EAST DISTNACES
				if ( row  == 0 || column == rowSize -1 || (         // If we in the top right corner
					isObstacleOrWall( row - 1, column ) ||          // If the node to the north is an obstacle
					isObstacleOrWall( row, column + 1) ||           // If the node to the east is an obstacle
					isObstacleOrWall( row - 1, column + 1 ) ) )     // if the node to the North East is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.NORTH_EAST ] = 0;
				}
				else if ( isEmpty(row - 1, column) &&                                                    // if the node to the north is empty
					isEmpty(row, column + 1) &&                                                          // if the node to the east is empty
					(getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.NORTH ] > 0 ||    // If the node to the north east has is a straight jump point ( or primary jump point) going north
					 getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.EAST  ] > 0))     // If the node to the north east has is a straight jump point ( or primary jump point) going east
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.NORTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row - 1, column + 1 ).jpDistances[ (int) eDirections.NORTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.NORTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.NORTH_EAST ] = -1 + jumpDistance;
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
				if ( row == maxRows - 1 || column == 0 || (         // If we in the south west most node
					isObstacleOrWall( row + 1, column ) ||          // If the node to the south is an obstacle
					isObstacleOrWall( row, column - 1) ||           // If the node to the west is an obstacle
					isObstacleOrWall( row + 1, column - 1 ) ) )     // if the node to the south West is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                    // if the node to the south is empty
					isEmpty(row, column - 1) &&                                                          // if the node to the west is empty
					(getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.SOUTH ] > 0 ||    // If the node to the south west has is a straight jump point ( or primary jump point) going south
					 getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.WEST  ] > 0))     // If the node to the south west has is a straight jump point ( or primary jump point) going West
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column - 1 ).jpDistances[ (int) eDirections.SOUTH_WEST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.SOUTH_WEST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.SOUTH_WEST ] = -1 + jumpDistance;
					}
				}

				// Calculate SOUTH EAST DISTNACES
				if ( row  == maxRows - 1 || column == rowSize -1 || (    // If we in the south east corner
					isObstacleOrWall( row + 1, column ) ||               // If the node to the south is an obstacle
					isObstacleOrWall( row, column + 1) ||                // If the node to the east is an obstacle
					isObstacleOrWall( row + 1, column + 1 ) ) )          // if the node to the south east is an obstacle
				{
					// Wall one away
					node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 0;
				}
				else if ( isEmpty(row + 1, column) &&                                                    // if the node to the south is empty
					isEmpty(row, column + 1) &&                                                          // if the node to the east is empty
					(getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.SOUTH ] > 0 ||    // If the node to the south east has is a straight jump point ( or primary jump point) going south
					 getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.EAST  ] > 0))     // If the node to the south east has is a straight jump point ( or primary jump point) going east
				{
					// Diagonal one away
					node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 1;
				}
				else
				{
					// Increment from last
					int jumpDistance = getNode( row + 1, column + 1 ).jpDistances[ (int) eDirections.SOUTH_EAST ];

					if (jumpDistance > 0)
					{
						node.jpDistances[ (int) eDirections.SOUTH_EAST ] = 1 + jumpDistance;
					}
					else //if( jumpDistance <= 0 )
					{
						node.jpDistances[ (int) eDirections.SOUTH_EAST ] = -1 + jumpDistance;
					}
				}
			}
		}
	}

	static readonly float SQRT_2 = Mathf.Sqrt( 2 );
	static readonly float SQRT_2_MINUS_1 = Mathf.Sqrt( 2 ) - 1.0f;

	internal static int octileHeuristic( int curr_row, int curr_column, int goal_row, int goal_column )
	{
		int heuristic;
		int row_dist = goal_row - curr_row;
		int column_dist = goal_column - curr_column;

		heuristic = (int) ( Mathf.Max( row_dist, column_dist ) + SQRT_2_MINUS_1 * Mathf.Min( row_dist, column_dist ) );

		return heuristic;
	}

	private eDirections[] getAllValidDirections( PathfindingNode curr_node )
	{
		// If parent is null, then explore all possible directions
		return curr_node.parent == null ? 
			allDirections : 
			validDirLookUpTable[ curr_node.directionFromParent ];
	}

	private bool isCardinal( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.SOUTH:
			case eDirections.EAST:
			case eDirections.NORTH:
			case eDirections.WEST:
				return true;
		}

		return false;
	}

	private bool isDiagonal( eDirections dir )
	{
		switch ( dir )
		{
			case eDirections.SOUTH_EAST:
			case eDirections.SOUTH_WEST:
			case eDirections.NORTH_EAST:
			case eDirections.NORTH_WEST:
				return true;
		}

		return false;
	}

	private bool goalIsInExactDirection( Point curr, eDirections dir, Point goal )
	{
		int diff_column = goal.column - curr.column;
		int diff_row    = goal.row - curr.row;

		// note: north would be DECREASING in row, not increasing. Rows grow positive while going south!
		switch ( dir )
		{
			case eDirections.NORTH:
				return diff_row < 0 && diff_column == 0;
			case eDirections.NORTH_EAST:
				return diff_row < 0 && diff_column > 0 && Mathf.Abs(diff_row) == Mathf.Abs(diff_column);
			case eDirections.EAST:
				return diff_row == 0 && diff_column > 0;
			case eDirections.SOUTH_EAST:
				return diff_row > 0 && diff_column > 0 && Mathf.Abs(diff_row) == Mathf.Abs(diff_column);
			case eDirections.SOUTH:
				return diff_row > 0 && diff_column == 0;
			case eDirections.SOUTH_WEST:
				return diff_row > 0 && diff_column < 0 && Mathf.Abs(diff_row) == Mathf.Abs(diff_column);
			case eDirections.WEST:
				return diff_row == 0 && diff_column < 0;
			case eDirections.NORTH_WEST:
				return diff_row < 0 && diff_column < 0 && Mathf.Abs(diff_row) == Mathf.Abs(diff_column);
		}

		return false;
	}

	private bool goalIsInGeneralDirection( Point curr, eDirections dir, Point goal )
	{
		int diff_column = goal.column - curr.column;
		int diff_row    = goal.row - curr.row;

		// note: north would be DECREASING in row, not increasing. Rows grow positive while going south!
		switch ( dir )
		{
			case eDirections.NORTH:
				return diff_row < 0 && diff_column == 0;
			case eDirections.NORTH_EAST:
				return diff_row < 0 && diff_column > 0;
			case eDirections.EAST:
				return diff_row == 0 && diff_column > 0;
			case eDirections.SOUTH_EAST:
				return diff_row > 0 && diff_column > 0;
			case eDirections.SOUTH:
				return diff_row > 0 && diff_column == 0;
			case eDirections.SOUTH_WEST:
				return diff_row > 0 && diff_column < 0;
			case eDirections.WEST:
				return diff_row == 0 && diff_column < 0;
			case eDirections.NORTH_WEST:
				return diff_row < 0 && diff_column < 0;
		}

		return false;
	}

	/// <summary>
	/// Get the Node, starting at the given position, in the given direction, at the given distance away.
	/// </summary>
	private PathfindingNode getNodeDist( int row, int column, eDirections direction, int dist )
	{
		PathfindingNode new_node = null;
		int new_row = row, new_column = column;

		switch ( direction )
		{
			case eDirections.NORTH:
				new_row -= dist;
				break;
			case eDirections.NORTH_EAST:
				new_row -= dist;
				new_column += dist;
				break;
			case eDirections.EAST:
				new_column += dist;
				break;
			case eDirections.SOUTH_EAST:
				new_row += dist;
				new_column += dist;
				break;
			case eDirections.SOUTH:
				new_row += dist;
				break;
			case eDirections.SOUTH_WEST:
				new_row += dist;
				new_column -= dist;
				break;
			case eDirections.WEST:
				new_column -= dist;
				break;
			case eDirections.NORTH_WEST:
				new_row -= dist;
				new_column -= dist;
				break;
		}

		// w/ the new coordinates, get the node
		if ( isInBounds( new_row, new_column ) )
		{
			new_node = this.pathfindingNodes[ this.rowColumnToIndex( new_row, new_column ) ];
		}

		return new_node;
	}

	// Reverse the goal
	public List< Point > reconstructPath( PathfindingNode goal, Point start )
	{
		List< Point > path = new List< Point >();
		PathfindingNode curr_node = goal;

		while ( curr_node.parent != null )
		{
			path.Add( curr_node.pos );
			curr_node = curr_node.parent;
		}

		// Push starting node on there too
		path.Add( start );

		path.Reverse();  // really wish I could have just push_front but NO!
		return path;
	}

	public IEnumerator getPathAsync( Point start, Point goal )
	{
		PriorityQueue< PathfindingNode, float > open_set = new PriorityQueue< PathfindingNode, float >();
		bool found_path = false;
		PathfindReturn return_status = new PathfindReturn();

		ResetPathfindingNodeData();
		PathfindingNode starting_node = this.pathfindingNodes[ pointToIndex( start ) ];
		starting_node.pos = start;
		starting_node.parent = null;
		starting_node.givenCost = 0;
		starting_node.finalCost = 0;
		starting_node.listStatus = ListStatus.ON_OPEN;

		open_set.push( starting_node, 0 );

		while ( ! open_set.isEmpty() )
		{
			PathfindingNode curr_node = open_set.pop();
			PathfindingNode parent = curr_node.parent;
			Node jp_node = gridNodes[ pointToIndex( curr_node.pos ) ];    // get jump point info

			return_status._current = curr_node;

			// Check if we've reached the goal
			if ( curr_node.pos.Equals( goal ) ) 
			{
				// end and return path
				return_status.path = reconstructPath( curr_node, start );
				return_status._status = PathfindReturn.PathfindStatus.FOUND;
				found_path = true;
				yield return return_status;
				break;
			}

			yield return return_status;

			// foreach direction from parent
			foreach ( eDirections dir in getAllValidDirections( curr_node ) )
			{
				PathfindingNode new_successor = null;
				int given_cost = 0;

				// goal is closer than wall distance or closer than or equal to jump point distnace
				if ( isCardinal( dir ) &&
				     goalIsInExactDirection( curr_node.pos, dir, goal ) && 
				     Point.diff( curr_node.pos, goal ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) )
				{
					new_successor = this.pathfindingNodes[ pointToIndex( goal ) ];

					given_cost = curr_node.givenCost + Point.diff( curr_node.pos, goal );
				}
				// Goal is closer or equal in either row or column than wall or jump point distance
				else if ( isDiagonal( dir ) &&
				          goalIsInGeneralDirection( curr_node.pos, dir, goal ) && 
				          ( Mathf.Abs( goal.column - curr_node.pos.column ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) ||
				            Mathf.Abs( goal.row - curr_node.pos.row ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) ) )
				{
					// Create a target jump point
					// int minDiff = min(RowDiff(curNode, goalNode),
					//                   ColDiff(curNode, goalNode));
					int min_diff = Mathf.Min( Mathf.Abs( goal.column - curr_node.pos.column ), 
					                          Mathf.Abs( goal.row - curr_node.pos.row ) );

					// newSuccessor = GetNode (curNode, minDiff, direction);
					new_successor = getNodeDist( 
						curr_node.pos.row, 
						curr_node.pos.column, 
						dir, 
						min_diff );

					// givenCost = curNode->givenCost + (SQRT2 * DiffNodes(curNode, newSuccessor));
					given_cost = curr_node.givenCost + (int)( SQRT_2 * Point.diff( curr_node.pos, new_successor.pos ) );
				}
				else if ( jp_node.jpDistances[ (int) dir ] > 0 )
				{
					// Jump Point in this direction
					// newSuccessor = GetNode(curNode, direction);
					new_successor = getNodeDist( 
						curr_node.pos.row, 
						curr_node.pos.column, 
						dir, 
						jp_node.jpDistances[ (int) dir ] );
					
					// givenCost = DiffNodes(curNode, newSuccessor);
					given_cost = Point.diff( curr_node.pos, new_successor.pos );

					// if (diagonal direction) { givenCost *= SQRT2; }
					if ( isDiagonal( dir ) )
					{
						given_cost = (int)( given_cost * SQRT_2 );
					}

					// givenCost += curNode->givenCost;
					given_cost += curr_node.givenCost;
				}

				// Traditional A* from this point
				if ( new_successor != null )
				{
				// 	if (newSuccessor not on OpenList)
					if ( new_successor.listStatus != ListStatus.ON_OPEN )
					{
				// 		newSuccessor->parent = curNode;
						new_successor.parent = curr_node;
				// 		newSuccessor->givenCost = givenCost;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
				// 		newSuccessor->finalCost = givenCost +
				// 			CalculateHeuristic(curNode, goalNode);
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.column, new_successor.pos.row, goal.column, goal.row );
						new_successor.listStatus = ListStatus.ON_OPEN;
				// 		OpenList.Push(newSuccessor);
						open_set.push( new_successor, new_successor.finalCost );
					}
				// 	else if(givenCost < newSuccessor->givenCost)
					else if ( given_cost < new_successor.givenCost )
					{
				// 		newSuccessor->parent = curNode;
						new_successor.parent = curr_node;
				// 		newSuccessor->givenCost = givenCost;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
				// 		newSuccessor->finalCost = givenCost +
				// 			CalculateHeuristic(curNode, goalNode);
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.column, new_successor.pos.row, goal.column, goal.row );
						new_successor.listStatus = ListStatus.ON_OPEN;
				// 		OpenList.Update(newSuccessor);
						open_set.push( new_successor, new_successor.finalCost );
					}
				}
			}
		}

		if ( ! found_path )
		{
			return_status._status = PathfindReturn.PathfindStatus.NOT_FOUND;
			yield return return_status;
		}
	}

	public List< Point > getPath( Point start, Point goal )
	{
		List< Point > path = new List< Point >();
		PriorityQueue< PathfindingNode, float > open_set = new PriorityQueue< PathfindingNode, float >();

		ResetPathfindingNodeData();
		PathfindingNode starting_node = this.pathfindingNodes[ pointToIndex( start ) ];
		starting_node.pos = start;
		starting_node.parent = null;
		starting_node.givenCost = 0;
		starting_node.finalCost = 0;
		starting_node.listStatus = ListStatus.ON_OPEN;

		open_set.push( starting_node, 0 );

		while ( ! open_set.isEmpty() )
		{
			PathfindingNode curr_node = open_set.pop();
			PathfindingNode parent = curr_node.parent;
			Node jp_node = gridNodes[ pointToIndex( curr_node.pos ) ];    // get jump point info

			// Check if we've reached the goal
			if ( curr_node.pos.Equals( goal ) ) 
			{
				// end and return path
				return reconstructPath( curr_node, start );
			}

			// foreach direction from parent
			foreach ( eDirections dir in getAllValidDirections( curr_node ) )
			{
				PathfindingNode new_successor = null;
				int given_cost = 0;

				// goal is closer than wall distance or closer than or equal to jump point distnace
				if ( isCardinal( dir ) &&
				     goalIsInExactDirection( curr_node.pos, dir, goal ) && 
				     Point.diff( curr_node.pos, goal ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) )
				{
					new_successor = this.pathfindingNodes[ pointToIndex( goal ) ];

					given_cost = curr_node.givenCost + Point.diff( curr_node.pos, goal );
				}
				// Goal is closer or equal in either row or column than wall or jump point distance
				else if ( isDiagonal( dir ) &&
				          goalIsInGeneralDirection( curr_node.pos, dir, goal ) && 
				          ( Mathf.Abs( goal.column - curr_node.pos.column ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) ||
				            Mathf.Abs( goal.row - curr_node.pos.row ) <= Mathf.Abs( jp_node.jpDistances[ (int) dir ] ) ) )
				{
					// Create a target jump point
					// int minDiff = min(RowDiff(curNode, goalNode),
					//                   ColDiff(curNode, goalNode));
					int min_diff = Mathf.Min( Mathf.Abs( goal.column - curr_node.pos.column ), 
					                          Mathf.Abs( goal.row - curr_node.pos.row ) );

					// newSuccessor = GetNode (curNode, minDiff, direction);
					new_successor = getNodeDist( 
						curr_node.pos.row, 
						curr_node.pos.column, 
						dir, 
						min_diff );

					// givenCost = curNode->givenCost + (SQRT2 * DiffNodes(curNode, newSuccessor));
					given_cost = curr_node.givenCost + (int)( SQRT_2 * Point.diff( curr_node.pos, new_successor.pos ) );
				}
				else if ( jp_node.jpDistances[ (int) dir ] > 0 )
				{
					// Jump Point in this direction
					// newSuccessor = GetNode(curNode, direction);
					new_successor = getNodeDist( 
						curr_node.pos.row, 
						curr_node.pos.column, 
						dir, 
						jp_node.jpDistances[ (int) dir ] );
					
					// givenCost = DiffNodes(curNode, newSuccessor);
					given_cost = Point.diff( curr_node.pos, new_successor.pos );

					// if (diagonal direction) { givenCost *= SQRT2; }
					if ( isDiagonal( dir ) )
					{
						given_cost = (int)( given_cost * SQRT_2 );
					}

					// givenCost += curNode->givenCost;
					given_cost += curr_node.givenCost;
				}

				// Traditional A* from this point
				if ( new_successor != null )
				{
				// 	if (newSuccessor not on OpenList)
					if ( new_successor.listStatus != ListStatus.ON_OPEN )
					{
				// 		newSuccessor->parent = curNode;
						new_successor.parent = curr_node;
				// 		newSuccessor->givenCost = givenCost;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
				// 		newSuccessor->finalCost = givenCost +
				// 			CalculateHeuristic(curNode, goalNode);
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.column, new_successor.pos.row, goal.column, goal.row );
						new_successor.listStatus = ListStatus.ON_OPEN;
				// 		OpenList.Push(newSuccessor);
						open_set.push( new_successor, new_successor.finalCost );
					}
				// 	else if(givenCost < newSuccessor->givenCost)
					else if ( given_cost < new_successor.givenCost )
					{
				// 		newSuccessor->parent = curNode;
						new_successor.parent = curr_node;
				// 		newSuccessor->givenCost = givenCost;
						new_successor.givenCost = given_cost;
						new_successor.directionFromParent = dir;
				// 		newSuccessor->finalCost = givenCost +
				// 			CalculateHeuristic(curNode, goalNode);
						new_successor.finalCost = given_cost + octileHeuristic( new_successor.pos.column, new_successor.pos.row, goal.column, goal.row );
						new_successor.listStatus = ListStatus.ON_OPEN;
				// 		OpenList.Update(newSuccessor);
						open_set.push( new_successor, new_successor.finalCost );
					}
				}
			}
		}

		return path;
	}

	public void ResetPathfindingNodeData()
	{
		foreach ( var node in this.pathfindingNodes )
		{
			node.Reset();
		}
	}
}
