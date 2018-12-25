#if USE_NGUI
using UnityEngine;
using System.Collections.Generic;

public class NGUIDepth : MonoBehaviour {

    public enum RenderType
    {
        top,
        bottom,
    }

    public bool m_KeepOneRenderQ = true;
    public RenderType m_RenderType = RenderType.top;
    public int m_Gain;//增量
    public bool m_UpdateNow;

    UIPanel mPanel;
    int mPanelStartRenderQ;
    int mPanelEndRenderQ;
    RenderType mRType;

    public bool m_CheckPanel = true;
    int mMinRenderQ;
    //int mDeltaRenderQ;
    List<Material> mMaterials = new List<Material>();
    public void FindPanelNow()
    {
        m_CheckPanel = true;
        mPanel = null;
    }

    class MRenderQCompare : IComparer<Material>
    {
        public int Compare(Material a, Material b)
        {
            return a.renderQueue - b.renderQueue;
        }
    }

    bool FindPanel()
    {
        if (mPanel)
            return true;
        if (!m_CheckPanel)
            return false;
        m_CheckPanel = false;
        Transform t = transform.parent;
        while (t)
        {
            mPanel = t.GetComponent<UIPanel>();
            if (mPanel)
                return true;
            t = t.parent;
        }
        return false;
    }

    void OnEnable()
    {
        m_UpdateNow = true;
    }

    void OnDestroy()
    {
        mPanel = null;
    }

    void Start()
    {

        FindMaterials();
        m_CheckPanel = true;
        FindPanel();
        //if (m_UpdateWhenStart && FindPanel())
        //{
        //    mPanelStartRenderQ = mPanel.startingRenderQueue;
        //    mPanelEndRenderQ = mPanelStartRenderQ + mPanel.drawCalls.size;
        //    mRType = m_RenderType;
        //    UpdateRenderQ();
        //}
    }

    public void FindMaterials()
    {
        Transform[] trans = GetComponentsInChildren<Transform>(true);
        mMaterials.Clear();
        for (int i = 0; i < trans.Length; i++)
        {
            Material[] m = trans[i].GetComponent<Renderer>() ? trans[i].GetComponent<Renderer>().materials : null;
            if (null != m)
            {
                for (int j = 0; j < m.Length; ++j)
                {
                    if (m[j] && !mMaterials.Contains(m[j]))
                    {
                        mMaterials.Add(m[j]);
                    }
                }
            }
        }
        mMaterials.Sort(new MRenderQCompare());
        int lastRenderQ = 0;
        int resetRenderQ = 0;
        for (int i = 0; i < mMaterials.Count; i++)
        {
            Material m = mMaterials[i];
            int n = m.renderQueue;
            if (i == 0)
            {
                mMinRenderQ = n;
                resetRenderQ = n;
            }
            else
            {
                if (n > lastRenderQ)
                    resetRenderQ++;
                m.renderQueue = resetRenderQ;
            }
            lastRenderQ = n;
            //             if (i == mMaterials.Count - 1)
            //                 mDeltaRenderQ = lastRenderQ - mMinRenderQ;
        }
    }

    void UpdateRenderQ()
    {
        int min = mRType == RenderType.top ? mPanelEndRenderQ : mPanelStartRenderQ;

        for (int i = mMaterials.Count - 1; i >= 0; i--)
        {
            Material m = mMaterials[i];
            if (m)
            {
                int delta = m_KeepOneRenderQ ? 0 : (m.renderQueue - mMinRenderQ);
                m.renderQueue = min + delta + m_Gain;
            }
            else
            {
                mMaterials.RemoveAt(i);
            }                                                                         
        }
        mMinRenderQ = min;
    }

    void Update()
    {
        if (FindPanel())
        {
            int start = mPanel.startingRenderQueue;
            int end = start + mPanel.drawCalls.size;
            bool changed = m_UpdateNow;
            if (!changed)
            {
                changed |= start != mPanelStartRenderQ || end != mPanelEndRenderQ;
                changed |= (mRType != m_RenderType);
            }
            if (changed)
            {
                mPanelStartRenderQ = start;
                mPanelEndRenderQ = end;
                mRType = m_RenderType;
                m_UpdateNow = false;
                UpdateRenderQ();
            }
        }

    }
}
#endif