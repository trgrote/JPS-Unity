using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GridView : MonoBehaviour 
{
	public GameObject blockPrefab = null;

	[RangeAttribute(0, 255)]
	public int numBlocks = 1;
	private int previousNumBlocks = 0;
	public float blockSize = 0.64f;
	public int rowSize = 10;

	private GameObject[] childObjects = new GameObject[1];

	void resize()
	{
		// Kill all my children
		foreach ( GameObject child in childObjects )
		{
			DestroyImmediate( child );
		}

		childObjects = new GameObject[numBlocks];

		for ( int i = 0; i < numBlocks ; ++i )
		{
			// Create a new Child object
			GameObject child = Instantiate( blockPrefab );
			child.GetComponent<Transform>().parent = GetComponent<Transform>();  // Set as parent of this new child
			child.GetComponent<Transform>().localPosition = new Vector3(
				( i % rowSize ) *  blockSize,
				( i / rowSize ) * -blockSize,
				0.0f
			);

			childObjects[ i ] = child;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		// If no one has given us a prefab to use, then don't make anything as we'll just get null pointer exception nonsense
		if ( blockPrefab == null )
			return;

		// If we need to resize then do
		if ( previousNumBlocks != numBlocks )
		{
			resize();
			previousNumBlocks = numBlocks;
		}
	}
}
