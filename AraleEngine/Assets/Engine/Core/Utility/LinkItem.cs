using UnityEngine;
using System.Collections;

public class LinkItem : MonoBehaviour {
    public float _unlinkval;
    public Transform _link;
	void Update ()
    {
        if (_link == null)return;
        Vector3 v = transform.localPosition;
        if (v.y<_unlinkval)
        {
            _link.transform.position = transform.position;
        }
        else
        {
            v.y = _unlinkval;
            Vector3 v1 = transform.localPosition;
            transform.localPosition = v;
            _link.transform.position = transform.position;
            transform.localPosition = v1;
        }
	}
}
