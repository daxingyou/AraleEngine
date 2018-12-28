using UnityEngine;
using Arale.Engine;

public class GameRoot : GRoot {
    protected override void gameStart()
    {
		Log.mFilter = (int)(Log.Tag.Net | Log.Tag.Unit | Log.Tag.Skill);
		Log.mDebugLevel = 2;
    }

    protected override void gameExit()
    {
    }

    protected override void gameUpdate()
    {
    }
}
