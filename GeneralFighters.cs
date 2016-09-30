using UnityEngine;
using System.Collections;

public class GeneralFighters : Ship {
	[Header("Components")]
	public Rigidbody rb;

	[Header("Faction")]
	public bool factionEarth;//from Earth Republic....
	public bool factionMars;//from mars republic
	public bool factionPirates;//from Astroid Pirates/bandits
	public bool factionOther;//if none, they are alone, will not fire unless fired apon
	string[] allTags;
	string[] tempAllTags;
	string myTag;

	[Header("AIMovement")]
	public float turnStrength = 0.5f;
	GameObject targetObject;
	Vector3 targetPos;
	public float speed;

	[Header("Ship Stats")]
	public float maxSensorRange;

	[Header("Random Moving Points")]
	public Vector3[] randomPoints;
	public float minDist = 1f;
	public float maxDist = 100f;
	public bool followPath;
	public int pathI;
	public bool lookAtTarget;
	public float targetDist;
	public Vector3 startPos;

	[Header("Status: 0-pathfollowing, 1-AttackTarget, 2-Evade, 3-Returntobase")]
	[Range(0, 3)]
	public int status;

	[Header("Mood: 0-neutral, 1-enemy, 2-ally, 3-Companion, 4-Super Aggressive")]
	[Range(0, 4)]
	public int mood;

	[Header("LeadShip Values")]
	public GameObject leadShipObject;
	public float collisionDist = 15f;

	void Start () {
		maxSensorRange = 100f;
		CalculateRandomPoints ();
		rb = this.gameObject.GetComponent<Rigidbody> ();
		startPos = this.gameObject.transform.position;
	}

	void Update () {
		//////////////////Temp Moving Thrust
		rb.AddForce(transform.right*speed);
		//////////////////Temp Moving Thrust
		myTag = this.gameObject.tag;
		//check if i have a lead ship near me, if not then ill become the lead ship
		leadShipObject = AreThereAnyLeadShips();
		if (leadShipObject == null) {
			leadShipObject = this.gameObject;
			leadShip = 1;
		}

		//overall find the general target distance if the target is not null/empty
		if (targetPos != null) {
			float targetDist = Vector3.Distance (transform.position, targetPos);
		}

		//check we dont exeed the pathI array lenth
		if(pathI>=7){
			pathI=0;
		}

		//Check What faction im in
		if (factionEarth) {
			factionMars = false;
			factionPirates = false;
			factionOther = false;
			allTags = new string[] { "mars", "pirates", "other" };
		} else if (factionMars) {
			factionEarth = false;
			factionPirates = false;
			factionOther = false;
			allTags = new string[] { "earth", "pirates", "other" };

		} else if (factionPirates) {
			factionEarth = false;
			factionMars = false;
			factionOther = false;
			allTags = new string[] { "mars", "earth", "other" };

		} else if (factionOther) {
			factionEarth = false;
			factionMars = false;
			factionPirates = false;
			allTags = new string[] { "mars", "pirates", "earth" };

		} else {
			factionOther = true;
		}

		//Check Status
		if (status == 0) {
			speed = 20f;
			followPath = true;

		} else if (status == 1) {
			speed = 30f;
			followPath = false;
			targetObject = FindClosestTarget(allTags);
			targetPos = targetObject.transform.position;
			targetlocked = targetObject;
			if (mood == 0) {
				//neutral mood
				//will not attack player, if attacked will fleee/retreat
			}else if (mood == 1) {
				targetObject = FindClosestTarget(allTags);
				//aggressive mood, will attack Player on sighter/in range if retreat will stop
				if (targetDist <= maxSensorRange) {
					//look at target
					lookAtTarget=true;
					//check if ray cast hit the target
					RaycastHit hit;
					if (Physics.Raycast (transform.position, -Vector3.forward, out hit)) {
						if(hit.distance<=maxSensorRange){
							//fire
						}
					}
				}
			}else if (mood == 2) {
				//ally mood
				//will attack Enemy Off friendly, will aid player further. 
				targetObject = leadShipObject.GetComponent<Ship>().targetlocked;
				if (targetDist <= maxSensorRange) {
					//look at target
					lookAtTarget=true;
					//check if ray cast hit the target
					RaycastHit hit;

					if (Physics.Raycast (transform.position, -Vector3.forward, out hit)) {
						if(hit.distance<=maxSensorRange){
							//fire
						}
					}
				}
				if (targetDist < collisionDist) {
					speed = targetObject.GetComponent<Rigidbody> ().velocity.magnitude;
				}
			}else if (mood == 3) {
				//companion mood
				//will attack enemy of player, will aid player, give resources.
				mood = 2;
			}else if (mood == 4) {
				//super aggressive mood
				//will attack Player where ever, even if retreat will still follow and attack.
				mood =1;
			}

		} else if (status == 2) {
			followPath = false;
			//play random evade animation
			status = 0;
		} else if (status == 3) {
			followPath = false;
			//find original start position (startPos), create points around that and follow them.for now just go and followpath
			status = 0;
		} else {
			followPath = false;
		}
			

		if (followPath) {
			targetPos = randomPoints [pathI];
			float targetDist = Vector3.Distance (transform.position, targetPos);
			lookAtTarget = true;
			if (targetDist <= 40f) {
				pathI++;
			}
		}
			
		if (lookAtTarget) {
			//Lookat a PathPoint
			float str = Mathf.Min (turnStrength * Time.deltaTime, 1);
			Quaternion newRotation = Quaternion.LookRotation (targetPos - transform.position);
			newRotation *= Quaternion.Euler (transform.position.x - targetPos.x, -90, 0); // this add a 90 degrees Y rotation
			transform.rotation = Quaternion.Slerp (transform.rotation, newRotation, str);
		}
	}

	void CalculateRandomPoints(){
		randomPoints [0] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [1] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [2] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [3] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [4] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [5] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [6] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
		randomPoints [7] = new Vector3 (targetPos.x + Random.Range (minDist, maxDist), targetPos.y + Random.Range (minDist, maxDist), targetPos.z + Random.Range (minDist, maxDist));
	}

	GameObject FindClosestTarget(string[] targetTags){
		ArrayList gos;
		gos = FindGameObjectsWithTags (targetTags);
		GameObject closest = null;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
			foreach (GameObject go in gos) {
				Vector3 diff = go.transform.position - position;
				float curDistance = diff.sqrMagnitude;
				if (curDistance < distance) {
					closest = go;
					distance = curDistance;
				}
			}
		return closest;
	}

	GameObject AreThereAnyLeadShips(){
		GameObject[] gos;
		gos = GameObject.FindGameObjectsWithTag (myTag);
		GameObject theLeadShip = null;
		foreach (GameObject go in gos) {
			int ifLeadShip = go.GetComponent<Ship> ().leadShip;
			if (ifLeadShip == 1) {
				float leadShipDist = Vector3.Distance (transform.position, go.transform.position);
				if (leadShipDist <= maxSensorRange) {
					theLeadShip = go;
					return theLeadShip;
				} else {
					return null;
				}
			} else {
				return null;
			}
		}
		if (theLeadShip == null) {
			return null;
		} else {
			return theLeadShip;
		}
	}

	ArrayList FindGameObjectsWithTags(string[] tags){
		ArrayList combinedList = new ArrayList();
		for (int i = 0; i < tags.Length; i++) {
			GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag (tags [i]);
			combinedList.AddRange (taggedObjects);
		}
		return combinedList;
	}
}
