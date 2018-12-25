using UnityEngine;
using System.Collections;

namespace Arale.Engine
{
    
    public class RecordMgr : MgrBase<RecordMgr>
    {
        public override void Init()
        {
            mSystem = new Record("YOUR-KEY");
            mSystem.Load ("system");
        }

        Record mSystem;
    	Record mPlayer;
    	public Record system{
    		get{return mSystem;}
    	}
    	public Record player{
    		get{return mPlayer;}
    	}


    	public void OpenPlayerRecord(string accountId, string areaId, string playerId)
    	{
    		if(mPlayer!=null)mPlayer.Save();
    		mPlayer = new Record("YOUR-KEY");
    		if(string.IsNullOrEmpty(accountId))accountId="0";
    		if(string.IsNullOrEmpty(areaId))areaId="0";
    		if(string.IsNullOrEmpty(playerId))playerId="0";
    		mPlayer.Load (accountId + "-" + areaId + "-"+playerId);
    	}

        public void ResetPlayerRecord(string accountId, string areaId, string playerId)
        {
            if (mPlayer != null) mPlayer.Save();
    		mPlayer = new Record("YOUR-KEY");
            if (string.IsNullOrEmpty(accountId)) accountId = "0";
            if (string.IsNullOrEmpty(areaId)) areaId = "0";
            if (string.IsNullOrEmpty(playerId)) playerId = "0";
            mPlayer.Reset(accountId + "-" + areaId + "-" + playerId);
        }

    	public void Save()
    	{
    		if(mSystem!=null)mSystem.Save ();
    		if(mPlayer!=null)mPlayer.Save ();
    	}
    }

}
