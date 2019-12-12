//挂到第一个场景相机上,放在第2个场景相机在某些机器上性能会聚降
using UnityEngine;
using System.Collections;
using Arale.Engine;

public class ImageEffect : MonoBehaviour {
    public Material mEffectMat;
    public float mDuration;
    float mTime;
    void OnEnable()
    {
        if(mEffectMat!=null)mEffectMat = Object.Instantiate(mEffectMat);
        mTime = 0;
    }

    public void SetMaterial(string matPath)
    {
        mEffectMat = ResLoad.get(matPath, ResideType.InScene).asset<Material>();
        OnEnable();
    }
	// Use this for initialization
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (mEffectMat == null)return;
        if (mDuration > 0)
        {
            if (mTime < mDuration)
            {
                mTime += Time.unscaledDeltaTime;
                mEffectMat.SetFloat("_Progress", mTime / mDuration);
            }
            else
            {
                mEffectMat.SetFloat("_Progress", 1);
            }
        }
        Graphics.Blit (src, dst, mEffectMat);
    }
}
