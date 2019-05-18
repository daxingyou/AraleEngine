using UnityEngine;
using System.Collections;

public class Atlas : ScriptableObject {
	public Sprite[] _sprites;
	public Sprite getSprite(string name)
	{
		for (int i = 0; i < _sprites.Length; ++i) {
			if (_sprites [i].name == name)
				return _sprites [i]; 
		}
		return null;
	}
}
