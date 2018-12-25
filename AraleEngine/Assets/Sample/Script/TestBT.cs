/*
using UnityEngine;
using System.Collections;

//绑定到要执行行为树的对象上
public class TestBT : MonoBehaviour
{
	public Transform target;
	#region 属性
	BTRoot.VSlot actionCD;
	BTRoot.VSlot findTarget;
	BTRoot.VSlot beAttack;
	BTRoot.VSlot distance;
	BTRoot.VSlot blood;
	#endregion

	#region 方法
	int patrol(int arg)
	{
		Debug.Log ("行为:巡逻");
		StartCoroutine ("_patrol");
		return 0;
	}

	IEnumerator  _patrol()
	{
		Debug.Log ("大王派我来巡山!");
		yield return new WaitForSeconds(3);
		findTarget.mVal = Random.Range (0, 2);
		if (findTarget.mVal>0)
		{
			Debug.Log ("大王，山里来了个毛脸和尚!");
		}
		actionCD.mVal = 0;
	}

	int show(int arg)
	{
		Debug.Log ("行为:秀");
		StartCoroutine ("_show");
		return 0;
	}

	IEnumerator  _show()
	{
		Debug.Log ("左三圈，右三圈，脖子扭扭屁股扭扭!");
		yield return new WaitForSeconds(3);
		actionCD.mVal = 0;
	}

	int walkToEnemy(int arg)
	{
		Debug.Log ("行为:走向目标");
		Debug.Log ("菇凉别走!");
		Vector3 dir = target.position - transform.position;
		if(dir.sqrMagnitude<1)
			transform.position = target.position;
		else
			transform.position += dir.normalized;
		return 0;
	}

	int doDistance(int arg)
	{
		distance.mVal =(int)((target.position - transform.position).sqrMagnitude*100);
		return 0;
	}

	int skill1(int arg)
	{
		Debug.Log ("行为:近程攻击");
		StartCoroutine ("_skill1");
		return 0;
	}

	IEnumerator  _skill1()
	{
		Debug.Log ("庐山升龙霸");
		yield return new WaitForSeconds(1);
		actionCD.mVal = 0;
	}

	int skill2(int arg)
	{
		Debug.Log ("行为:远程攻击");
		StartCoroutine ("_skill2");
		return 0;
	}

	IEnumerator  _skill2()
	{
		Debug.Log ("天马流星拳");
		yield return new WaitForSeconds(1);
		actionCD.mVal = 0;
	}

	int behit(int arg)
	{
		Debug.Log ("行为:被攻击");
		StartCoroutine ("_behit");
		return 0;
	}

	IEnumerator  _behit()
	{
		Debug.Log ("阿福咬了你一口!!!");
		yield return new WaitForSeconds(1);
		actionCD.mVal = 0;
	}

	int dead(int arg)
	{
		Debug.Log ("行为:死亡");
		StartCoroutine ("_dead");
		return 0;
	}

	IEnumerator  _dead()
	{
		Debug.Log ("oh my god!!!");
		yield return new WaitForSeconds(1);
		actionCD.mVal = 0;
	}
	#endregion

	public Object mBTFile;
	BTRoot mBTRoot = new BTRoot(0,0,0);
	// Use this for initialization
	void Start ()
	{
		mBTRoot.mountDebug (gameObject);
	}

	void regSlot()
	{
		findTarget = mBTRoot.getVSlot ("findTarge");
		beAttack   = mBTRoot.getVSlot ("beattack");
		actionCD   = mBTRoot.getVSlot ("actioncd");
		blood      = mBTRoot.getVSlot ("blood");
		blood.mVal = 100;
		distance   = mBTRoot.getVSlot ("distance");
		distance.mVal = 200;

		mBTRoot.getFSlot ("doPatrol").mFunc = patrol;
		mBTRoot.getFSlot ("doShow").mFunc = show;
		mBTRoot.getFSlot ("doGoto").mFunc = walkToEnemy;
		mBTRoot.getFSlot ("doDistance").mFunc = doDistance;
		mBTRoot.getFSlot ("doAttack1").mFunc = skill1;
		mBTRoot.getFSlot ("doAttack2").mFunc = skill2;
		mBTRoot.getFSlot ("behit").mFunc = behit;
		mBTRoot.getFSlot ("doDie").mFunc = dead;
	}

	void Update()
	{
		mBTRoot.update ();
	}

	void OnGUI()
	{
		int ox = 0;
		int oy = 0;
		string s = mBTRoot.isPlaying?"停止":"播放";
		if (GUI.Button (new Rect (ox, oy, 100, 30), s))
		{
			if (mBTFile == null) {
				Debug.LogError ("设置你要执行的脚本");
				return;
			}

			if (mBTRoot.isPlaying)
			{
				mBTRoot.stop ();
			}
			else
			{
                #if UNITY_EDITOR
				string path = UnityEditor.AssetDatabase.GetAssetPath (mBTFile);
				path = Application.dataPath + path.Remove(0,6);
				mBTRoot.load (path);
				regSlot ();
				mBTRoot.play ();
                #endif
			}
		}
	}
}
*/