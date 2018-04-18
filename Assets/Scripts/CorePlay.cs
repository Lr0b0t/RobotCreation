using System.Collections;
using System;
using System.Text;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Linq;
using MoonSharp.Interpreter;

public class CorePlay : MonoBehaviour {

	public static string RobotName;
	public static string robotData;
	public static string brainData;
	public static string codeData;
	public static float[] valuesSensor = new float[8];
	public GameObject textObj;
	[SerializeField]
	GameObject[] objects;

	GameObject[] motors = new GameObject[10];

	public List<GameObject> instantiatedObjectList = new List<GameObject>();
	public List<int> instantiatedObjectPrefabIndex = new List<int>();

	public void Serialize(string data) {
		foreach(string line in data.Split('#')) {
			string[] objectParameters = line.Split('|');

			int objectIndex = int.Parse(objectParameters[0]);
			int objectPrefabIndex = int.Parse(objectParameters[1]);
			Vector2 position = Utils.SerializeVector2(objectParameters[2]);
			Quaternion rotation = Utils.SerializeQuaternion(objectParameters[3]);

			GameObject go = Instantiate(objects[objectPrefabIndex], position, rotation);
			instantiatedObjectList.Add(go);
			instantiatedObjectList[objectIndex].AddComponent<Rigidbody2D>();
			instantiatedObjectPrefabIndex.Add(objectPrefabIndex);
			//string[] jointIndexes = objectParameters[4].Split(',');
		}

		foreach (string line in data.Split('#'))
		{
			string[] objectParameters = line.Split('|');

			int objectIndex = int.Parse(objectParameters[0]);
			int objectPrefabIndex = int.Parse(objectParameters[1]);
			Vector2 position = Utils.SerializeVector2(objectParameters[2]);
			Quaternion rotation = Utils.SerializeQuaternion(objectParameters[3]);

			try
			{
				string[] jointIndexes = objectParameters[4].Split(',');

				foreach (string jointData in jointIndexes)
				{
					int indexInt = int.Parse(jointData.Split('.')[0]);
					bool isHinged = jointData.Split('.')[1] == "h";

					if(isHinged) {
						instantiatedObjectList[objectIndex].AddComponent<HingeJoint2D>().connectedBody = instantiatedObjectList[indexInt].GetComponent<Rigidbody2D>();
						instantiatedObjectList[objectIndex].GetComponent<HingeJoint2D>().useMotor = true;
					}
					else
					{
						instantiatedObjectList[objectIndex].AddComponent<FixedJoint2D>().connectedBody = instantiatedObjectList[indexInt].GetComponent<Rigidbody2D>();
					}
				}
			}
			catch {
				continue;
			}
		}
	}

	void SetMotor(int pin, float speed)
	{
		Debug.Log(pin + " | " + speed);
		try
		{
			if (motors[pin])
			{

				JointMotor2D motor = motors[pin].GetComponent<HingeJoint2D>().motor;
				motor.motorSpeed = speed * 200f;
				motors[pin].GetComponent<HingeJoint2D>().motor = motor;
			}
		}
		catch {
			return;
		}
	}

	void PrintText(string textVal)
	{
		textObj.GetComponent<Text> ().text = textVal;
	}

	[SerializeField]
	VirtualJoystick leftJoystick, rightJoystick;

	float GetSensorValue(int pin)
	{
		if (valuesSensor [pin] != null) {
			Debug.Log (valuesSensor [pin]);
			return valuesSensor [pin];
		} else {
			return 0;
			Debug.Log (0);
		}
	}

	float GetLeftJoystickX() {
		return -leftJoystick.InputVector.x;
	}
	float GetLeftJoystickY()
	{
		return -leftJoystick.InputVector.z;
	}
	float GetRightJoystickX()
	{
		return -rightJoystick.InputVector.x;
	}
	float GetRightJoystickY()
	{
		return -rightJoystick.InputVector.z;
	}

	Script brainScript;
	void Start() {
		robotData = PlayerPrefs.GetString("robotData" + RobotName);
		codeData = PlayerPrefs.GetString("codeData" + RobotName);
		brainData = PlayerPrefs.GetString("brainData" + RobotName);


		/*codeData = @"
		function start()
		end

		function loop()
			SetMotor(0, GetLeftJoystickX());
			SetMotor(1, GetLeftJoystickX());
		end";*/

		Debug.Log(robotData);
		Serialize(robotData);

		int i = 0;
		foreach(string objIndexStr in brainData.Split(',')) {
			int ind = int.Parse(objIndexStr);
			if (ind != -1)
			{
				motors[i] = instantiatedObjectList[ind];
			}
			i++;
		}

		brainScript = new Script();
		brainScript.DoString(codeData);

		brainScript.Globals["SetMotor"] = (Action<int, float>)SetMotor;
		brainScript.Globals["GetSensorValue"] = (Func<int, float>)GetSensorValue;
		brainScript.Globals["GetLeftJoystickX"] = (Func<float>)GetLeftJoystickX;
		brainScript.Globals["GetLeftJoystickY"] = (Func<float>)GetLeftJoystickY;
		brainScript.Globals["GetRightJoystickX"] = (Func<float>)GetRightJoystickX;
		brainScript.Globals["GetRightJoystickY"] = (Func<float>)GetRightJoystickY;
		brainScript.Globals["PrintText"] = (Action<string>)PrintText;
	

		brainScript.Call(brainScript.Globals["start"]);
	}

	void Update()
	{
		brainScript.Call(brainScript.Globals["loop"]);
	}


	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}

	public void Back() {
		SceneManager.LoadScene(0);
	}
}
