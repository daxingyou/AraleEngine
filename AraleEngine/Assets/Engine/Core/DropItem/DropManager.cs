/*using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DropManager 
{
    public float screen_ratio = 1.0f; // 屏幕比例
    public GameObject _drop_root = null;
	public List<DropItem> dropItems = new List<DropItem>();
    public static DropItem show_finger_object = null;
	public enum DropType
	{
		Equip,
		Money,
	};

	public class ItemData{
		public int id;
		public int num;
		public DropType type;
		public ItemData(int _id, int _num, DropType _type){
			id = _id;
			num = _num;
			type = _type;
		}
	};

	Transform transform;
	Transform mount;

	static DropManager mThis = null;
	public void init() {
		_drop_root = new GameObject ("Drops");
        _drop_root.layer = GameDefines.layer_drop;
        GameObject.DontDestroyOnLoad(_drop_root);
        transform = _drop_root.transform;
		mount = GHelper.getSubGameObjectByPath (GRoot.single.WinRoot, "DropRoot").transform;

        screen_ratio = Screen.width / 1.777f / Screen.height;
	}
	
	public void deinit(){
	}
	
	public static DropManager single{
		get{
			if(null!=mThis)return mThis;
			return mThis=new DropManager();
		}
	}

	public void clear()
	{
		dropItems.Clear ();
        if (null != _drop_root)
        {
            GHelper.destroyChilds(_drop_root.transform);
        }
        if (mount.childCount>0)
        {
            GHelper.destroyChilds(mount);
        }
	}

	public void pickup(DropItem dropItem)
	{
		if (null == dropItem)
			return;

        if (GGameLogic.m_FightFBType == GameDefines.FB_ARENA_PVP)
            ArenaProcessManager.Singleton.SendMessage_PickupSoul(dropItem.data.id);
        else
        {
            SEToolkit.PlayPickDropItemSE(dropItem);
            dropItem.pick();
        }
	}

    public void ArenaPickup(int id)
    {
        for (int i = 0; i < dropItems.Count; ++i)
        {
            if (dropItems[i].data.id == id)
            {
                dropItems[i].ArenaPickup();
                break;
            }
        }
    }

	public void pickupAll()
	{
		for (int i = dropItems.Count - 1; i >= 0; --i) 
        {
			DropItem d = dropItems[i];

            SEToolkit.PlayPickDropItemSE(d);
            if (d.enabled == false)
            {
                pickup(d);
            }
        }
	}
	//情景切换时,防止灵韵消失/
	public void pause()
	{
		for (int i=0; i<dropItems.Count; ++i) {
			DropItem d = dropItems[i];
			if(d!=null&&d.data.type == DropType.Soul)
			{
				d.enabled = false;
			}
		}
	}

	public void resume()
	{
		for (int i=0; i<dropItems.Count; ++i) {
			DropItem d = dropItems[i];
			if(d!=null&&d.data.type == DropType.Soul)
			{
				d.enabled = true;
			}
		}
	}

    private static int random_count = 0;
    // 掉落特殊物品，模型在DropItemModel表中查询，返回受击特效ID
    public int showSpecialDropItem(int id, int num, Vector3 pos, bool is_rand)
    {
        if (!GConfig.dropEnable || id == 0)
            return 0;

        if (true == is_rand)
        {
            // 按照一定的几率掉落
            ++random_count;
            if (0 == random_count % 2)
                return 0;
        }

        BattleCameraDummy.GetInstance().ShakeCamera(GameDefines.CAMERA_SHAKE_X, GameDefines.CAMERA_SHAKE_Y, GameDefines.CAMERA_SHAKE_TIME);

        int item_id = id * 100000 + num;
        DropItemModel table_drop = TableManager.GetInstance().GetData<DropItemModel>(item_id);
        if (null != table_drop)
        {
            ResManager.single.load(table_drop.item_prefab, onItemLoadFinish, pos, new ItemData(id, num, DropType.Equip));
            DataManager.single.fbData.slash_hit_counter++;
            return table_drop.hit_effect_id;
        }
        else
        {
            ResManager.single.load("Game/Model/Drop/BaoXiang", onItemLoadFinish, pos, new ItemData(id, num, DropType.Equip));
            DataManager.single.fbData.slash_hit_counter++;
            return 0;
        }
    }

    // 魔族入侵打死最后一个怪后，一次性掉落物品
    public int showSpecialDropItem(int id, int num, Vector3 pos)
    {
        if (!GConfig.dropEnable || id == 0)
            return 0;

        int item_id = id * 100000 + num;
        DropItemModel table_drop = TableManager.GetInstance().GetData<DropItemModel>(item_id);
        if (null != table_drop)
        {
            ResManager.single.load(table_drop.item_prefab, onItemLoadFinish, pos, new ItemData(id, num, DropType.Equip));
            return table_drop.hit_effect_id;
        }
        else
        {
            ResManager.single.load("Game/Model/Drop/BaoXiang", onItemLoadFinish, pos, new ItemData(id, num, DropType.Equip));
            return 0;
        }
    }

	public void showDropItem(int id, int num, Vector3 pos, DropType dropType = DropType.Equip)
	{
		if (!GConfig.dropEnable)
			return;
		switch(dropType)
		{
		case DropType.Equip:
			if(id==0)return;
            ResManager.single.load("Game/Model/Drop/BaoXiang", onItemLoadFinish, pos, new ItemData(id, num, DropType.Equip));
			break;
		case DropType.Money:
			if(num==0)return;
            ResManager.single.load("Game/Model/Drop/QianDai", onItemLoadFinish, pos, new ItemData(id, num, DropType.Money));
			break;
        case DropType.GhostExp:
            if (num == 0) return;
            ResManager.single.load("Game/Model/Drop/ui_fugui_xing01", onSpritePetDrop, pos, new ItemData(id, num, DropType.GhostExp));
            break;
        case DropType.GhostHappy:
            if (num == 0) return;
            ResManager.single.load("Game/Model/Drop/ui_fugui_xin01", onSpritePetDrop, pos, new ItemData(id, num, DropType.GhostHappy));
            break;
        case DropType.GhostHungry:
            if (num == 0) return;
            ResManager.single.load("Game/Model/Drop/ui_fugui_baozi01", onSpritePetDrop, pos, new ItemData(id, num, DropType.GhostHungry));
            break;
		default:
            LogManager.GetInstance().LogError("not support this drop type id=" + id);
			break;
		}
	}

    public void showDropSoul(int soul_id, int soul_type, Vector3 pos)
    {
        switch (soul_type)
        {
            case 0:
                ResManager.single.load("Game/Model/Drop/LingYun", onItemLoadFinish, pos, new ItemData(soul_id, soul_type, DropType.Soul));
                break;
            case 1:
                ResManager.single.load("Game/Model/Drop/LingYun_PingPong", onItemLoadFinish, pos, new ItemData(soul_id, soul_type, DropType.Soul));
                break;
            case 2:
                ResManager.single.load("Game/Model/Drop/LingYun_zhiliao", onItemLoadFinish, pos, new ItemData(soul_id, soul_type, DropType.Soul));
                break;
            case 3:
                ResManager.single.load("Game/Model/Drop/LingYun_sudu", onItemLoadFinish, pos, new ItemData(soul_id, soul_type, DropType.Soul));
                break;
            case 4:
                ResManager.single.load("Game/Model/Drop/LingYun_gongjili", onItemLoadFinish, pos, new ItemData(soul_id, soul_type, DropType.Soul));
                break;
        }
    }

    public void showDropSoul_Normal(Vector3 pos)
    {
        if (!GConfig.dropEnable)
            return;

        ResManager.single.load("Game/Model/Drop/LingYun", onItemLoadFinish, pos, new ItemData(0, GameDefines.value_ball_mp_recover, DropType.Soul));

//         float rand = Random.Range(0f, 100f);
//         if (rand <= 60f)
//         {
//             ResManager.single.load("Game/Model/Drop/LingYun", onItemLoadFinish, pos, new ItemData(0, 40, DropType.Soul));
//         }
//         else if (rand <= 70f)
//         {
//             ResManager.single.load("Game/Model/Drop/LingYun_PingPong", onItemLoadFinish, pos, new ItemData(0, 80, DropType.Soul));
//         }
//         else if (rand <= 80f)
//         {
//             ResManager.single.load("Game/Model/Drop/LingYun_zhiliao", onItemLoadFinish, pos, new ItemData(-1, 10, DropType.Soul));
//         }
//         else if (rand <= 90f)
//         {
//             ResManager.single.load("Game/Model/Drop/LingYun_sudu", onItemLoadFinish, pos, new ItemData(-2, 20, DropType.Soul));
//         }
//         else
//         {
//             ResManager.single.load("Game/Model/Drop/LingYun_gongjili", onItemLoadFinish, pos, new ItemData(-3, 15, DropType.Soul));
//         }
    }

//     public void showDropSoul_PingPong(Vector3 pos)
//     {
//         if (!GConfig.dropEnable)
//             return;
// 
//         float rand = Random.Range(0f, 100f);
//         if (rand <= 80f)
//             ResManager.single.load("Game/Model/Drop/LingYun_PingPong", onItemLoadFinish, pos, new ItemData(0, 80, DropType.Soul));
//         else
//             ResManager.single.load("Game/Model/Drop/LingYun_zhiliao", onItemLoadFinish, pos, new ItemData(-1, 30, DropType.Soul));
//     }

	public void showDropIcon(Vector3 pos, ItemData data)
	{
		ResManager.single.load ("UI/UI_Drop", onIconLoadFinish, pos, data);
	}

    public void showSpritePetIcon(Vector3 pos, ItemData data, Vector3 target_pos)
    {
        ResManager.single.load("UI/UI_SpritePetIcon", onSpriteIconLoadFinish, pos, data, target_pos);
    }

    void onSpritePetDrop(GameObject go, object param1 = null, object param2 = null, object param3 = null)
    {
        Vector3 p = (Vector3)param1;
        DropItem d = go.GetComponent<DropItem>();
        d.transform.position = new Vector3(p.x, p.y, p.z);
        d.transform.parent = transform;
        d.data = param2 as ItemData;
        dropItems.Add(d);

        GhostWindow mGhostWin = WindowManager.single.findWindow(GhostWindow.NAME) as GhostWindow;
        if (mGhostWin != null)
        {
            MainToolkit.SetLayerRecursively(go, GameDefines.layer_default);
        }
    }

	void onItemLoadFinish(GameObject go, object param1=null, object param2=null, object param3=null)
	{
		Vector3 p = (Vector3)param1;
		DropItem d = go.GetComponent<DropItem> ();
		d.transform.position = new Vector3(p.x, p.y+1, p.z);
		d.transform.parent = transform;
		d.data = param2 as ItemData;
		dropItems.Add (d);

        if (BattleSceneManager.GetInstance() && BattleSceneManager.GetInstance().current_battle_state == BattleState.Slash)
        {
            MainToolkit.SetLayerRecursively(go, GameDefines.layer_actor);
        }  
    }

	void onIconLoadFinish(GameObject go, object param1=null, object param2=null, object param3=null)
	{
		Vector3 p = (Vector3)param1;
		DropIcon d = go.GetComponent<DropIcon> ();
		d.transform.parent = mount;
		d.transform.localScale = Vector3.one;
		d.transform.position = new Vector3 (p.x, p.y, p.z);
		d.data = param2 as ItemData;
	}

    void onSpriteIconLoadFinish(GameObject go, object param1 = null, object param2 = null, object param3 = null)
    {
        Vector3 p = (Vector3)param1;
        SpritePetIcon d = go.GetComponent<SpritePetIcon>();
        d.dst_pos = (Vector3)param3;
        d.transform.parent = mount;
        d.transform.localScale = Vector3.one;
        d.transform.position = new Vector3(p.x, p.y, p.z);
        d.data = param2 as ItemData;
    }

    public void SetDropsActive(bool enable)
    {
        _drop_root.SetActive(enable);
    }

    // 设置掉落物品的层
    public void SetDropItemLayer(int layer)
    {
        for (int i = 0; i < dropItems.Count; ++i)
        {
            if (null != dropItems[i] && dropItems[i].gameObject.activeSelf)
                dropItems[i].gameObject.layer = layer;
        }
    }

    // 设置掉落物品的渲染层级
    public void SetRenderLayer(int layer)
    {
        for (int i = 0; i < dropItems.Count; ++i)
        {
            if (null != dropItems[i] && dropItems[i].gameObject.activeSelf)
                dropItems[i].SetLayer(layer);
        }
    }

    // 随机一个掉落物品
    public ItemInfo RandomDropItem(int item_group_id)
    {
        ItemInfo item_info = new ItemInfo();
        ItemGroup item_group = TableManager.GetInstance().GetDataByKey(typeof(ItemGroup), item_group_id) as ItemGroup;
        int total_weight = 0;
        for (int i = 0; i < item_group.item_weight.Length && 0 != item_group.item_weight[i]; ++i)
        {
            total_weight += item_group.item_weight[i];
        }

        int rand = Random.Range(0, total_weight);
        int sum_weight = 0;
        for (int i = 0; i < item_group.item_weight.Length; ++i)
        {
            sum_weight += item_group.item_weight[i];
            if (rand < sum_weight)
            {
                item_info.ItemID = item_group.item_ID[i];
                item_info.ItemNum = item_group.item_num[i];
                break;
            }
        }
        return item_info;
    }

#region 战斗中灵蕴球的掉落逻辑
    private int _round_count = 0; // 当前战斗回合数
    private int _rest_soul_num = 0; // 剩余灵蕴球数量
    private float _round_time = 0.0f; // 战斗持续时间
    private int _total_soul = 0; // 总共掉落的灵蕴球数量

    public int round_count { get { return _round_count; } }

    public void UpdateSoulDropChance()
    {
        if ((!GuideSystem.single.isFinish(501)) && (!GuideSystem.single.isGuiding()))
            return;

        if (true == BeheadSystem.GetInstance().in_piling_or_slashing)
            return;

        _round_time += Time.deltaTime;
        if (_round_time > GameDefines.value_round_time)
        {
            ++_round_count;
            _round_time = _round_time - GameDefines.value_round_time;
            if (0 == _round_count % 3)
            {
                ++_rest_soul_num;
            }
        }
    }

    public void ResetSoulDropChance()
    {
        _round_count = 0;
        _round_time = 0.0f;
        _total_soul = 0;

        if ((!GuideSystem.single.isFinish(202)) && (!GuideSystem.single.isGuiding()))
            _rest_soul_num = 0;
        else
            _rest_soul_num = 1;

        if (GameDefines.FB_SKY_PATH == GGameLogic.m_FightFBType)
            _rest_soul_num = 1;
    }

    public void TryDropSoul(ActorDummy dummy)
    {
        if (_rest_soul_num > 0)
        {
            LogManager.GetInstance().LogMessage("掉落灵蕴球1个. 回合数:" + _round_count + " 当前灵蕴球个数:" + _rest_soul_num + " 总计掉落个数:" + _total_soul, LogManager.ModuleFilter.Effect);
            DropManager.single.showDropSoul_Normal(dummy.cur_pos);
            --_rest_soul_num;
            ++_total_soul;
        }
    }
#endregion
}
*/