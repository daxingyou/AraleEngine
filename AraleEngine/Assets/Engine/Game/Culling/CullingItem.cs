using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arale.Engine;

public class CullingItem : MonoBehaviour
{
    public float radius;
    public string path;//动态资源加载路径，不设置则会控制根节点显示隐藏
	// Use this for initialization
    void OnEnable()
    {
        if (string.IsNullOrEmpty(path) || transform.childCount>0)return;
		GameObject go = ResLoad.get(path, ResideType.InScene).gameObject();
        go.transform.SetParent(transform, false);
    }

    void OnDisable()
    {
        if (transform.childCount < 1)return;
        GameObject.Destroy(transform.GetChild(0).gameObject);
    }

    #if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
    #endif
}
