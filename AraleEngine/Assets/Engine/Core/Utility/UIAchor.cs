using UnityEngine;
using System.Collections;

public class UIAchor : MonoBehaviour {
    public enum Achor{
        Center,
        Corner,
        HSide,
        VSide,
    }
    public RectTransform _root;
    public void setPositon(Vector3 pos, Achor achor=Achor.Center)
    {
        switch (achor)
        {
            case Achor.Corner:
                {
                    Vector3 wp = pos;
                    float kx = wp.x < 0 ? 0.5f : -0.5f;
                    float ky = wp.y < 0 ? 0.5f : -0.5f;
                    RectTransform rc = transform as RectTransform;
                    Vector3 v = rc.worldToLocalMatrix.MultiplyPoint(wp);
                    v.x += kx * _root.sizeDelta.x;
                    v.y += ky * _root.sizeDelta.y;
                    _root.localPosition = v;
                    break;
                }
            case Achor.HSide:
                {
                    Vector3 wp = pos;
                    float kx = wp.x < 0 ? 0.5f : -0.5f;
                    RectTransform rc = transform as RectTransform;
                    Vector3 v = rc.worldToLocalMatrix.MultiplyPoint(wp);
                    v.x += kx * _root.sizeDelta.x;
                    _root.localPosition = v;
                    break;
                }
            case Achor.VSide:
                {
                    Vector3 wp = pos;
                    float ky = wp.y < 0 ? 0.5f : -0.5f;
                    RectTransform rc = transform as RectTransform;
                    Vector3 v = rc.worldToLocalMatrix.MultiplyPoint(wp);
                    v.y += ky * _root.sizeDelta.y;
                    _root.localPosition = v;
                    break;
                }
            default:
                _root.position = pos;
                break;
        }
    }
}
