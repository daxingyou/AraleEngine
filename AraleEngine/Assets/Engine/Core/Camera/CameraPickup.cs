using UnityEngine;
using System.Collections;

namespace Arale.Engine
{

    public class CameraPickup : MonoBehaviour
    {
        private Camera mCamera = null;
    	public delegate void OnSelGameObject(GameObject go);
    	OnSelGameObject mOnSelGameObject;
    	// Update is called once per frame
    	void Start()
    	{
    		mCamera = GetComponent<Camera> ();
    	}

    	void Update () 
        {
    		if (mCamera == null)return;
    		if (!Input.GetMouseButtonDown(0))return;
    		Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);
    		RaycastHit hitInfo;
    		LayerMask mask = 1 << LayerMask.NameToLayer("Default");
    		if (!Physics.Raycast (ray, out hitInfo, Mathf.Infinity, mask.value))return;
    		GameObject go = hitInfo.collider.gameObject;
    		if (go == null)return;
    		Debug.Log("pick:"+go.name);
    		if (mOnSelGameObject != null)mOnSelGameObject (go);
    	}

    	public void setCamera(Camera cam, OnSelGameObject onSelCallback)
    	{
    		mCamera = cam;
    		mOnSelGameObject = onSelCallback;
    	}
    }

}
