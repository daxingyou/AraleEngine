using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;

public class UIGray : MonoBehaviour {
	public bool _gray;
	public bool _grayChilds;
	// Use this for initialization
	Material _grayMat;
	void Start () {
		_grayMat = ResLoad.get("Mat/GrayMat",ResideType.InGame ).asset<Material>();
		InvokeRepeating ("checkSate", 0, 0.1f);
	}

	void checkSate()
	{
		if (_grayChilds)
		{
			MaskableGraphic[] gs = GetComponentsInChildren<MaskableGraphic> (true);
			for (int i = 0, max = gs.Length; i < max; ++i)gs [i].material = _gray ? _grayMat : null;
		}
		else
		{
			MaskableGraphic g = GetComponent<MaskableGraphic> ();
			g.material = _gray ? _grayMat : null;
		}
	}
}
