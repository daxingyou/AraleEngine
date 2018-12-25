using UnityEngine;
using System.Collections;

public class ScreenWaterDropEffect : MonoBehaviour {
	public Material mEffectMat;
	float mTime;
	// Use this for initialization
	void Start () {
	}

	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (mEffectMat == null)
			return;
		mTime += Time.deltaTime;
		if (mTime > 100)
			mTime -= 100;
		mEffectMat.SetFloat ("_CurTime", mTime);
		Graphics.Blit (src, dst, mEffectMat);
	}
}
