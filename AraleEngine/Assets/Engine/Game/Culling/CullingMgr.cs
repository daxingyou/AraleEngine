using UnityEngine;
using System.Collections;

public class CullingMgr : MonoBehaviour {
    public Camera cam;
    CullingGroup  culling;
    CullingItem[] items;
    // Use this for initialization
    void Start () {
        culling = new CullingGroup();
        culling.targetCamera = cam;
        items = GetComponentsInChildren<CullingItem>(true);
        int count = items.Length;
        BoundingSphere[] bs = new BoundingSphere[count];
        for (int i = 0; i < count; ++i)
        {
            CullingItem it = items[i];
            bs[i] = new BoundingSphere(it.transform.position, it.radius); 
        }
        culling.SetBoundingSpheres(bs);
        culling.SetBoundingSphereCount(count);
        culling.onStateChanged = onCullingChanged;
    }

    void onCullingChanged (CullingGroupEvent evt)
    {
        if (evt.hasBecomeInvisible)
        {
            items[evt.index].gameObject.SetActive(false);
        }
        else if(evt.hasBecomeVisible)
        {
            items[evt.index].gameObject.SetActive(true);
        }
    }

    void OnDestroy()
    {
        if (culling == null)return;
        culling.Dispose();
        culling = null;
    }
}
