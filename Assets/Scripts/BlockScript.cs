using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class BlockScript : MonoBehaviour
{
	[Header("Sprites")]
	[SerializeField] private Sprite passableSprite = null;
	[SerializeField] private Sprite obstacleSprite = null;

	[Header("Text")]
	[SerializeField] private TextMesh northDistanceText     = null;
	[SerializeField] private TextMesh northEastDistanceText = null;
	[SerializeField] private TextMesh eastDistanceText      = null;
	[SerializeField] private TextMesh southEastDistanceText = null;
	[SerializeField] private TextMesh southDistanceText     = null;
	[SerializeField] private TextMesh southWestDistanceText = null;
	[SerializeField] private TextMesh westDistanceText      = null;
	[SerializeField] private TextMesh northWestDistanceText = null;

	[Header("Sprite Renderers")]
	[SerializeField] private SpriteRenderer jumpPointIndicator = null;
	[SerializeField] private SpriteRenderer northJPArrow       = null;
	[SerializeField] private SpriteRenderer southJPArrow       = null;
	[SerializeField] private SpriteRenderer eastJPArrow        = null;
	[SerializeField] private SpriteRenderer westJPArrow        = null;
	[SerializeField] private SpriteRenderer pathEndPoint;

	[Header("Marker Colors")]
	[SerializeField] Color jumpPointDistanceColor = Color.blue;
	[SerializeField] Color wallDistanceColor      = Color.red;

	// Weak Reference to node
	public Node nodeReference = null;
	public GridView gridView;

	public bool isPathEndPoint = false;

	private void setJumpPointArrows()
	{
		setJumpPointIndicator( true );
		northJPArrow.gameObject.SetActive( nodeReference.jumpPointDirection[ (int) eDirections.NORTH ] );
		southJPArrow.gameObject.SetActive( nodeReference.jumpPointDirection[ (int) eDirections.SOUTH ] );
		eastJPArrow.gameObject.SetActive ( nodeReference.jumpPointDirection[ (int) eDirections.EAST  ] );
		westJPArrow.gameObject.SetActive ( nodeReference.jumpPointDirection[ (int) eDirections.WEST  ] );
	}

	private void setJumpPointIndicator( bool enabled )
	{
		jumpPointIndicator.gameObject.SetActive( nodeReference.isJumpPoint && enabled );
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
		northDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.NORTH      ].ToString();
		northEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.NORTH_EAST ].ToString();
		eastDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.EAST       ].ToString();
		southEastDistanceText.text = nodeReference.jpDistances[ (int) eDirections.SOUTH_EAST ].ToString();
		southDistanceText.text     = nodeReference.jpDistances[ (int) eDirections.SOUTH      ].ToString();
		southWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.SOUTH_WEST ].ToString();
		westDistanceText.text      = nodeReference.jpDistances[ (int) eDirections.WEST       ].ToString();
		northWestDistanceText.text = nodeReference.jpDistances[ (int) eDirections.NORTH_WEST ].ToString();
	}

	private void displayGreaterThanZeroJumpDistances()
	{
		displayJumpPointDistances();

		// Set Active if value != 0
		northDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.NORTH      ] > 0 );
		northEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.NORTH_EAST ] > 0 );
		eastDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.EAST       ] > 0 );
		southEastDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.SOUTH_EAST ] > 0 );
		southDistanceText.gameObject.SetActive    ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.SOUTH      ] > 0 );
		southWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.SOUTH_WEST ] > 0 );
		westDistanceText.gameObject.SetActive     ( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.WEST       ] > 0 );
		northWestDistanceText.gameObject.SetActive( ! nodeReference.isObstacle && nodeReference.jpDistances[ (int) eDirections.NORTH_WEST ] > 0 );
	}

	// Display all Jump Point Distances ( including Wall distances )
	private void dispayAllJumpPointDistances()
	{
		displayJumpPointDistances();
		setJPValuesEnabled( true );
	}

	private void setJumpPointColors()
	{
		northDistanceText.color     = nodeReference.jpDistances[ (int) eDirections.NORTH      ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		northEastDistanceText.color = nodeReference.jpDistances[ (int) eDirections.NORTH_EAST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		eastDistanceText.color      = nodeReference.jpDistances[ (int) eDirections.EAST       ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southEastDistanceText.color = nodeReference.jpDistances[ (int) eDirections.SOUTH_EAST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southDistanceText.color     = nodeReference.jpDistances[ (int) eDirections.SOUTH      ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		southWestDistanceText.color = nodeReference.jpDistances[ (int) eDirections.SOUTH_WEST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		westDistanceText.color      = nodeReference.jpDistances[ (int) eDirections.WEST       ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
		northWestDistanceText.color = nodeReference.jpDistances[ (int) eDirections.NORTH_WEST ] > 0 ? jumpPointDistanceColor : wallDistanceColor;
	}

	// this is the Grid View telling this Node to turn off the State/Stop node positions
	public void removePathMarker()
	{
		isPathEndPoint = false;
		setupDisplay();
	}

	// setup this object all display objects based off the node reference values
	public void setupDisplay()
	{
		if ( nodeReference == null ) return;    // If a Node Reference wasn't given, then don't do anything
		pathEndPoint.gameObject.SetActive( false );

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
				setJumpPointIndicator( false );
				break;
			case eJPSState.ST_PLACE_SEARCH_ENDPOINTS:
			case eJPSState.ST_FIND_PATH:
			case eJPSState.ST_PATH_FIND_COMPLETE:
				GetComponent<SpriteRenderer>().sprite = nodeReference.isObstacle == false ? 
					passableSprite :
					obstacleSprite;

				disableJumpPointArrows(); // make sure jump point arrows are off

				// Disable all the texts
				dispayAllJumpPointDistances();
				setJumpPointColors();
				setJumpPointIndicator( false );

				// If I am a goal sprite, then turn it on yo
				if ( isPathEndPoint )
				{
					pathEndPoint.gameObject.SetActive(true);
				}
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
		if ( nodeReference == null ) return;                                // If a Node Reference wasn't given, then don't do anything
		
		switch ( JPSState.state )
		{
			// if we are in the building state, then yeah let them build
			case eJPSState.ST_OBSTACLE_BUILDING:
				nodeReference.isObstacle = ! nodeReference.isObstacle;    // flip obstacles
				setupDisplay();
				break;
			case eJPSState.ST_PLACE_SEARCH_ENDPOINTS:
				if ( ! nodeReference.isObstacle && ! isPathEndPoint )    // if we are not an obstacle, then let them select this node
				{
					// Mark as one of the Goal
					gridView.markNodeAsPathPoint( this );
					// set visiual indicator FUCK HOW DO I TELL OLD NODES THAT THEY ARE NO LONGER SELECTED?!
					isPathEndPoint = true;
					setupDisplay();
				}
				break;
		}

	}

	// Use this for initialization
	void Start () 
	{
		setupDisplay();
	}
}
