using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sensor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		int pin = gameObject.GetComponent<ExportObjectData> ().connectedPort;


		RaycastHit2D hit = Physics2D.Raycast (transform.position, Vector2.right);
		if (hit.collider != null) {
			float distance = Mathf.Abs (hit.point.x - transform.position.x);
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector2.right) * hit.distance, Color.yellow);
			Debug.Log (distance);
			CorePlay.valuesSensor [pin] = distance; 
		}


	}
}
