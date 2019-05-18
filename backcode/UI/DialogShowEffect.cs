using UnityEngine;
using System.Collections;

using DG.Tweening;

namespace Scripts.BehaviourScripts.UI.Common{
	public class DialogShowEffect : MonoBehaviour {

		public float _apearTime = 0.3f;//出现时间

		public float _amplitude = 1.0f;//振幅

		public float _period = 0.2f;//

		// Use this for initialization
		void Start () {
			transform.localScale = new Vector3 (0.8f, 0.8f, 0.8f);
            transform.DOScale(1.0f, _apearTime).SetEase(Ease.OutElastic, _amplitude, _period);           
		}
		
		// Update is called once per frame
		void Update () {
		
		}
	}
}
