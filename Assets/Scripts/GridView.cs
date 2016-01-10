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

	[SerializeField] private PathLineRenderer _pathRenderer;

	private int previousNumBlocks = 0;
	private float previousBuffer = 0;

	private GameObject[] childObjects = new GameObject[1];

	private Grid grid = new Grid();

	private Queue< BlockScript > selectedPathPoints = new Queue< BlockScript >();

	private IEnumerator findPath = null;

	void Start()
	{
		Debug.Assert( _pathRenderer != null, "Path Renderer isn't set!" );

		_pathRenderer._gridView = this;
	}

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

	public void Reset()
	{
		JPSState.state = eJPSState.ST_OBSTACLE_BUILDING;
		_pathRenderer.disablePath();
		findPath = null;
		selectedPathPoints.Clear();
		resize();
	}

	// Resize the grid based off the new values
	public void resize()
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

	// Return the World Position of these grid points, relative to this object
	public Vector3 getNodePosAsWorldPos( Point point )
	{
		var trans = GetComponent<Transform>();
		
		return new Vector3(
			trans.localPosition.x + point.column *  ( blockSize + blockBuffer ),
			trans.localPosition.y + point.row    * -( blockSize + blockBuffer ),
			0.0f
		);
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

		// Disable existing paths if we are restarting
		foreach ( var block_script in selectedPathPoints )
		{
			block_script.isPathEndPoint = false;
		}

		selectedPathPoints.Clear();

		// Disable path view
		_pathRenderer.disablePath();
		findPath = null;

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

		BlockScript[] points = this.selectedPathPoints.ToArray();

		Point start = points[ 0 ].nodeReference.pos;
		Point stop  = points[ 1 ].nodeReference.pos;

		List<Point> path = grid.getPath( start, stop );

		if ( path != null && path.Count != 0 )
		{
			_pathRenderer.drawPath( path );    // Draw Path on Screen
		}
	}

	public void StepThroughPath()
	{
		// Verify at least TWO END POINTS ARE SET!
		if ( this.selectedPathPoints.Count != 2 ) return;
		JPSState.state = eJPSState.ST_FIND_PATH;
		JPSState.LastPathFound = true;

		if ( findPath == null )
		{
			BlockScript[] points = this.selectedPathPoints.ToArray();

			Point start = points[ 0 ].nodeReference.pos;
			Point stop  = points[ 1 ].nodeReference.pos;

			// Get enumerator path finding
			findPath = grid.getPathAsync( start, stop );

			findPath.MoveNext();  // First iteration doesn't really do anything, so just skip it
		}

		// step through path finding process
		if ( findPath.MoveNext() )
		{
			PathfindReturn curr_return = (PathfindReturn) findPath.Current;

			switch ( curr_return._status )
			{
				case PathfindReturn.PathfindStatus.SEARCHING:
					// render path up to this point
					List< Point > path_so_far = grid.reconstructPath( 
						curr_return._current, 
						selectedPathPoints.Peek().nodeReference.pos 
					);
					_pathRenderer.drawPath( path_so_far );
					break;
				case PathfindReturn.PathfindStatus.FOUND:
					// render path
					_pathRenderer.drawPath( curr_return.path );
					findPath = null;
					JPSState.state = eJPSState.ST_PATH_FIND_COMPLETE;
					break;
				case PathfindReturn.PathfindStatus.NOT_FOUND:
					// disable rendering, ya blew it
					_pathRenderer.disablePath();
					findPath = null;
					JPSState.state = eJPSState.ST_PATH_FIND_COMPLETE;
					JPSState.LastPathFound = false;   // tell everyone that we failed to find a path
					break;
			}
		}
		else
		{
			Debug.Log("WE ARRIVED AT THE END!");
			findPath = null;
			JPSState.state = eJPSState.ST_PATH_FIND_COMPLETE;
		}
	}

#endregion
}
