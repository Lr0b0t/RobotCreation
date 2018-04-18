using System.Collections;
using System;
using System.Text;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class Core : MonoBehaviour
{

	[SerializeField]
	GameObject[] objects;
	public int currentlySelectedItemIndex;

	public List<GameObject> instantiatedObjects = new List<GameObject>();

	public GameObject focusedObject;

	[SerializeField]
	public int SelectedPrefabIndex;

	[SerializeField]
	CameraControl controller;

	[SerializeField]
	GameObject positionWhereToSpawnObjects;

	[SerializeField]
	Color colorFocused;

	[SerializeField]
	Color colorDefault;

	[SerializeField]
	Material linerFixedMaterial;

	[SerializeField]
	Material linerHingedMaterial;

	[SerializeField]
	Material linerElectronicMaterial;

	public List<GameObject> pivotPoints = new List<GameObject>();

	[SerializeField]
	BrainData brainData;

	[SerializeField]
	LayerMask motorMask;
	[SerializeField]
	LayerMask normalMask;

	[SerializeField]
	InputField codeField;

	[SerializeField]
	Image codingBackground;

	public Button linkHingeButton, linkFixedButton, deleteButton, powerButton, rotateCCWButton, rotateCWButton, lockButton;
	public Image lockImage;

	public GameObject addingJointImage;
	public Text jointTextData;

	public Text ErrorTextField;
	public Animation ErrorAnim;

	public GameObject linerPrefab;
	public Sprite lockedSprite, unlockedSprite;

	public static string RobotName = "defaultname";
	public string DeserializeRobot()
	{
		StringBuilder builder = new StringBuilder();

		foreach (GameObject instantiatedObject in instantiatedObjects)
		{
			builder.Append(instantiatedObject.GetComponent<ExportObjectData>().ToString());
			builder.Append("#");
		}

		if (builder.Length != 0)
		{
			builder.Remove(builder.Length - 1, 1);
		}
		return builder.ToString();
	}

	public void DisplayError(string text) {
		ErrorTextField.text = text;
		ErrorAnim.Play();
	}
	public string DeserializeBrain()
	{
		return brainData.ToString();
	}

	public void LoadRobot() {
		string data = PlayerPrefs.GetString("robotData" + RobotName);
		try
		{
			foreach (string line in data.Split('#'))
			{
				string[] objectParameters = line.Split('|');

				int objectIndex = int.Parse(objectParameters[0]);
				int objectPrefabIndex = int.Parse(objectParameters[1]);
				Vector2 position = Utils.SerializeVector2(objectParameters[2]);
				Quaternion rotation = Utils.SerializeQuaternion(objectParameters[3]);

				GameObject go = Instantiate(objects[objectPrefabIndex], position, rotation);

				ExportObjectData dat = go.AddComponent<ExportObjectData>();
				dat.objectPrefabIndex = objectPrefabIndex;
				dat.objectIndex = objectIndex;

				instantiatedObjects.Add(go);

				//string[] jointIndexes = objectParameters[4].Split(',');
			}
		}
		catch {}

		foreach (string line in data.Split('#'))
		{
			try
			{
				string[] objectParameters = line.Split('|');

				int objectIndex = int.Parse(objectParameters[0]);
				int objectPrefabIndex = int.Parse(objectParameters[1]);
				Vector2 position = Utils.SerializeVector2(objectParameters[2]);
				Quaternion rotation = Utils.SerializeQuaternion(objectParameters[3]);
				string[] jointIndexes = objectParameters[4].Split(',');

				foreach (string jointData in jointIndexes)
				{
					int indexInt = int.Parse(jointData.Split('.')[0]);
					bool isHinged = jointData.Split('.')[1] == "h";

					if (isHinged)
					{
						NonPhysicalJoint joint = instantiatedObjects[objectIndex].AddComponent<NonPhysicalJoint>();
						joint.IsHingedJoint = true;
						joint.SetConnectedObject(instantiatedObjects[indexInt]);
						if (isPausedNow)
						{
							joint.Pause();
						}
						else
						{
							joint.Unpause();
						}

						LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
						liner.materialToSet = linerHingedMaterial;
						liner.attachedJoint = joint;
						liner.object1 = instantiatedObjects[indexInt];
						liner.object2 = instantiatedObjects[objectIndex];
					}
					else
					{
						NonPhysicalJoint joint = instantiatedObjects[objectIndex].AddComponent<NonPhysicalJoint>();
						joint.IsHingedJoint = false;
						joint.SetConnectedObject(instantiatedObjects[indexInt]);
						if (isPausedNow)
						{
							joint.Pause();
						}
						else
						{
							joint.Unpause();
						}

						LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
						liner.materialToSet = linerFixedMaterial;
						liner.attachedJoint = joint;
						liner.object1 = instantiatedObjects[indexInt];
						liner.object2 = instantiatedObjects[objectIndex];
					}
				}
			}
			catch
			{
				continue;
			}
		}

		string brainDataStr = PlayerPrefs.GetString("brainData" + RobotName);
		int i = 0;
		foreach (string objIndexStr in brainDataStr.Split(','))
		{
			int ind = int.Parse(objIndexStr);
			if (ind != -1)
			{
				LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
				liner.connectedPort = i;
				liner.isMotorConnected = true;
				liner.materialToSet = linerElectronicMaterial;
				liner.object1 = brainData.transform.Find("Motor" + i.ToString()).gameObject;
				liner.object2 = instantiatedObjects[ind];

				brainData.connectedMotors[i] = instantiatedObjects[ind];
				instantiatedObjects[ind].GetComponent<ExportObjectData>().connectedPort = i;
			}
			i++;
		}

		string codeData = @"
		-- Code for the robot is written in Lua scripting language
		function start()
			-- Functions to execute on the launch of the robot
		end

		function loop()
			-- Functions to execute repeatedly
			SetMotor(0, GetLeftJoystickX());
			SetMotor(1, GetLeftJoystickX());
		end";

		codeField.text = PlayerPrefs.GetString("codeData" + RobotName, codeData);
	}

	public void CancelJoint() {
		if(!IsNowAddingElectricLink && !IsNowAddingHingedLink && !IsNowAddingRigidLink)
			return;
		IsNowAddingElectricLink = false;
		IsNowAddingHingedLink = false;
		IsNowAddingRigidLink = false;
		hitObjectOnStart = null;
		hitAndObjectCenterDelta = Vector2.zero;
		startTime = 0f;
		addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideOut");
	}

	public void Save()
	{
		PlayerPrefs.SetString("robotData" + RobotName, DeserializeRobot());
		PlayerPrefs.SetString("codeData" + RobotName, codeField.text);
		PlayerPrefs.SetString("brainData" + RobotName, DeserializeBrain());
	}

	public void Play() {
		Save();
		CorePlay.RobotName = RobotName;
		SceneManager.LoadScene(1);
	}
	public void InstantiateObject(int index)
	{
		GameObject spawnedObject = Instantiate(objects[index], Vector2.zero, Quaternion.identity);
		ExportObjectData data = spawnedObject.AddComponent<ExportObjectData>();
		data.objectPrefabIndex = index;
		data.objectIndex = instantiatedObjects.Count;
		instantiatedObjects.Add(spawnedObject);
		try
		{
			BoxCollider2D collider = spawnedObject.GetComponent<BoxCollider2D>();
			Vector2 pos = positionWhereToSpawnObjects.transform.position;
			pos.y += collider.bounds.size.y / 2f;
			spawnedObject.transform.position = pos;
		}
		catch
		{
			CircleCollider2D collider = spawnedObject.GetComponent<CircleCollider2D>();
			Vector2 pos = positionWhereToSpawnObjects.transform.position;
			pos.y += collider.radius;
			spawnedObject.transform.position = pos;
		}

		if (index == 6)
		{
			pivotPoints.Add(spawnedObject);
		}

		SetObjectAsFocused(spawnedObject);
		Camera.main.transform.position = new Vector3(spawnedObject.transform.position.x, spawnedObject.transform.position.y, -10);
	}

	public void RemoveObject()
	{
		foreach(GameObject go in instantiatedObjects) {
			if(go != focusedObject) {
				foreach(NonPhysicalJoint joint in go.GetComponents<NonPhysicalJoint>()) {
					if(joint.connectedObject == focusedObject) {
						Destroy(joint);
					}
				}
			}
		}
		instantiatedObjects.Remove(focusedObject);
		Destroy(focusedObject);

		if (instantiatedObjects.Count != 0)
		{
			SetObjectAsFocused(instantiatedObjects[instantiatedObjects.Count - 1]);
		}
	}

	public void ToggleCoding() {
		codingBackground.gameObject.SetActive(!codingBackground.gameObject.activeSelf);
	}
	bool isPausedNow = false;

	public void PauseToggle()
	{
		isPausedNow = !isPausedNow;
		if (isPausedNow)
		{
			lockImage.sprite = lockedSprite;
		}
		else
		{
			lockImage.sprite = unlockedSprite;
		}

		foreach (GameObject obj in instantiatedObjects)
		{
			NonPhysicalJoint[] joints = obj.GetComponents<NonPhysicalJoint>();
			foreach (NonPhysicalJoint joint in joints)
			{
				if (isPausedNow)
				{
					joint.Pause();
					lockImage.sprite = lockedSprite;
				}
				else
				{
					joint.Unpause();
					lockImage.sprite = unlockedSprite;
				}
			}
		}
	}

	bool IsNowAddingRigidLink = false;
	public void AddRigidLink()
	{
		if (!focusedObject)
		{
			return;
		}

		IsNowAddingRigidLink = true;
		addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideIn");
		jointTextData.text = "Select object to add fixed (rigid) joint";
	}

	bool IsNowAddingElectricLink = false;
	public void AddElectricLink()
	{
		if (!focusedObject)
		{
			return;
		}

		IsNowAddingElectricLink = true;
		addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideIn");
		jointTextData.text = "Select a port on the brain";
	}

	bool IsNowAddingHingedLink = false;
	public void AddHingedLink() {
		if(!focusedObject) {
			return;
		}

		IsNowAddingHingedLink = true;
		addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideIn");
		jointTextData.text = "Select object to add hinge joint";
	}

	public void RotateObject(int direction)
	{
		if (!focusedObject)
		{
			return;
		}
		focusedObject.transform.Rotate(0, 0, 45 * -direction);
	}

	int[,] map = new int[50, 50];

	void Start()
	{
		string codeData = @"
		-- Code for the robot is written in Lua scripting language
		function start()
			-- Functions to execute on the launch of the robot
		end

		function loop()
			-- Functions to execute repeatedly
			SetMotor(0, GetLeftJoystickX());
			SetMotor(1, GetLeftJoystickX());
		end";

		codeField.text = codeData;

		LoadRobot();
	}

	void SetObjectAsFocused(GameObject toFocus)
	{
		if (!instantiatedObjects.Contains(toFocus))
		{
			return;
		}
		if (focusedObject)
		{
			focusedObject.GetComponent<SpriteRenderer>().color = colorDefault;
		}
		focusedObject = toFocus;
		focusedObject.GetComponent<SpriteRenderer>().color = colorFocused;
	}

	void RoundVector(ref Vector3 toRound)
	{
		toRound.x = Mathf.Round(toRound.x * 10f) / 10f;
		toRound.y = Mathf.Round(toRound.y * 10f) / 10f;
		toRound.z = Mathf.Round(toRound.z * 10f) / 10f;
	}
	float startTime = 0f;
	GameObject hitObjectOnStart;
	Vector2 hitAndObjectCenterDelta;
	bool isHitOnFocusedObject = false;

	bool AllowCameraMovement = true;
	void Update()
	{
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			Vector2 touchPosition = touch.position;

			RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(touchPosition), Vector2.zero, normalMask);


			if (touch.phase == TouchPhase.Began)
			{
				//If we just started pressing (1 frame when user clicked)
				if (hit && !IsPointerOverUIObject())
				{
					hitObjectOnStart = hit.transform.gameObject;
					hitAndObjectCenterDelta = hit.transform.position - Camera.main.ScreenToWorldPoint(touchPosition);

					if (focusedObject && focusedObject == hit.transform.gameObject)
					{
						isHitOnFocusedObject = true;
					}
					else
					{
						isHitOnFocusedObject = false;
					}
				}
				else
				{
					hitObjectOnStart = null;
					isHitOnFocusedObject = false;
					hitAndObjectCenterDelta = Vector2.zero;
				}

				startTime = Time.time;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				//If we just ended pressing (1 frame when user releasd his finger)
				if (Time.time - startTime < 0.1f && hit && hitObjectOnStart == hit.transform.gameObject)
				{
					if (IsNowAddingRigidLink)
					{
						bool ok = true;
						foreach(NonPhysicalJoint j in focusedObject.GetComponents<NonPhysicalJoint>()) {
							if(j.connectedObject == hitObjectOnStart) {
								ok = false;
								DisplayError("A joint was already added to this object");
							}
						}
						foreach(NonPhysicalJoint j in hitObjectOnStart.GetComponents<NonPhysicalJoint>()) {
							if (j.connectedObject == focusedObject)
							{
								ok = false;
								DisplayError("A joint was already added to this object");
							}
						}
						if(focusedObject == hitObjectOnStart) {
							ok = false;
							DisplayError("You cannot attach an object to itself");
						}
						if (ok)
						{
							NonPhysicalJoint joint = focusedObject.AddComponent<NonPhysicalJoint>();
							joint.IsHingedJoint = false;
							joint.SetConnectedObject(hit.transform.gameObject);
							if (isPausedNow)
							{
								joint.Pause();
							}
							else
							{
								joint.Unpause();
							}

							LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
							liner.materialToSet = linerFixedMaterial;
							liner.object1 = hit.transform.gameObject;
							liner.attachedJoint = joint;
							liner.object2 = focusedObject;
						}


						hitObjectOnStart = null;
						IsNowAddingRigidLink = false;
						hitAndObjectCenterDelta = Vector2.zero;
						startTime = 0f;
						addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideOut");
					}
					else if(IsNowAddingHingedLink) {
						bool ok = true;
						foreach (NonPhysicalJoint j in focusedObject.GetComponents<NonPhysicalJoint>())
						{
							if (j.connectedObject == hitObjectOnStart)
							{
								ok = false;
								DisplayError("A joint was already added to this object");
							}
						}
						foreach (NonPhysicalJoint j in hitObjectOnStart.GetComponents<NonPhysicalJoint>())
						{
							if (j.connectedObject == focusedObject)
							{
								ok = false;
								DisplayError("A joint was already added to this object");
							}
						}
						if (ok)
						{
							if (focusedObject == hitObjectOnStart)
							{
								ok = false;
								DisplayError("You cannot attach an object to itself");
							}

							NonPhysicalJoint joint = focusedObject.AddComponent<NonPhysicalJoint>();
							joint.SetConnectedObject(hit.transform.gameObject);
							joint.IsHingedJoint = true;
							if (isPausedNow)
							{
								joint.Pause();
							}
							else
							{
								joint.Unpause();
							}

							LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
							liner.materialToSet = linerHingedMaterial;
							liner.object1 = hit.transform.gameObject;
							liner.attachedJoint = joint;
							liner.object2 = focusedObject;
						}

						hitObjectOnStart = null;
						IsNowAddingHingedLink = false;
						hitAndObjectCenterDelta = Vector2.zero;
						startTime = 0f;
						addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideOut");
					}
					else if (IsNowAddingElectricLink && hit.transform.parent && hit.transform.parent.CompareTag("Brain"))
					{
						int connectedIndex = 0;
						bool isMotor = true;
						bool ok = false;
						if (focusedObject.CompareTag("Motor"))
						{
							if (hit.transform.name.Contains("Motor"))
							{
								int motorIndex = hit.transform.name[5] - '0';
								if(hit.transform.parent.GetComponent<BrainData>().connectedMotors[motorIndex] != null) {
									ok = false;
									DisplayError("This port is already occupied");
								}
								else
								{
									hit.transform.parent.GetComponent<BrainData>().connectedMotors[motorIndex] = focusedObject;
									ok = true;
									connectedIndex = motorIndex;
								}
							}
							else
							{
								ok = false;
								DisplayError("You cannot attach a motor to sensor port");
							}
						}
						else if (focusedObject.CompareTag("Sensor"))
						{
							if (hit.transform.name.Contains("Sensor"))
							{
								int sensorIndex = hit.transform.name[6] - '0';
								if (hit.transform.parent.GetComponent<BrainData>().connectedSensors[sensorIndex] != null)
								{
									ok = false;
									DisplayError("This port is already occupied");
								}
								else
								{
									hit.transform.parent.GetComponent<BrainData>().connectedSensors[sensorIndex] = focusedObject;
									ok = true;
									connectedIndex = sensorIndex;
									isMotor = false;
								}
							}
							else
							{
								ok = false;
								DisplayError("You cannot attach a sensor to motor port");
							}
						}

						hitObjectOnStart = null;
						IsNowAddingElectricLink = false;
						hitAndObjectCenterDelta = Vector2.zero;
						startTime = 0f;
						addingJointImage.GetComponent<Animation>().Play("NowAddingPanelSlideOut");

						if (ok)
						{
							focusedObject.GetComponent<ExportObjectData>().connectedPort = connectedIndex;
							LineBetweenTwoObjects liner = Instantiate(linerPrefab, Vector3.zero, Quaternion.identity).GetComponent<LineBetweenTwoObjects>();
							liner.connectedPort = connectedIndex;
							liner.isMotorConnected = isMotor;
							liner.materialToSet = linerElectronicMaterial;
							liner.object1 = hit.transform.gameObject;
							liner.object2 = focusedObject;
						}
					}
					else
					{
						hitObjectOnStart = null;
						SetObjectAsFocused(hit.transform.gameObject);
						hitAndObjectCenterDelta = Vector2.zero;
						startTime = 0f;
					}
				}
				isHitOnFocusedObject = false;

				AllowCameraMovement = true;
			}
			else
			{
				//If we are moving our finger
				if (isHitOnFocusedObject)
				{
					Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPosition);
					worldPos.z = 0;

					/*bool pivoted = false;
					foreach (GameObject pivot in pivotPoints)
					{
						//Debug.Log((pivot.transform.position - worldPos).magnitude);
						if (pivot != focusedObject && (pivot.transform.position - worldPos).magnitude < 0.5f && pivot.GetComponent<ExportObjectData>().hingedObject == null)
						{
							pivot.GetComponent<ExportObjectData>().hingedObject = focusedObject;
							focusedObject.GetComponent<ExportObjectData>().hingedObject = focusedObject;
							pivoted = true;
							worldPos = pivot.transform.position + (Vector3)hitAndObjectCenterDelta;
							break;
						}
					}*/
					worldPos += (Vector3)hitAndObjectCenterDelta;
					RoundVector(ref worldPos);
					worldPos.z = 0;
					focusedObject.transform.position = worldPos;
					AllowCameraMovement = false;
				}
			}
		}

		if (AllowCameraMovement && !IsPointerOverUIObject())
		{
			controller.UpdateCamera();
		}

		if(focusedObject == null) {
			linkHingeButton.interactable = false;
			linkFixedButton.interactable = false;
			deleteButton.interactable = false;
			rotateCCWButton.interactable = false;
			rotateCWButton.interactable = false;
			powerButton.interactable = false;
		}
		else {
			linkHingeButton.interactable = true;
			linkFixedButton.interactable = true;
			deleteButton.interactable = true;
			rotateCCWButton.interactable = true;
			rotateCWButton.interactable = true;

			if (focusedObject.GetComponent<ExportObjectData>().connectedPort == -1)
			{
				powerButton.interactable = true;
			}
			else {
				powerButton.interactable = false;
			}
		}
	}

	private bool IsPointerOverUIObject()
	{
		PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
		return results.Count > 0;
	}
}
