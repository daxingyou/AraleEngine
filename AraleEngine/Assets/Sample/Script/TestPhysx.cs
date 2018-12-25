using UnityEngine;
using System.Collections;

public class TestPhysx : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{
		#if UNITY_EDITOR
		float y = 0;
		if (GUI.Button (new Rect (0, y, 200, 50), "使用力"))
		{
			GameObject go = UnityEditor.Selection.activeGameObject;
			Rigidbody rd = go.GetComponent<Rigidbody> ();
			rd.AddForce (new Vector3 (0, 98, 0), ForceMode.Force);
		}

		if (GUI.Button (new Rect (0, y+=50, 200, 50), "使用扭距矩"))
		{
			GameObject go = UnityEditor.Selection.activeGameObject;
			Rigidbody rd = go.GetComponent<Rigidbody> ();
			rd.AddTorque(new Vector3 (0, 98, 0), ForceMode.Force);
		}
		#endif
	}
}
