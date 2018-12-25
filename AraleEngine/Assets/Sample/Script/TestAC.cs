using UnityEngine;
using System.Collections;
using Arale.Engine;

public class TestAC : MonoBehaviour {
	public Transform transObj;
	public Transform target;
	public string acGroupName;
	public float duration;
	public float k;

	ACMovement acMovement;
	// Use this for initialization
	void Start () {
		if (transObj == null)
			transObj = transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(null!=acMovement)acMovement.Update ();
	}

	void OnGUI() {
		if(GUI.Button(new Rect(0,0,120,30),"play ACMovement"))
		{
			transObj.position=Vector3.zero;
			transObj.rotation=Quaternion.Euler(Vector3.zero);
			acMovement = new ACMovement (ACMovement.MoveType.Centripetence,acGroupName);
			acMovement.target = target;
			acMovement.duration = duration;
			acMovement.trans = transObj;
			acMovement.Play();
		}
	}

	void onOver()
	{
		Destroy (transObj.gameObject);
	}
}
