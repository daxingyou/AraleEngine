using UnityEngine;
using System.Collections;
using Arale.Engine;
using System.Collections.Generic;

public class Randoms
{
    public static int lookId;//监视id
    static Randoms mThis;
    public static void init(){mThis = new Randoms();}
    int[] mSeeds;
    int   mIdx;
    Randoms()
    {
        mSeeds = new int[]{1,2,6,0,22,99,56,12,34,44,112,343,9,22,21,65,1,2,6,0,22,99,56,12,34,44,112,343,9,22,21,65};
    }

    class DropRandom
    {
        TBItem table;
        uint reqCount;//请求次数
        uint dropCount;//掉落次数
        float lastTime;//上次掉落时间
        public DropRandom(int id)
        {
            table = TableMgr.single.GetData<TBItem>(id);
            if(table==null)enable(false);
        }

        //reqRate可以理解每个怪的独立掉落率，最后会被平衡到物品整体掉落率附近
        public bool drop(int id, float reqRate)
        {
            float now = Time.realtimeSinceStartup;
            if (now - lastTime < table.dropInterval)return false;
            float realRate = 1.0f * dropCount / ++reqCount;
            float rate = (reqRate + (table.dropRate - realRate)) / 2;
            if (rate<=0 || Random.value > rate)return false;
            if(id==lookId)Debug.LogError(string.Format("id={0},realRate={1},curRate={2},interval={3}", id, realRate, rate, (now - lastTime)));
            ++dropCount; lastTime = now;
            return true;
        }

        public void enable(bool able)
        {
            lastTime = able ? 0 : float.MaxValue;
        }
    }

    public static float rang(float b, float e)
    {
        Random.seed = mThis.mSeeds[mThis.mIdx=++mThis.mIdx%mThis.mSeeds.Length];
        return Random.Range(b, e);
    }

    Dictionary<int,DropRandom> mDropRandoms = new Dictionary<int,DropRandom>();
    public static bool drop(int id, float reqRate)
    {
        DropRandom dr;
        if (!mThis.mDropRandoms.TryGetValue(id, out dr))
        {
            mThis.mDropRandoms[id] = dr = new DropRandom(id); 
        }
        return dr.drop(id, reqRate);
    }

    public static void enableDrop(int id, bool able)
    {
        DropRandom dr;
        if (!mThis.mDropRandoms.TryGetValue(id, out dr))
        {
            mThis.mDropRandoms[id] = dr = new DropRandom(id); 
        }
        dr.enable(able);
    }
}
