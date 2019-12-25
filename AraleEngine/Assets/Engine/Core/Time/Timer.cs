using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
    
    public class Timer
    {
        public delegate void OnTimer(Timer.Node n);
        public class Node
        {
            public int timerID;
            public float time;
            public Node(int timerID, float time)
            {
                this.timerID = timerID;
                this.time = time;
            }
            public void loop(float delay)
            {
                this.time += delay;
            }
        }

        List<Node> nodes = new List<Node>();
        float time;
        OnTimer onTimer;
        public Timer(OnTimer onTimer)
        {
            this.onTimer = onTimer;
        }

        public bool AddTimer(int timerID, float delay)
        {//添加node对象池提生性能
            Node n = nodes.Find(delegate(Node nd){return nd.timerID == timerID;});
            if(n!=null)return false;
            nodes.Add(new Node(timerID, time+delay));
            nodes.Sort(delegate(Node a, Node b){return a.time.CompareTo(b.time);});
            return true;
        }

        public void RemoveTimer(int timerID)
        {
            int idx = nodes.FindIndex(delegate(Node nd){return nd.timerID == timerID;});
            if (idx >= 0)nodes.RemoveAt(idx);
        }

        public void update()
        {
            time += Time.deltaTime;
            int i = 0;
            bool dirty = false;
            for (int max=nodes.Count; i < max; ++i)
            {
                Node n = nodes[i];
                if (time < n.time)break;
                onTimer(n);
                if (n.time > time)
                {
                    nodes.Add(n);
                    dirty = true;
                }
            }
            nodes.RemoveRange(0, i);
            if(dirty)nodes.Sort(delegate(Node a, Node b){return a.time.CompareTo(b.time);});
        }
    }

}
