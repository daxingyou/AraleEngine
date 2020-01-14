using Arale.Engine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActMainWindow : Window
{
    Player hero;
    public UIStick stick;
    // Start is called before the first frame update
    public override void OnWindowEvent(Window.Event eventId)
    {
        if(eventId == Event.Create)
        {
            hero = mUserData as Player;
            EventMgr.single.AddListener("Game.Player", OnBindPlayer);
        }
        else if(eventId == Event.Destroy)
        {
            EventMgr.single.RemoveListener("Game.Player", OnBindPlayer);
        }
    }

    void OnBindPlayer(EventMgr.EventData ed)
    {
       hero = ed.data as Player;
        Camera.main.GetComponent<CameraController>().mTarget = hero.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (hero == null) return;
        Vector3 dir = new Vector3(stick.mDir.x, 0.0f, stick.mDir.y).normalized;
        if (dir.z == 0 && dir.x == 0)
        {
            hero.move.moveStop();
        }
        else
        {
            hero.move.move(dir);
        }
    }
}
