/*using UnityEngine;
using System.Collections;
using System;
//add to game gameobject
using SteamEngine.TableUtility;


public class DropItem : MonoBehaviour
{

    #region 可编辑属性
    public AnimationCurve m_item_drop_curve = new AnimationCurve(); // 掉落的曲线
    public int _rebounce_time = 0; // 物品落地后反弹的次数

    public Vector2 m_x_offset = new Vector2(3.5f, 6.0f); // 静态灵蕴球掉落位置横向偏移
    public Vector2 m_z_offset = new Vector2(-4.0f, -3.0f); // 静态灵蕴球掉落位置纵向偏移

    public float m_item_last_time = 3.0f; // 物品掉落后持续的时间，时间到后自动拾取
    public float m_item_drop_time = 1.0f; // 物品掉落的预期时间

    public int m_min_roll = 0;
    public int m_max_roll = 0;
    #endregion

    private DropManager.ItemData _data;
    private bool _has_picked; // 物品是否被拾取
    private Transform _battle_center = null; // 战场中心位置
    private Transform[] render_object = null; // 显示的物体，在堆叠中动态的改变它们的层级

    private Vector3 _target_pos = Vector3.zero;
    private Vector3 _origin_pos = Vector3.zero;
    private float _drop_time = 0.0f; // 物品坠落的时间
    private float _pass_time = 0.0f; // 流逝的时间

    private int _rand_round = 1; // 随机旋转的圈数

    public DropManager.ItemData data
    {
        set
        {
            _data = value;
        }
        get
        {
            return _data;
        }
    }

    // Use this for initialization
    void Start()
    {
        _rand_round = UnityEngine.Random.Range(m_min_roll, m_max_roll + 1);
        _has_picked = false;
        if (BattleSceneManager.GetInstance())
        {
            _battle_center = BattleSceneManager.GetInstance().GetFightRoundTrans();
        }
        else
        {
            GhostWindow mGhostWin = WindowManager.single.findWindow(GhostWindow.NAME) as GhostWindow;
            if (null != mGhostWin)
            {
                _battle_center = mGhostWin.working_Point[0];
            }
        }
        _drop_time = m_item_drop_time * GRandom.rand(0.7f, 1.0f);
        render_object = this.GetComponentsInChildren<Transform>(true);
        ToRandomPosition();
    }

    // 掉到随机的位置
    void ToRandomPosition()
    {
        _pass_time = 0.0f;
        _origin_pos = transform.position;

        _target_pos = Vector3.zero;
        _target_pos.x = GRandom.rand(m_x_offset.x, m_x_offset.y) * DropManager.single.screen_ratio;
        _target_pos.z = GRandom.rand(m_z_offset.x, m_z_offset.y);

        _target_pos = _battle_center.localToWorldMatrix.MultiplyPoint(_target_pos);

        // 通过碰撞取得和地面的交点
        _target_pos.y += 1f;
        Ray ray = new Ray(_target_pos, Vector3.down);
        RaycastHit hitInfo;
        LayerMask mask =  1 << GameDefines.layer_terrain;
        if (Physics.Raycast(ray, out hitInfo, 100, mask.value))
        {
            _target_pos = hitInfo.point;
        }
        else
        {
            _target_pos = _battle_center.position;
        }

        Vector3 pos = _target_pos;
        pos.y = transform.position.y;
        transform.LookAt(pos);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (BeheadSystem.GetInstance())
        {
            if (BeheadSystem.GetInstance().piling == true || BeheadSystem.GetInstance().slashing > 0)
                return;
        }
        
        _pass_time += Time.deltaTime;

        if (null != data && data.type != DropManager.DropType.Soul && 0 != _rand_round)
            transform.Rotate(Time.deltaTime / _drop_time * 360.0f * _rand_round, 0.0f, 0.0f);

        if (_pass_time >= _drop_time)
        {
            _pass_time = _drop_time;
            OnHitTarget();
        }

        float t = _pass_time / _drop_time;
        transform.position = Vector3.Lerp(_origin_pos, _target_pos, t);
        transform.position += new Vector3(0.0f, m_item_drop_curve.Evaluate(t), 0.0f);
        //Debug.LogError("++++++++++++" + t + " " + m_item_drop_curve.Evaluate(t) + " " + transform.position.x + " " + transform.position.y + " " + transform.position.z);
    }

    IEnumerator ShowGuideFinger()
    {
        yield return new WaitForSeconds(GameDefines.GetInstance().m_pick_ball_guide_time);

        if (true == _has_picked)
            yield break;

        BattleWindow battle_window = BeheadSystem.GetInstance().battle_window;
        battle_window.m_GuideFingerFollow.gameObject.SetActive(true);
        battle_window.m_GuideFingerFollow.target = this.transform.GetChild(0);
        battle_window.m_GuideFinger.SetActive(false);
        DropManager.show_finger_object = this;

        yield return null; // 避免显示动画出现收尾过渡的问题
        if (false == _has_picked && null != battle_window && true == battle_window.enabled && null != battle_window.m_GuideFinger)
        {
            battle_window.m_GuideFinger.SetActive(true);
            battle_window.m_GuideFingerTween.Play();
        }
    }

    void OnHitTarget()
    {
        while (null != data && data.type == DropManager.DropType.Soul)
        {
            if (GuideSystem.single.showGuide(200, true, gameObject))
            {
                EffectManager.single.play(401027, null, gameObject, onEffectCallback1);
                break;
            }
            break;
        }

        //LogManager.GetInstance().LogMessage("los y=" + v.y);
        if (0 == _rebounce_time)
        {
            enabled = false;

            if (null != data)
            {
                if (data.type == DropManager.DropType.Soul)
                {
                    if (false == PlayerManager.GetInstance().HasAllFunc(GameDefines.FUNC_AUTO_FIGHT)
                        && GameDefines.GetInstance().m_pick_ball_guide_time < m_item_last_time)
                        StartCoroutine(ShowGuideFinger()); // 新手阶段提示捡球

                    Invoke("delayPickUp", m_item_last_time);
                }
                else
                {
                    if (BattleSceneManager.GetInstance())
                    {
                        if (BattleSceneManager.GetInstance().current_battle_state != BattleState.Slash)
                        {
                            Invoke("delayPickUp", m_item_last_time);
                        }
                    }
                    else
                    {
                        GhostWindow ghost_win = WindowManager.single.findWindow(GhostWindow.NAME) as GhostWindow;
                        if (null != ghost_win)
                        {
                            switch (data.type)
                            {
                                case DropManager.DropType.GhostExp:
                                    //LogManager.GetInstance().LogError("" + ghost_win.m_ExpSprite.transform.position);
                                    StartCoroutine(FlyToUI(ghost_win.m_ExpSprite.transform.position));
                                    break;
                                case DropManager.DropType.GhostHappy:
                                    //LogManager.GetInstance().LogError("" + ghost_win.m_HappySprite.transform.position);
                                    StartCoroutine(FlyToUI(ghost_win.m_HappySprite.transform.position));
                                    break;
                                case DropManager.DropType.GhostHungry:
                                    //LogManager.GetInstance().LogError("" + ghost_win.m_HungrySprite.transform.position);
                                    StartCoroutine(FlyToUI(ghost_win.m_HungrySprite.transform.position));
                                    break;
                            }
                        }
                    }
                }

                // 战斗结束后，就把他们捡起来
                if (BattleSceneManager.GetInstance())
                {
                    if (BattleSceneManager.GetInstance().current_battle_state != BattleState.Fighting
                   && BattleSceneManager.GetInstance().current_battle_state != BattleState.Slash)
                    {
                        DropManager.single.pickup(this);
                    }
                }
            }
        }
        else
        {
            --_rebounce_time;
            ToRandomPosition();
        }
    }

    private void delayPickUp()
    {
        DropManager.single.pickup(this);
    }

    protected void SetPicked()
    {
        _has_picked = true;
        if (this == DropManager.show_finger_object)
        {
            BeheadSystem.GetInstance().battle_window.m_GuideFingerFollow.gameObject.SetActive(false);
            BeheadSystem.GetInstance().battle_window.m_GuideFingerFollow.target = null;
            DropManager.show_finger_object = null;
        }
        DropManager.single.dropItems.Remove(this);
        DestroyObject(gameObject);
    }
    public void pick()
    {
        if (_has_picked) 
            return;

        SetPicked();
        CancelInvoke("delayPickUp");
        if (data != null && data.type != DropManager.DropType.Soul)
        {
            DropManager.single.showDropIcon(transform.position, data);
        }
        else
        {
			GuideSystem.single.closeGuide();
            int effect_id = 11;
            foreach (ActorDummy dummy in HeroManager.GetInstance().battle_dummy_list)
            {
                if (dummy != null && false == dummy.actor_data.IsType(ActorType.Pet))
                {
                    Effect effect = EffectManager.single.play(effect_id, gameObject, dummy.cur_actor_object, onEffectCallback);
                    if (BeheadSystem.GetInstance().piling)
                        effect.setLayer(GameDefines.layer_actor);
                    else if (BeheadSystem.GetInstance().slashing > 0 )
                        effect.pause(); // 斩杀时灵蕴球不动
                }
            }
        }
    }

    // 竞技场中拾取灵蕴球
    public void ArenaPickup()
    {
        if (_has_picked)
            return;

        //拾取灵蕴球音效
        SEToolkit.PlayPickDropItemSE(this);
        SetPicked();

        int effect_id = 11;
        foreach (ActorDummy dummy in  PVPArenaBattleCtrl.GetPVPInstance().friend_list)
        {
            if (dummy != null && false == dummy.actor_data.IsType(ActorType.Pet))
            {
                Effect effect = EffectManager.single.play(effect_id, gameObject, dummy.cur_actor_object, OnArenaSoulCallback);
                if (BeheadSystem.GetInstance().in_piling_or_slashing)
                    effect.setLayer(GameDefines.layer_actor);
            }
        }
    }

    // 竞技场灵蕴球拾取
    void OnArenaSoulCallback(Effect.EventType eventType, Effect e, GameObject target)
    {
        if (eventType != Effect.EventType.Arrive)
            return;
        if (target == null)
            return;
        Effect effect = EffectManager.single.play(12, null, target);
        if (BeheadSystem.GetInstance().in_piling_or_slashing)
            effect.setLayer(GameDefines.layer_actor);
    }

    //灵韵飞散特效回调/
    void onEffectCallback(Effect.EventType eventType, Effect e, GameObject target)
    {
        if (eventType != Effect.EventType.Arrive)
            return;
        if (target == null)
            return;
        ActorDummy dummy = target.transform.parent.GetComponent<ActorDummy>();
        if (dummy != null && data != null && false == dummy.dead)
        {
            switch (data.id)
            {                   
                case 0:
                    // 回复灵蕴
                    dummy.ChangeMp(data.num, DamageInfoManager.NumberLevel.None);
                    break;
                case -1:
                    // 回复生命
                    dummy.ChangeHp(dummy.actor_data.fight_attribute[0].hp * data.num * 0.01f, DamageInfoManager.NumberLevel.Normal);
                    dummy.ChangeMp(GameDefines.value_ball_mp_recover, DamageInfoManager.NumberLevel.Normal);
                    break;
                case -2:
                    // 提升攻速
                    if (BattleSceneManager.GetInstance().IsFighting())
                    {
                        TableBuff table_buff = TableManager.GetInstance().GetDataByKey(typeof(TableBuff), 2616) as TableBuff;
                        dummy.buff_pool.AddBuff(table_buff, dummy, null);
                    }
                    dummy.ChangeMp(GameDefines.value_ball_mp_recover, DamageInfoManager.NumberLevel.Normal);
                    break;
                case -3:
                    // 提升攻击力
                    if (BattleSceneManager.GetInstance().IsFighting())
                    {
                        TableBuff table_buff1 = TableManager.GetInstance().GetDataByKey(typeof(TableBuff), 2621) as TableBuff;
                        dummy.buff_pool.AddBuff(table_buff1, dummy, null);
                        TableBuff table_buff2 = TableManager.GetInstance().GetDataByKey(typeof(TableBuff), 2626) as TableBuff;
                        dummy.buff_pool.AddBuff(table_buff2, dummy, null);
                    }
                    dummy.ChangeMp(GameDefines.value_ball_mp_recover, DamageInfoManager.NumberLevel.Normal);
                    break;
            }
        }
        Effect effect = EffectManager.single.play(12, null, target);
        if (BeheadSystem.GetInstance().in_piling_or_slashing)
            effect.setLayer(GameDefines.layer_actor);
    }

    //灵韵引导特效/
    void onEffectCallback1(Effect.EventType eventType, Effect e, GameObject target)
    {
        if (eventType == Effect.EventType.Play)
        {
            e.layer = GameDefines.layer_highlight;
			e.ignoreTimeScale=true;
        }
    }

    public void disappear()
    {
        if (_has_picked)
            return;

        SetPicked();
		//修改球消失不重置DataManager.single.comboData.resetCombo();
    }

    // 设置渲染物体的层级
    public void SetLayer(int layer)
    {
        try
        {
            for (int i = 0; i < render_object.Length; ++i)
            {
                if (this.gameObject != render_object[i].gameObject)
                    render_object[i].gameObject.layer = layer;
            }
        }
        catch (Exception e)
        {
            LogManager.GetInstance().LogException("DropItem SetLayer Error. ", e);
        }
    }

    // 飞向某个UI位置
    public IEnumerator FlyToUI(Vector3 target_pos)
    {
        yield return new WaitForSeconds(1.0f);

        DropManager.single.showSpritePetIcon(transform.position, _data, target_pos);
        DropManager.single.dropItems.Remove(this);
        DestroyObject(gameObject);
        // 触发进度条变化
    }
}
*/