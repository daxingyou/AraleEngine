using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpriteAnimation : MonoBehaviour {
    public Sprite[] _sprites;
    public float    _Interval=0.33f;
    public bool     _autoPlay;
    Image _image;
    int   _idx;
	// Use this for initialization
	void Start () {
        _image = GetComponent<Image>();
        _image.sprite = _sprites[0];
        if (_autoPlay)
            play();
	}

    void play()
    {
        _idx = 0;
        InvokeRepeating("changeSprite", 0, _Interval);
    }

    void stop()
    {
        CancelInvoke("changeSprite");
    }

    void changeSprite()
    { 
        _image.sprite = _sprites[_idx];
        _idx = ++_idx%_sprites.Length;
    }
}
