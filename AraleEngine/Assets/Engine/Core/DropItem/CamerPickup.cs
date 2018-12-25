/*using UnityEngine;
using System.Collections;

public class CamerPickup : MonoBehaviour {

    private Camera _main_camera = null;
    private Camera _ui_camera = null;
    private GameObject _touch_track = null;

    void Awake()
    {
        _main_camera = Camera.main;
        _ui_camera = NGUITools.FindCameraForLayer(GameDefines.layer_ui);
        _touch_track = ResManager.single.loadSync("Battle/TouchTrack");
        _touch_track.SetActive(false);
    }

	// Update is called once per frame
	void Update () 
    {
		if(UICamera.hoveredObject!=null || Input.touchCount > 1)
		{//防止ngui事件穿透/
			return;
		}

        // 竞技场战斗回放不能进行操作
		if (Input.GetMouseButtonDown (0) && null != _main_camera)
		{
            Ray ray = _main_camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
            LayerMask mask = 1 << GameDefines.layer_shadow_object | 1 << GameDefines.layer_actor;
            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask.value))
            {
                GameObject go = hitInfo.collider.gameObject;
                if (null != go)
                {
                    BattleSceneManager.GetInstance().OnActorClick(go);
                }
            }

            _touch_track.SetActive(true);
		}

        if (Input.GetMouseButtonUp(0))
        {
            _touch_track.SetActive(false);
        }

        if (Input.GetMouseButton(0) && null != _main_camera)
		{
			if (0 != DropManager.single.dropItems.Count)
			{
				Ray ray = _main_camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				LayerMask mask = 1 << GameDefines.layer_drop;
				if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask.value))
				{
					GameObject go = hitInfo.collider.gameObject;
					if (go != null)
					{
						DropManager.single.pickup(go.GetComponent<DropItem>());
					}
				}
			}

            // 绘制手指触摸的轨迹，避免暂停的时候还继续绘制
            if (Time.timeScale != 0)
            {
                Vector3 pos = _ui_camera.ScreenToWorldPoint(Input.mousePosition);
                pos.z = 1.0f;
                _touch_track.transform.position = pos;
            }
		}
	}
}
*/