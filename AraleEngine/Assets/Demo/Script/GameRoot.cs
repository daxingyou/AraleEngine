using UnityEngine;
using Arale.Engine;

public class GameRoot : GRoot {
    protected override void GameStart()
    {
        Log.mDebugLevel = 1;
        Randoms.init();
        Randoms.lookId = 1001;//监视该物品掉落概率
    }

    protected override void GameExit()
    {
		
    }

    protected override void GameUpdate()
    {
		
    }
}
