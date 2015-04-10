using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum eDirections
{
	DIR_NORTH,
	DIR_NORTH_EAST,
	DIR_EAST,
	DIR_SOUTH_EAST,
	DIR_SOUTH,
	DIR_SOUTH_WEST,
	DIR_WEST,
	DIR_NORTH_WEST,
}

[RequireComponent(typeof(Collider2D))]
public class BlockScript : MonoBehaviour
{
	public bool isJumpPoint = false;
	public bool isObstacle = false;

	public Sprite passableSprite = null;
	public Sprite obstacleSprite = null;

	public Dictionary< eDirections, int > wallDistances = new Dictionary<eDirections, int>(8);

	void setSprite()
	{
		GetComponent<SpriteRenderer>().sprite = isObstacle == false ? 
			GetComponent<SpriteRenderer>().sprite = passableSprite :
			GetComponent<SpriteRenderer>().sprite = obstacleSprite;
	}

	void OnMouseDown()
	{
		isObstacle = !isObstacle;
		setSprite();
	}

	// Use this for initialization
	void Start () 
	{
		setSprite();
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
