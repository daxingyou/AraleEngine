using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Arale.Engine
{
	[DisallowMultipleComponent]
	public class AssetRef : MonoBehaviour
	{
		public Object mAsset;
		[HideInInspector][SerializeField]string[] mDepends;//不声明SerializeField或public，实例化对象时，该变量不会赋值
		void Awake(){
			addRef ();
		}

		void OnDestroy () {
			decRef ();
		}
		
		internal void setDepends(string[] depends)
		{
			mDepends = depends;
		}

		internal bool hasDepends()
		{
			return mDepends != null;
		}

		void copyRef(AssetRef ar)
		{
			if (ar == null)return;
			mAsset = ar.mAsset;
			string[] tmp = mDepends;
			mDepends = ar.mDepends;
			addRef ();
			decRef (tmp);
		}

		internal static void decRef(string[] depends)
		{
			if (depends == null)return;
			for (int i = 0, max = depends.Length; i < max; ++i) 
			{
				string key = depends [i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.decAssetRef (key);
			}
		}

		internal void addRef()
		{
			if (mDepends == null)return;
			for (int i = 0, max = mDepends.Length; i < max; ++i) 
			{
				string key = mDepends [i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.addAssetRef (key);
			}
		}

		internal void  decRef()
		{
			if (mDepends == null)return;
			for (int i = 0, max = mDepends.Length; i < max; ++i) 
			{
				string key = mDepends [i];
				if (string.IsNullOrEmpty (key))continue;
				ResLoad.decAssetRef (key);
			}
			mDepends = null;
		}

		public static bool setImage(Image image, string path)
		{
			using (ResLoad rl = ResLoad.get (path)) 
			{
				rl.asset<GameObject> ();
				return setImage (image, rl);
			}
		}

		public static bool setImage(Image image, ResLoad rl)
		{
			AssetRef ar = image.GetComponent<AssetRef> ();
			if (ar == null)ar = image.gameObject.AddComponent<AssetRef> ();
			ar.copyRef (rl.depends ());
			image.sprite = ar.mAsset as Sprite;
			return true;
		}
	}
}