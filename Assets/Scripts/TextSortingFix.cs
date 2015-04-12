using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TextMesh))]
[ExecuteInEditMode]
public class TextSortingFix : MonoBehaviour
{
	public string sortingLayerName = "Default";
	public int sortingOrder = 0;

	// Use this for initialization
	void Start () 
	{
		GetComponent<Renderer>().sortingLayerName = this.sortingLayerName;
		GetComponent<Renderer>().sortingOrder     = this.sortingOrder;
	}

	void Update()
	{
		GetComponent<Renderer>().sortingLayerName = this.sortingLayerName;
		GetComponent<Renderer>().sortingOrder     = this.sortingOrder;
	}
}
