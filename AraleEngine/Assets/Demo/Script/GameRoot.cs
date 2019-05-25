using UnityEngine;
using Arale.Engine;

public class GameRoot : GRoot {
    protected override void gameStart()
    {
        Log.mDebugLevel = 1;
        Randoms.init();
        Randoms.lookId = 1001;//监视该物品掉落概率
    }

    protected override void gameExit()
    {
		
    }

    protected override void gameUpdate()
    {
		
    }
}
