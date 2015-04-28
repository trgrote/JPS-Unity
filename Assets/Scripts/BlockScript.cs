using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BlockScript : MonoBehaviour
{
	public Sprite passableSprite = null;
	public Sprite obstacleSprite = null;

	// Weak Reference to node
	public Node nodeReference = null;

	[SerializeField] private TextMesh northDistanceText     = null;
	[SerializeField] private TextMesh northEastDistanceText = null;
	[SerializeField] private TextMesh eastDistanceText      = null;
	[SerializeField] private TextMesh southEastDistanceText = null;
	[SerializeField] private TextMesh southDistanceText     = null;
	[SerializeField] private TextMesh southWestDistanceText = null;
	[SerializeField] private TextMesh westDistanceText      = null;
	[SerializeField] private TextMesh northWestDistanceText = null;

	public void setSprite()
	{
		if ( nodeReference == null ) return;

		GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
			passableSprite :
			obstacleSprite;

		if ( ! nodeReference.isObstacle )
		{
			setJPValuesEnabled( true );

			// Assign the Values to Each Text Value
			northDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH      ].ToString();
			northEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_EAST ].ToString();
			eastDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.DIR_EAST       ].ToString();
			southEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ].ToString();
			southDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH      ].ToString();
			southWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ].ToString();
			westDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.DIR_WEST       ].ToString();
			northWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_WEST ].ToString();

			if ( nodeReference.isJumpPoint )
			{
				GetComponent<SpriteRenderer>().color = Color.blue;
			}
		}
		else
		{
			// Disable all the texts, because no one wants to see that shit
			setJPValuesEnabled( false );
		}

		// Make sure we are in the current state to even render the jump point values
		if ( true /* JPSState.state != eJPSState.ST_JP_CALCULATED */ )
		{
			setJPValuesEnabled( false );
		}
	}

	private void setJPValuesEnabled( bool enabled )
	{
		northDistanceText.gameObject.SetActive(enabled);
		northEastDistanceText.gameObject.SetActive(enabled);
		eastDistanceText.gameObject.SetActive(enabled);
		southEastDistanceText.gameObject.SetActive(enabled);
		southDistanceText.gameObject.SetActive(enabled);
		southWestDistanceText.gameObject.SetActive(enabled);
		westDistanceText.gameObject.SetActive(enabled);
		northWestDistanceText.gameObject.SetActive(enabled);
	}

	void OnMouseDown()
	{
		if ( nodeReference == null ) return;
		if ( JPSState.state != eJPSState.ST_OBSTACLE_BUILDING ) return;
		
		nodeReference.isObstacle = ! nodeReference.isObstacle;
		setSprite();
	}

	// Use this for initialization
	void Start () 
	{
		setSprite();
	}
}
