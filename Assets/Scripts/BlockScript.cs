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

	[SerializeField] private SpriteRenderer jumpPointIndicator = null;
	[SerializeField] private SpriteRenderer northJPArrow       = null;
	[SerializeField] private SpriteRenderer southJPArrow       = null;
	[SerializeField] private SpriteRenderer eastJPArrow        = null;
	[SerializeField] private SpriteRenderer westJPArrow        = null;

	[SerializeField] Color jumpPointDistanceColor = Color.blue;
	[SerializeField] Color wallDistanceColor      = Color.red;

	private void setJumpPointArrows()
	{
		jumpPointIndicator.gameObject.SetActive( nodeReference.isJumpPoint );
		northJPArrow.gameObject.SetActive( nodeReference.jumpPointDirection[ (int) eDirections.DIR_NORTH ] );
		southJPArrow.gameObject.SetActive( nodeReference.jumpPointDirection[ (int) eDirections.DIR_SOUTH ] );
		eastJPArrow.gameObject.SetActive ( nodeReference.jumpPointDirection[ (int) eDirections.DIR_EAST  ] );
		westJPArrow.gameObject.SetActive ( nodeReference.jumpPointDirection[ (int) eDirections.DIR_WEST  ] );
	}

	private void disableJumpPointArrows()
	{
		northJPArrow.gameObject.SetActive( false );
		southJPArrow.gameObject.SetActive( false );
		eastJPArrow.gameObject.SetActive ( false );
		westJPArrow.gameObject.SetActive ( false );
	}

	private void displayJumpPointDistances()
	{
		northDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH      ].ToString();
		northEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_EAST ].ToString();
		eastDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.DIR_EAST       ].ToString();
		southEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ].ToString();
		southDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH      ].ToString();
		southWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ].ToString();
		westDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.DIR_WEST       ].ToString();
		northWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_WEST ].ToString();
	}

	private void displayGreaterThanZeroJumpDistances()
	{
		displayJumpPointDistances();

		// Set Active if value != 0
		northDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_NORTH      ] > 0 );
		northEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] > 0 );
		eastDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_EAST       ] > 0 );
		southEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] > 0 );
		southDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH      ] > 0 );
		southWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] > 0 );
		westDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_WEST       ] > 0 );
		northWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] > 0 );
	}

	// Display all Jump Point Distances ( including Wall distances )
	private void dispayAllJumpPointDistances()
	{
		displayJumpPointDistances();
		setJPValuesEnabled( true );
	}

	private void setJumpPointColors()
	{
		northDistanceText.color     = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH      ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		northEastDistanceText.color = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_EAST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		eastDistanceText.color      = nodeReference.jpDistances[ (int) eDirections.DIR_EAST       ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southEastDistanceText.color = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_EAST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southDistanceText.color     = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH      ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southWestDistanceText.color = nodeReference.jpDistances[ (int) eDirections.DIR_SOUTH_WEST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		westDistanceText.color      = nodeReference.jpDistances[ (int) eDirections.DIR_WEST       ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		northWestDistanceText.color = nodeReference.jpDistances[ (int) eDirections.DIR_NORTH_WEST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
	}

	public void setSprite()
	{
		if ( nodeReference == null ) return;

		switch ( JPSState.state )
		{
			case eJPSState.ST_OBSTACLE_BUILDING:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				disableJumpPointArrows(); // make sure jump point arrows are off

				// Disable all the texts, because no one wants to see that shit
				setJPValuesEnabled( false );

				setJumpPointColors();

				break;
			case eJPSState.ST_PRIMARY_JPS_BUILDING:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				// Enabled your Arrows, if you are a jump point
				if ( ! nodeReference.isObstacle && nodeReference.isJumpPoint )
				{
					setJumpPointArrows();
				}
				else
				{
					disableJumpPointArrows(); // make sure jump point arrows are off
				}

				// Disable all the texts, because no one wants to see that shit
				setJPValuesEnabled( false );
				setJumpPointColors();

				break;
			case eJPSState.ST_STRAIGHT_JPS_BUILDING:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				disableJumpPointArrows(); // make sure jump point arrows are off

				// Disable all the texts, because no one wants to see that shit
				displayGreaterThanZeroJumpDistances();
				setJumpPointColors();

				break;
			case eJPSState.ST_DIAGONAL_JPS_BUILDING:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				disableJumpPointArrows(); // make sure jump point arrows are off

				// Disable all the texts, because no one wants to see that shit
				displayGreaterThanZeroJumpDistances();
				setJumpPointColors();

				break;
			case eJPSState.ST_WALL_DISTANCES_BUILT:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				disableJumpPointArrows(); // make sure jump point arrows are off

				// Disable all the texts
				dispayAllJumpPointDistances();
				setJumpPointColors();
				break;
			default:
				break;
		}
	}

	// enables/disables JP points ( always disables if an obstacle )
	private void setJPValuesEnabled( bool enabled )
	{
		northDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && enabled );
		northEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && enabled );
		eastDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && enabled );
		southEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && enabled );
		southDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && enabled );
		southWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && enabled );
		westDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && enabled );
		northWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && enabled );
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
