using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using XLua;

namespace Scripts.CoreScripts.Core
{
	[ReflectionUse]
	[DisallowMultipleComponent]
	public class AssetRef : MonoBehaviour
	{
		public Object _asset;
		[HideInInspector][SerializeField]string[] _depends;//不声明SerializeField或public，实例化对象时，该变量不会赋值
		void Awake(){
			AddRef ();
		}

		void OnDestroy () {
			DecRef ();
		}
		
		internal void SetDepends(string[] depends)
		{
            _depends = depends;
		}

        internal bool hasDepends()
        {
            return _depends != null;
        }

        void CopyRef(AssetRef ar)
		{
			if (ar == null)return;
            _asset = ar._asset;
			string[] tmp = _depends;
            _depends = ar._depends;
			AddRef ();
			DecRef (tmp);
		}

		internal static void DecRef(string[] depends)
		{
			if (depends == null)return;
			for (int i = 0, max = depends.Length; i < max; ++i) 
			{
				string key = depends [i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.DecAssetRef (key);
			}
		}

		internal void AddRef()
		{
			if (_depends == null)return;
			for (int i = 0, max = _depends.Length; i < max; ++i) 
			{
				string key = _depends[i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.AddAssetRef (key);
			}
		}

		internal void  DecRef()
		{
			if (_depends == null)return;
			for (int i = 0, max = _depends.Length; i < max; ++i) 
			{
				string key = _depends[i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.DecAssetRef (key);
			}
            _depends = null;
		}

		public static bool SetImage(Image image, string path)
		{
			if (path == null)return false;
			using (ResLoad rl = ResLoad.Get (path)) 
			{
                if (null == rl.Asset<GameObject>()) return false;
				return SetImage (image, rl);
			}
		}

		public static bool SetImage(Image image, ResLoad rl)
		{
            if (image == null) return false;
			AssetRef ar = image.GetComponent<AssetRef> ();
			if (ar == null)ar = image.gameObject.AddComponent<AssetRef> ();
			ar.CopyRef (rl.Depends ());
			image.sprite = ar._asset as Sprite;
			return true;
		}
	}
}