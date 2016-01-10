using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

// Control mouse zoom and click and drag
public class MouseMoveControler : MonoBehaviour 
{
	[Header("Mouse Zoom")]
	[SerializeField] private float _zoomSpeed = 5f;
	[SerializeField] private float _zoomSensitivity = 3f;
	[SerializeField, Range(1.0f, 100f)] private float _sizeMin = 1.0f;
	[SerializeField, Range(1.0f, 100f)] private float _sizeMax = 20.0f;

    private Vector3 _dragOrigin;
	private IEnumerator _zoomCoroutine = null;

	// Update is called once per frame
	void Update () 
	{
		CheckForMouseZoom();
		CheckForMouseClickAndDrag();
	}

	void CheckForMouseZoom()
	{
		float wheel = Input.GetAxis("Mouse ScrollWheel");

		if ( ! Mathf.Approximately( wheel, 0 ) )
		{
			// Disable current zoroutine if it existst
			if ( _zoomCoroutine != null )
			{
				StopAllCoroutines();

				_zoomCoroutine = null;
			}

			// Start zoom coroutine
			Camera cam = Camera.main;

			Debug.Assert( _sizeMin < _sizeMax, 
				"Size Ranges on MouseMoveController are bunk: Min = " + _sizeMin + "  Max = " + _sizeMax + " makes no fucking sense" );

			float new_size = Mathf.Clamp(
				-wheel * _zoomSensitivity + cam.orthographicSize,
				_sizeMin,
				_sizeMax
			);

			StartCoroutine( _zoomCoroutine = lerpToSize( new_size ) );
		}
	}

	// So this is the click and draw functionality
	// If I click my mouse and start to move it, the ScreenToWorldPoint of my mouse position SHOULD ALWAYS BE THE SAME
	// 
	void CheckForMouseClickAndDrag()
	{
		Camera cam = Camera.main;

		// Check for initial Mouse Click
		if ( Input.GetMouseButtonDown( 1 ) )
		{
			_dragOrigin = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		}

		// If mouse isn't down, then stop this whole nonsense
		if ( Input.GetMouseButton( 1 ) )
		{
			Vector3 diff = cam.ScreenToWorldPoint( Input.mousePosition ) - _dragOrigin;
			cam.transform.position -= diff;
		}
	}

	IEnumerator lerpToSize( float target_size )
	{
		while ( true )
		{
			Camera cam = Camera.main;

			// If we've reached the end, then stop
			if ( Mathf.Approximately( target_size, cam.orthographicSize ) ) break;

			cam.orthographicSize = Mathf.Lerp( cam.orthographicSize, target_size, Time.deltaTime * _zoomSpeed );

			yield return null;
		}
	}
}
