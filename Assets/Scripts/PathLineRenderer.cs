using UnityEngine;
using System.Collections.Generic;

// This Compoent will Take in a path argument and then draw the path, on the screen
public class PathLineRenderer : MonoBehaviour 
{
#region Serialize Fields

	[SerializeField] private GameObject _pathLinePrefab;

	// Grid view so I can pull block size and buffer size and I know where to place my lines
	public GridView _gridView;

#endregion

#region Private Fields

	private GameObject[] _lines = new GameObject[0];

#endregion

// ( I should probably throw these in an interface )
#region public Methods

	// Set the points and turn on the path
	public void drawPath( List<Point> directed_path )
	{
		Debug.Assert( _gridView != null, "Grid View is NULL");

		disablePath();

		// Allocate Child Array Size
		_lines = new GameObject[ directed_path.Count - 1 ];
		Point[] directed_path_as_arr = directed_path.ToArray();

		for ( int i = 1 ; i < directed_path_as_arr.Length ; ++i )
		{
			Point start_point = directed_path_as_arr[ i - 1 ], 
			      end_point   = directed_path_as_arr[ i ];

			// Calculate WorldSpace position based off what Grid View Says
			Vector3 start_world_pos = _gridView.getNodePosAsWorldPos( start_point ),
			        end_world_pos   = _gridView.getNodePosAsWorldPos( end_point );

			// Allocate a new Path Line
			GameObject line_path_obj = Instantiate( _pathLinePrefab );
			line_path_obj.GetComponent<Transform>().parent = this.GetComponent<Transform>();    // I'm your Da'Da now

			// Get Line Info
			var pathSegment = line_path_obj.GetComponent<PathSegmentRenderer>();
			pathSegment.DrawSegment( start_world_pos, end_world_pos );

			_lines[ i - 1 ] = line_path_obj;    // Grab a ref to this new object
		}
	}

	// Just turn off the view
	public void disablePath()
	{
		// Kill all my children
		foreach ( GameObject child in _lines )
		{
			DestroyImmediate( child );
		}
	}

#endregion

}
