using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HeadInfo : MonoBehaviour
{
	public Vector3 mOffset;
	public Transform mTarget;
	public Text mName;
	RectTransform mRT;
	//不同角色高度修正值
	void Start()
	{
		mRT = GetComponent<RectTransform> ();
	}

	void Update()
	{
		if(mTarget==null)return;
		/*Vector3 tPos = mTarget .position;
		Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, tPos);
		Vector3 v;
		RectTransformUtility.ScreenPointToWorldPointInRectangle (transform.parent as RectTransform, pos, Camera.main, out v);
		v.z = v.z + (tPos - Camera.main.transform.position).magnitude / 1000;
		mRT.position = v;
		mRT.localPosition = mRT.localPosition + mOffset;*/
		mRT.position = mTarget.position+new Vector3(0,3,0);
	}

}
