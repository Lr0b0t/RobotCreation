using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class BrainData : MonoBehaviour {
	public GameObject[] connectedMotors = new GameObject[8];
	public GameObject[] connectedSensors = new GameObject[8];

	public override string ToString(){
		StringBuilder builder = new StringBuilder();

		foreach(GameObject motor in connectedMotors) {
			if (motor != null)
			{
				builder.Append(motor.GetComponent<ExportObjectData>().objectIndex);
			}
			else {
				builder.Append(-1);
			}
			builder.Append(",");
		}
		builder.Remove(builder.Length - 1, 1);

		return builder.ToString();
	}
}
