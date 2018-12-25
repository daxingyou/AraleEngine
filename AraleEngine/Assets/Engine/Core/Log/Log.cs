using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;
using System.Timers;


namespace Arale.Engine
{
    public class Log 
    {
        public enum Tag
        {
            Default =       1 << 0,
            RES =           1 << 1,
            Net =           1 << 2,
            BT  =           1 << 3,
            DB  =           1 << 4,
            UI  =           1 << 5,
            Update =        1 << 6,
            Sitcom =        1 << 7,
            Unit =          1 << 8,
            Scene =         1 << 9,
            Skill =         1 << 10,
        }

        public enum Type
        {
            D,
            I,
            W,
            E,
        }

        const int MaxLogFileSize = 1024 * 1024;
        static public int  mDebugLevel=0;
        static public bool mWriteScreen=false;
        static public bool mWriteFile=false;
        static public bool mErrorPause = false;
        static public bool mWriteFileImmediate;
        static public bool mShowTimeStamp;
        static public int  mFilter=int.MaxValue;

        static string mLogFile;
        static Mutex mMutex = new Mutex();
        static Queue<string> mLogQueue = new Queue<string>();
        static System.Timers.Timer mFlushTimer;
        static public LogData mLogData = new LogData();

        public static void init ()
        {
            readConfig ();
            if (mWriteFile)
            {
                mLogFile = Application.persistentDataPath+"/gamelog.txt";
                FileInfo fi = new FileInfo (mLogFile);
                if (fi.Exists && fi.Length > MaxLogFileSize)fi.Delete ();

                if(!mWriteFileImmediate)
                {
                    mFlushTimer = new System.Timers.Timer(10000);
                    mFlushTimer.Enabled = true;
                    mFlushTimer.Elapsed += new ElapsedEventHandler(writeToFile);
                }

                mLogQueue.Enqueue("Log Start:"+DateTime.Now.ToString("MM-dd HH:mm:ss"));
            }
            Application.logMessageReceived += onUnityLogCallback;
        }

        public static void deinit ()
        {
            Application.logMessageReceived -= onUnityLogCallback;
            writeToFile(null, null);
        }

        public static void saveConfig()
        {
            UnityEngine.PlayerPrefs.SetInt ("Log.debugLevel", mDebugLevel);
            UnityEngine.PlayerPrefs.SetInt ("Log.WriteScreen", mWriteScreen?1:0);
            UnityEngine.PlayerPrefs.SetInt ("Log.WriteFile", mWriteFile?1:0);
            UnityEngine.PlayerPrefs.SetInt ("Log.errorPause", mErrorPause?1:0);
            UnityEngine.PlayerPrefs.SetInt ("Log.ShowTimeStamp", mShowTimeStamp?1:0);
            UnityEngine.PlayerPrefs.SetInt ("Log.Filter", mFilter);
            UnityEngine.PlayerPrefs.Save ();
        }

        public static void readConfig()
        {
            #if DEBUG_TOOL_ENABLE
            mDebugLevel = UnityEngine.PlayerPrefs.GetInt ("Log.debugLevel", 1);
            mWriteScreen = UnityEngine.PlayerPrefs.GetInt ("Log.WriteScreen", 1) == 1;
            mWriteFile = UnityEngine.PlayerPrefs.GetInt ("Log.WriteFile", 0) == 1;
            mErrorPause = UnityEngine.PlayerPrefs.GetInt ("Log.errorPause", 0) == 1;
            mShowTimeStamp = UnityEngine.PlayerPrefs.GetInt ("Log.ShowTimeStamp", 1) == 1;
            mFilter = UnityEngine.PlayerPrefs.GetInt ("Log.Filter", 0);
            #else
            mDebugLevel = UnityEngine.PlayerPrefs.GetInt ("Log.debugLevel", 0);
            mWriteScreen = UnityEngine.PlayerPrefs.GetInt ("Log.WriteScreen", 0) == 1;
            mWriteFile = UnityEngine.PlayerPrefs.GetInt ("Log.WriteFile", 0) == 1;
            mErrorPause = UnityEngine.PlayerPrefs.GetInt ("Log.errorPause", 0) == 1;
            mShowTimeStamp = UnityEngine.PlayerPrefs.GetInt ("Log.ShowTimeStamp", 0) == 1;
            mFilter = UnityEngine.PlayerPrefs.GetInt ("Log.Filter", 0);
            #endif
        }

        static void onUnityLogCallback(string condition, string StackTrace, LogType lt)
        {
            if (lt == LogType.Exception) 
            {
                Log.e (condition, Tag.Default, StackTrace);
                //DebugTools.errorShow = true;
            }
        }

        public static void d(string msg, Tag tag=Tag.Default, object content=null)
        {
            if(mDebugLevel<3)return;
            if((mFilter&(int)tag)==0)return;
            writeLog(Type.D, tag, msg, content);
        }

        public static void i(string msg, Tag tag=Tag.Default, object content=null)
        {
            if(mDebugLevel<2)return;
            if((mFilter&(int)tag)==0)return;
            writeLog(Type.I, tag, msg, content);
        }

        public static void w(string msg, Tag tag=Tag.Default, object content=null)
        {
            if(mDebugLevel<1)return;
            writeLog(Type.W, tag, msg, content);
        }

        public static void e(string msg, Tag tag=Tag.Default, object content=null)
        {
            if(mDebugLevel<1)return;
            writeLog(Type.E, tag, msg, content);
        }

        public static void e(Exception exception, Tag tag=Tag.Default, object content=null)
        {
            if(mDebugLevel<1)return;
            writeLog(Type.E, tag, exception.ToString(), content);
        }

        static void writeLog(Type type, Tag tag, string msg, object content)
        {
            mMutex.WaitOne();
            string logStr;
            if(mShowTimeStamp)
            {
                string strLogTime = DateTime.Now.ToString("MM-dd HH:mm:ss.fff");
                logStr = string.Format("{0} {1} {2} {3}", type.ToString(), strLogTime, tag.ToString(), msg);
            }
            else
            {
                logStr = string.Format("{0} {1} {2}", type.ToString(), tag.ToString(), msg);
            }
            mLogQueue.Enqueue(logStr);

            if(mWriteFileImmediate)
            {
                writeToFile(null, null);
            }

            if(mWriteScreen)
            {
                mLogData.addLog(type, logStr, content);
                if (mErrorPause && type == Log.Type.E)
                {
                    mWriteScreen = false;
                }
            }
            mMutex.ReleaseMutex();

            #if UNITY_EDITOR
            switch(type)
            {
                case Type.D:
                case Type.I:
                    Debug.Log(logStr, null);
                    break;
                case Type.W:
                    Debug.LogWarning(logStr, null);
                    break;
                case Type.E:
                    Debug.LogError(logStr, null);
                    break;
            }
            #endif
        }

        static void writeToFile(object source, ElapsedEventArgs e)
        {
            if (!mWriteFile || mLogQueue.Count == 0)
                return;

            mMutex.WaitOne();
            {
                StreamWriter sw = File.AppendText(mLogFile);
                while (mLogQueue.Count > 0)
                {
                    string strLogData = mLogQueue.Dequeue();
                    sw.WriteLine(strLogData);
                }
                sw.Close();
            }
            mMutex.ReleaseMutex();
        }


        #region 屏幕日志缓存
        public class LogData
        {
            public delegate void OnNewLogListener(Log.Type type, Log.LogData.Item msg);
            public OnNewLogListener onNewLogListener;
            const int MaxItem = 512;
            uint mICount;
            public uint infoCount{get{return mICount;}}
            uint mWCount;
            public uint warnningCount{get{return mWCount;}}
            uint mECount;
            public uint errorCount{get{return mECount;}}
            uint mCount;
            public uint count{get{return mCount;}}
            public uint lastIdx{get{return mCount;}}

            public class Item
            {
                uint mIdx;
                public uint    idx{ get{return mIdx;}}
                Type mType;
                public Type    type{get{return mType;}}
                string mMsg;
                public string  msg{get{return mMsg;}}
                string mStack;
                public string  stack{get{return mStack;}}
                public Item(uint idx, Type type, string msg, object content)
                {
                    this.mIdx  = idx;
                    this.mType = type;
                    this.mMsg  = idx+":"+msg;
                    this.mStack= content as string;
                }
            }

            Item[] mItems = null;
            internal void addLog(Type type, string msg, object content)
            {
                if(mItems==null)mItems = new Item[MaxItem];
                switch (type)
                {
                    case Type.D:
                    case Type.I:
                        ++mICount;
                        mItems [++mCount % MaxItem] = new Item (mCount, Type.I, msg, content);
                        break;
                    case Type.W:
                        ++mWCount;
                        mItems [++mCount % MaxItem] = new Item (mCount, type, msg, content);
                        break;
                    case Type.E:
                        ++mECount;
                        mItems [++mCount % MaxItem] = new Item (mCount, type, msg, content);
                        break;
                }
                if (null != onNewLogListener)onNewLogListener (type, mItems[mCount % MaxItem]);
            }

            public int getLog(int pageIdx, Type type, LogData.Item[] outMsg)//pageIdx start at 0
            {
                if (pageIdx < 0)pageIdx = 0;
                int m = 0;
                int page = 0;
                int count = outMsg.Length;
                int pageCount = count;
                uint max = mCount > MaxItem ? MaxItem : mCount;
                for (uint c = 0, i = mCount % MaxItem; c < max && count > 0; ++c,i = (i + MaxItem - 1) % MaxItem)
                {
                    Item it = mItems [i];
                    if (it.type != type && type != Type.D)continue;
                    page = m++ / pageCount;
                    if (page < pageIdx)continue;
                    outMsg [--count]=it;
                }
                while (--count >= 0)outMsg [count] = null;
                return page;
            }

            public void clear()
            {
                mICount = 0;
                mWCount = 0;
                mECount = 0;
                mCount  = 0;
            }
        }
        #endregion

    }
}
