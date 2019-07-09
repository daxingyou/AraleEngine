using UnityEngine;
using System.Collections;

public class LoopNode : MonoBehaviour
{//一个loop节点必须>=相机视区
    public Vector3 mSize;
    public float   mSpeed;
    Transform[] mLN;//原点左下
    void CloneNode()
    {
        mLN = new Transform[2];//如果是x,z平面则是4个,当前只考虑水平方向循环
        mLN[0] = transform;
        GameObject go = GameObject.Instantiate(gameObject) as GameObject;
        DestroyObject(go.GetComponent<LoopNode>());
        mLN[1] = go.transform;
        mLN[1].SetParent(transform.parent); 
    }

    void LateUpdate()
    {//必须在camera跟新位置后执行否则画面抖动
        if(mSpeed==0)return;
        if (mLN == null)CloneNode();
        if (mSpeed >= 0)
        {
            Vector3 pos = mLN[0].position;
            pos.x += Time.deltaTime * mSpeed;
            mLN[0].position = new Vector3(pos.x % mSize.x, pos.y, pos.z);
            mLN[1].position = mLN[0].position + new Vector3(-mSize.x, 0, 0);
        }
        else
        {
            Vector3 pos = mLN[1].position;
            pos.x += Time.deltaTime * mSpeed;
            mLN[1].position = new Vector3(pos.x % mSize.x, pos.y, pos.z);
            mLN[0].position = mLN[1].position + new Vector3(mSize.x, 0, 0);
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {//对应的脚本在inspector必须为展开状态，否则不会被调用
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, mSize);
    }
    #endif
}
