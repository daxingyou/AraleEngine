//一个可以从中间向两边滑的slider
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UISlider2 : Slider {
    RectTransform mfill;//rect设置为铺满
	// Use this for initialization
	void Start () {
        base.Start();
        minValue = -1;
        maxValue = 1;
        value = 0;
        mfill = this.fillRect;
        this.fillRect = null;
        onDrawSliderFill(value);
        this.onValueChanged.AddListener(onDrawSliderFill);
	}

    void onDrawSliderFill(float f)
    {
        Rect size = (mfill.parent as RectTransform).rect;
        float halfW = 0.5f * size.width;
        mfill.offsetMin = new Vector2(f>0?halfW:(halfW+f*halfW), 0);
        mfill.offsetMax = new Vector2(f>0?halfW+f*halfW-size.width:halfW-size.width, 0);
    }
}
