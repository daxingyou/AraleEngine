using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Arale.Engine;

public class UITimeCount : MonoBehaviour {
	public enum ExpireAction
	{
		Nothing,
		Hide,
		Destroy,
	}

	public Text _time;
	public Text _time2;
	public ExpireAction _expireAction = ExpireAction.Nothing;
	[System.NonSerialized]
	int _expireTime;
	int _restTime;
	// Use this for initialization
	public void SetExpireTime(int timeStamp)
	{//timestamp以秒为单位的截止时间戳
		gameObject.SetActive (true);
		_expireTime = timeStamp;
		InvokeRepeating("UpdateTime", 0, 0.3f);
	}

	void UpdateTime ()
	{
		
		int serverTime = (int)(RTime.R.utcTickMs/1000);
		_restTime = _expireTime - serverTime;
		if (_restTime >= 24 * 60 * 60) {//显示天时
			if (_time2 == null) {
				_time.text = string.Format ("{0}天{1}小时", _restTime / (24 * 60 * 60), _restTime % (24 * 60 * 60) / (60 * 60));
			} else {
				_time.text  = string.Format ("{0}天", _restTime / (24 * 60 * 60));
				_time2.text = string.Format ("{0}小时", _restTime % (24 * 60 * 60) / (60 * 60));
			}
		} else if (_restTime >= 60 * 60) {//显示时分
			if (_time2 == null) {
				_time.text = string.Format ("{0}小时{1}分钟", _restTime / (60 * 60), _restTime % (60 * 60) / (60));
			} else {
				_time.text = string.Format ("{0}小时", _restTime / (60 * 60));
				_time2.text = string.Format ("{0}分钟",  _restTime % (60 * 60) / (60));
			}
		} else if (_restTime >= 60) {//显示分秒
			if (_time2 == null) {
				_time.text = string.Format ("{0}分钟{1}秒", _restTime / 60, _restTime % 60);
			} else {
				_time.text = string.Format ("{0}分钟", _restTime / 60);
				_time2.text = string.Format ("{0}秒", _restTime % 60);
			}
		} else if (_restTime > 0) {
			if (_time2 == null) {
				_time.text = string.Format ("{0}秒", _restTime);
			} else {
				_time.text = "0分钟";
				_time2.text = string.Format ("{0}秒", _restTime % 60);
			}
		} else {
			if (_time2 == null) {
				_time.text = string.Format ("0秒");
			} else {
				_time.text = "0分钟";
				_time2.text = "0秒";
			}

			CancelInvoke ("UpdateTime");
			OnTimeExpire ();
			switch (_expireAction) 
			{
			case ExpireAction.Hide:
				gameObject.SetActive (false);
				break;
			case ExpireAction.Destroy:
				GameObject.Destroy (gameObject);
				break;
			}
		}
	}

	protected virtual void OnTimeExpire()
	{
	}
}
