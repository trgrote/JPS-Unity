using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class GridView : MonoBehaviour 
{
	public GameObject blockPrefab = null;

	[RangeAttribute(0, 255)]
	public int numBlocks = 1;
	public float blockSize = 0.64f;
	public int rowSize = 10;
	[RangeAttribute(0.0f, 1.0f)]
	public float blockBuffer = 0.0f;

	private int previousNumBlocks = 0;
	private float previousBuffer = 0;

	private GameObject[] childObjects = new GameObject[1];

	private Grid grid = new Grid();

	private Queue< BlockScript > selectedPathPoints = new Queue< BlockScript >();

	// Update is called once per frame
	void Update () 
	{
		// If no one has given us a prefab to use, then don't make anything as we'll just get null pointer exception nonsense
		if ( blockPrefab == null )
			return;

		// If we need to resize then do
		if ( previousNumBlocks != numBlocks || previousBuffer != blockBuffer )
		{
			resize();
			previousNumBlocks = numBlocks;
			previousBuffer = blockBuffer;
		}
	}

#region Helper Functions

	// Resize the grid based off the new values
	void resize()
	{
		// clear the queue
		selectedPathPoints.Clear();

		// Kill all my children
		foreach ( GameObject child in childObjects )
		{
			DestroyImmediate( child );
		}

		// realloc the grids
		grid.gridNodes = new Node[numBlocks];
		grid.pathfindingNodes = new PathfindingNode[numBlocks];
		childObjects   = new GameObject[numBlocks];

		for ( int i = 0; i < numBlocks ; ++i )
		{
			int column = i % rowSize;
			int row    = i / rowSize;
			
			// Create a new Child object
			GameObject child = Instantiate( blockPrefab );
			child.GetComponent<Transform>().parent = GetComponent<Transform>();  // Set as parent of this new child
			child.GetComponent<Transform>().localPosition = new Vector3(
				column *  ( blockSize + blockBuffer ),
				row    * -( blockSize + blockBuffer ),
				0.0f
			);

			grid.gridNodes[ i ] = new Node();
			grid.gridNodes[ i ].pos  = new Point( row, column );

			grid.pathfindingNodes[ i ] = new PathfindingNode();
			grid.pathfindingNodes[ i ].pos = new Point( row, column );

			grid.rowSize = this.rowSize;
			child.GetComponent<BlockScript>().nodeReference = grid.gridNodes[ i ]; // give the child a shared_ptr reference to the node it needs to act on
			child.GetComponent<BlockScript>().gridView = this;

			childObjects[ i ] = child;
		}
	}

	public void markNodeAsPathPoint( BlockScript block_script )
	{
		if ( selectedPathPoints.Contains( block_script ) )
		{
			return;
		}

		// max size has to be 2
		while ( selectedPathPoints.Count >= 2 )
		{
			selectedPathPoints.Dequeue().removePathMarker();   // remove the oldest element
		}

		// enqueue the new postition
		selectedPathPoints.Enqueue( block_script );
	}

#endregion

#region Button Callbacks

	public void CalcPrimaryJumpPoints()
	{
		grid.buildPrimaryJumpPoints();    // Build primary Jump Points
		JPSState.state = eJPSState.ST_PRIMARY_JPS_BUILDING; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}
	}

	public void CalcStraightJPDistances()
	{
		grid.buildStraightJumpPoints();    // Build primary Jump Points
		JPSState.state = eJPSState.ST_STRAIGHT_JPS_BUILDING; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}
	}

	public void CalcDiagonalJPDistances()
	{
		grid.buildDiagonalJumpPoints();    // Build primary Jump Points
		JPSState.state = eJPSState.ST_DIAGONAL_JPS_BUILDING; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}
	}

	public void CalcWallDistances()
	{
		//grid.buildDiagonalJumpPoints();    // Build primary Jump Points
		JPSState.state = eJPSState.ST_WALL_DISTANCES_BUILT; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}
	}

	// This button just enters the path search mode where the user can select the start and end points
	public void PlaceSearchEndPoints()
	{
		JPSState.state = eJPSState.ST_PLACE_SEARCH_ENDPOINTS; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}
	}

	public void BeginPathFind()
	{
		// Verify at least TWO END POINTS ARE SET!
		if ( this.selectedPathPoints.Count != 2 ) return;

		JPSState.state = eJPSState.ST_FIND_PATH; // transition state to Primary Jump Point Building State

		// Tell each child object to re-evaulte their rendering info
		foreach ( GameObject child in childObjects )
		{
			BlockScript block_component = child.GetComponent<BlockScript>();
			block_component.setupDisplay();	
		}

		Point start = this.selectedPathPoints.Dequeue().nodeReference.pos;
		Point stop = this.selectedPathPoints.Dequeue().nodeReference.pos;

		List<Point> path = grid.getPath( start, stop );

		if ( path != null && path.Count != 0 )
		{
			Debug.Log("Found Path");
			foreach ( Point pos in path )
			{
				Debug.Log( pos );
			}
		}
		else
		{
			Debug.Log("failed to find path");
		}
	}

#endregion
}
