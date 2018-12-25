using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
        
    public class UGUIDepth : MonoBehaviour
    {
        public enum RenderType
        {
            top,
            bottom,
        }
        public RenderType mRenderType = RenderType.top;
        Canvas mCanvas;
        int mSortOrder;
        // Use this for initialization
        void Start () {
            mCanvas = GetComponentInParent<Canvas>();
            if (!mCanvas) return;
            mSortOrder = mCanvas.sortingOrder;
            Renderer[] rds = GetComponentsInChildren<Renderer>();
            foreach(Renderer r in rds)
            {
                r.sortingOrder = mRenderType == RenderType.top ? mSortOrder + 1 : mSortOrder;
            }
        }
    	
    	// Update is called once per frame
    	void Update () {
            if (!mCanvas) return;
            if(mCanvas.sortingOrder!=mSortOrder)
            {
                mSortOrder = mCanvas.sortingOrder;
                Renderer[] rds = GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rds)
                {
                    r.sortingOrder = mRenderType == RenderType.top ? mSortOrder + 1 : mSortOrder;
                }
            }
        }
    }

}
