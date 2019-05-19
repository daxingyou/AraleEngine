using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Net;
using System;
using ICSharpCode.SharpZipLib.Zip;
using System.Timers;

namespace Arale.Engine
{
    //支持分包同时更新，但是初始包必须开始游戏时首先更新
    public class ResUpdate
    {
        static ResUpdate mThis;
        public static ResUpdate single{get { return mThis!=null?mThis:mThis = new ResUpdate(); }}

        ResUpdate()
        {
            mDataPath = ResLoad.resPath;
            if(!Directory.Exists(mDataPath))Directory.CreateDirectory(mDataPath);
            EventMgr.single.AddListener("UpdateEvent",updateCallBack);
            mPatch = new XmlPatch(mDataPath);
            mNewVersion = ResLoad.version;
            clearTmpFile();
        }

        public void dispose()
        {
            stopTask();
            EventMgr.single.RemoveListener("UpdateEvent",updateCallBack);
            mThis = null;
        }

        void clearTmpFile()
        {
            //下载的临时文件会被清掉
            DirectoryInfo dir = new DirectoryInfo(mDataPath);
            FileInfo[] fis = dir.GetFiles("*.tmp",SearchOption.AllDirectories);
            for (int i=0,max=fis.Length; i<max; ++i) {fis[i].Delete();}
        }

        void initUrl(int version, string url)
        {
            string appVersionName = GRoot.device.getAppVersion ();
            mUrl = string.Format("{0}{1}/{2}{3}/", url, appVersionName, FileUtils.platform, version);
            Log.i("initUrl="+mUrl, Log.Tag.Update);
        }


        public PatchTask checkVersion(int newVersion, string url)
        {//version.xml只能在进入游戏前下载
            initUrl(mNewVersion, url);
            if(ResLoad.version >= newVersion)return null;
            mNewVersion = newVersion;
            initUrl(mNewVersion, url);
            PatchTask pt = getTask("init");
            if (pt == null)mTasks.Add(pt = new PatchTask("init", ResLoad.part));
            Log.i("pullVersion:"+ResLoad.version+"->"+newVersion, Log.Tag.Update);
            return pt;
        }

        //====================
        int       mNewVersion;
        string    mUrl;
        string    mDataPath;
        XmlPatch  mPatch;
        bool      mRuning;
        List<PatchTask> mTasks = new List<PatchTask>();
        public PatchTask getTask(string name, int[] downParts=null)
        {
            PatchTask pt = mTasks.Find(delegate(PatchTask obj){return obj.mName == name;});
            if (downParts!=null&&pt == null)
            {
                if (mUrl==null)throw new Exception("url not set, please checkVersion first");
                if (ResLoad.version != mNewVersion)throw new Exception("version.xml is not match version, please checkVersion first");
                pt = new PatchTask(name, downParts);
                mTasks.Add(pt);
            }
            return pt;
        }

        //name=null will stop all task
        void stopTask(string name=null)
        {   
            if (name != null)
            {
                for (int i = 0; i < mTasks.Count; ++i)mTasks[i].stop();
            }
            else
            {
                PatchTask pt = mTasks.Find(delegate(PatchTask obj){return obj.mName == name;});
                if (pt != null)pt.stop();
            }
        }

        public void closeTask(string name)
        {
            PatchTask pt = getTask(name);
            if (pt == null)return;
            pt.stop();
            mTasks.Remove(pt);
        }

        public bool hasRunningTask(bool includeBackground=false)
        {
            for (int i = 0; i < mTasks.Count; ++i)
            {
                PatchTask pt = mTasks[i];
                if (!includeBackground && pt.runInBackground)
                    continue;
                if (pt.isRunning)return true;
            }
            return false;
        }

        void updateCallBack(EventMgr.EventData eb)
        {
            UpdateEvent ue = eb.data as UpdateEvent;
            ue.task.onEvent(ue.state,ue.action,ue.progress);
            if (ue.state != State.Completed)return;
            for (int i = 0; i < mTasks.Count; ++i)mTasks[i].start(Action.CalcSize);
        }

        #region 下载缓存
        Dictionary<int, XmlPatch.DFileInfo> mDFS = new Dictionary<int, XmlPatch.DFileInfo>();
        void addDFile(XmlPatch.DFileInfo df)
        {
            lock (mDFS)
            {
                if (mDFS.ContainsKey(df.id))
                    return;
                mDFS[df.id] = df;
            }
        }

        XmlPatch.DFileInfo lockDFile(int id)
        {
            lock (mDFS)
            {
                XmlPatch.DFileInfo df;
                if (!mDFS.TryGetValue(id, out df))
                    return null;
                if (df.locked)
                    return null;
                df.locked = true;
                return df;
            }
        }
        #endregion

        #region 更新任务
        public class PatchTask
        {
            const int RecvBuffSize = 256*1024;
            const int TimeOut = 5000;
            HttpWebRequest[] mHWRS = new HttpWebRequest[2];
            HttpWebRequest   mHWR;
            Thread mThread = null;
            System.Object mLock = new System.Object();
            long   mExitCount;
            bool   mCancel;
            bool   mError;

            public string mName{ get;protected set;}
            public int[]  mParts{ get;protected set;}
            public long   mTotalSize{ get;protected set;}//总大小
            long          mDownSize;//已下载的大小
            public long   mWantSize{get{return mTotalSize-mDownSize;}}
            public float  mProgress{ get;protected set;}
            public bool   useCancel{ get{return mCancel;}}
            public Action mAction{get;protected set;}
            public State  mState{get;protected set;}
            public bool   isRunning{get{ return mState == State.Doing; }}
            public bool   runInBackground;
            List<int>     mIDS;
            int           mIndex;
            OnPatchTaskCallback mCallback;
            public void addListern(OnPatchTaskCallback callback){mCallback += callback;}
            public void removeListern(OnPatchTaskCallback callback){mCallback -= callback;}
            internal PatchTask(string name, int[] parts)
            {
                if(mThis==null)throw new Exception("disable new PatchTask by yourself");
                mParts = parts;
                mName  = name;
                Log.i("creat patch task name="+mName+",parts="+ResLoad.intArray2Str(parts), Log.Tag.Update);
            }

            internal void onEvent(State state, Action action, float progress)
            {
                mAction   = action;
                mState    = state;
                mProgress = progress;
                switch (mState)
                {
                    case State.Completed:
                        ResLoad.setVersionPart(mThis.mNewVersion, mParts);
                        Log.i("patch task ok! name=" + mName, Log.Tag.Update);
                        if (mCallback != null)mCallback(this);
                        //移除任务
                        mThis.closeTask(mName);
                        break;
                    default:
                        if (mCallback != null)mCallback(this);
                        break;
                }
            }

            public bool start(Action action=Action.DownFile)
            {
                if (mState == State.Doing||mState==State.Completed)return false;
                Log.i("start patch Task name="+mName+",action="+action, Log.Tag.Update);
                mState     = State.Doing;
                mError     = false;
                mCancel    = false;
                mIndex     = 0;
                mExitCount = 0;
                mProgress  = 0;
                if (action == Action.CalcSize)mTotalSize = 0;
                mThread = new Thread(new ParameterizedThreadStart(CDNDownThread));
                mThread.Start(action);
                return true;
            }

            public void stop()
            {
                mCancel = true;
                mError  = true;
                if(null!=mHWR)mHWR.Abort();
                for (int i=0; i<mHWRS.Length; ++i)
                {
                    if(mHWRS[i]==null)continue;
                    mHWRS[i].Abort();
                    mHWRS[i]=null;
                }

                if (mThread != null)
                {
                    mThread.Join();
                    mThread = null;
                }
            }
                
            void CDNDownThread(object param)
            {
                float progress = 0;
                Action action = Action.DownXml;
                try
                {
                    //获取版本配置文件/
                    EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress,State.Doing,action));
                    if(mThis.mPatch.getVersion() != mThis.mNewVersion)
                    {
                        mHWR = HttpWebRequest.Create (new Uri (mThis.mUrl + "version.xml"))as HttpWebRequest;
                        mHWR.KeepAlive = false;
                        mHWR.Timeout=TimeOut;
                        mHWR.ReadWriteTimeout=TimeOut;
                        HttpWebResponse rt = mHWR.GetResponse() as HttpWebResponse;
                        recvXml(mThis.mDataPath+"version.xml", rt.GetResponseStream(), (int)rt.ContentLength);
                        rt.Close();
                        mTotalSize=0;
                    }
                    mHWR=null;

                    //计算下载文件
                    if(mTotalSize <= 0)
                    {
                        action     = Action.CalcSize;
                        mIDS       = new List<int>();
                        mParts     = mThis.mPatch.getDepends(mParts);
                        List<XmlPatch.DFileInfo> dfs = mThis.mPatch.listDownFiles(delegate(float percent){EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress=percent,State.Doing,action));}, ref mCancel, mParts);
                        long size=0;
                        long tmpSize=0;
                        for(int i=0,max=dfs.Count;i<max;++i)
                        {
                            XmlPatch.DFileInfo df = dfs[i];
                            mThis.addDFile(dfs[i]);
                            mIDS.Add(df.id);

                            size += df.size;
                            FileInfo f = new FileInfo(df.path+"."+mThis.mNewVersion+".tmp");
                            if(f.Exists)tmpSize+=f.Length;
                        }

                        mTotalSize= size;
                        mDownSize=tmpSize;
                        System.GC.Collect();
                        if((Action)param == Action.CalcSize)
                        {
                            EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress,State.Idle,action));
                            return;
                        }
                    }
                }
                catch(Exception e)
                {
                    mHWR=null;
                    mError=true;
                    Log.e(e, Log.Tag.Update);
                    EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress,State.Failed,action));
                    return;
                }


                //开启文件下载线程/
                progress  = mTotalSize>0?1.0f * Interlocked.Read(ref mDownSize) / mTotalSize:1;
                action = Action.DownFile;
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress,State.Doing,action));
                List<Thread> lsThread= new List<Thread> ();
                for (int i=0; i<mHWRS.Length; ++i)
                {
                    Thread t = new Thread(new ParameterizedThreadStart(CDNFileDownThread));
                    lsThread.Add(t);
                    t.Start(i);
                }
                //通知显示资源下载进度/
                while (Interlocked.Read(ref mExitCount) < lsThread.Count)
                {
                    progress  = mTotalSize>0?1.0f * Interlocked.Read(ref mDownSize) / mTotalSize:1;
                    EventMgr.single.PostEvent("UpdateEvent",new UpdateEvent(this,progress,State.Doing,action));
                    Thread.Sleep(100);
                }
                //等待下载线程结束/
                for (int i=0; i<lsThread.Count; ++i)
                {
                    lsThread[i].Join();
                }

                //解压lua脚本
                mError = !unzipLua (ref action, ref progress);
                //===========
                Log.i ("cdn main thread exit,task name="+this.mName, Log.Tag.Update);
                if (!mError)
                {
                    //通知更新完成
                    EventMgr.single.PostEvent ("UpdateEvent", new UpdateEvent(this,1,State.Completed,action));
                }
                else
                {
                    Thread.Sleep(500);
                    EventMgr.single.PostEvent ("UpdateEvent", new UpdateEvent(this,progress,State.Failed,action));
                }
            }

            void CDNFileDownThread(object param)
            {
                int threadIdx = (int)param;
                //Log.I("cdn sub thread start",Log.Tag.Update);
                XmlPatch.DFileInfo df = null;
                byte[] buf = new byte[RecvBuffSize];
                int pLen   = mThis.mDataPath.Length;
                int  retryTimes = 0;
                while(!mError)
                {
                    if(retryTimes<=0)
                    {//取下一个下载文件
                        lock(mLock)
                        {
                            if (mIDS.Count<=0)break;
                            mIndex = ++mIndex % mIDS.Count;
                            df = mThis.lockDFile(mIDS[mIndex]);
                            if (df == null)continue;
                            if (df.size <= 0)
                            {
                                mIDS.RemoveAt(mIndex);
                                continue;
                            }
                        }
                    }

                    HttpWebResponse rt=null;
                    FileStream fs=null;
                    Stream st=null;
                    try
                    {
                        string savePath = df.path;
                        string tmpPath  = savePath+"."+mThis.mNewVersion+".tmp";
                        fs = File.Exists(tmpPath)?new FileStream(tmpPath, FileMode.Append, FileAccess.Write, FileShare.Write):new FileStream(tmpPath, FileMode.Create);
                        int tmpSize = (int)fs.Length;
                        if(tmpSize<df.size)
                        {
                            mHWRS[threadIdx] = HttpWebRequest.Create (new Uri (mThis.mUrl + savePath.Substring(pLen)))as HttpWebRequest;
                            //mHWRS[threadIdx].KeepAlive = true;//保持连接，否则消耗服务器的连接数,有的服务器设置会强制关闭连接/
                            mHWRS[threadIdx].KeepAlive = false;
                            mHWRS[threadIdx].Timeout=TimeOut;
                            mHWRS[threadIdx].ReadWriteTimeout=TimeOut;
                            mHWRS[threadIdx].AddRange("bytes",tmpSize);//必须在GetResponse之前设置
                            rt = mHWRS[threadIdx].GetResponse() as HttpWebResponse;
                            st = rt.GetResponseStream();
                            int size = ((int)df.size) - tmpSize;
                            int n = 0;
                            while(size>0)
                            {
                                n = st.Read(buf, 0, size>RecvBuffSize?RecvBuffSize:size);
                                if(n<=0)throw new Exception("HttpWebResponse read exception");
                                fs.Write(buf,0,n);
                                size -= n;
                                Interlocked.Add(ref mDownSize, n);
                            }
                        }
                        fs.Close();

                        if(File.Exists(savePath))File.Delete(savePath);
                        File.Move(tmpPath,savePath);
                        if(df.md5!=FileUtils.GetMd5Hash (savePath))
                        {
                            File.Delete(savePath);
                            Interlocked.Add(ref mDownSize, -df.size);
                            throw new Exception("data md5 error");
                        }
                        retryTimes=0;
                        df.size   =0;//标记文件下载完成
                        df.locked = false;
                    }
                    catch(Exception e)
                    {
                        ++retryTimes;
                        if(mCancel || retryTimes>=3)
                        {
                            mError=true;
                            df.locked = false;
                            Log.e("download failed file="+df.path, Log.Tag.Update);
                            Log.e(e, Log.Tag.Update);
                        }
                        else
                        {
                            Log.i("retry download file="+df.path, Log.Tag.Update);
                            Thread.Sleep(300);
                        }
                    }
                    finally
                    {
                        if(null!=fs)fs.Close();
                        if(null!=st)st.Close();
                        if(null!=rt)rt.Close();
                    }
                }
                Interlocked.Increment (ref mExitCount);
                //Log.I("cdn sub thread exit", Log.Tag.Update);
            }

            bool unzipLua(ref Action action, ref float progress)
            {
                if (mError)return false;
                Log.i ("begin unzip lua", Log.Tag.Update);
                try
                {
                    action = Action.Unzip;
                    EventMgr.single.PostEvent ("UpdateEvent", new UpdateEvent(this,progress,State.Doing,action));
                    string dataPath = mThis.mDataPath;
                    string luaZip  = dataPath+"/lua.data";
                    if(!File.Exists(luaZip))return true;
                    string outPath = dataPath+"/lua/";
                    string tagFile = outPath+FileUtils.GetMd5Hash(luaZip);
                    if(File.Exists(tagFile))return true;
                    if(Directory.Exists(outPath))Directory.Delete(outPath, true);
                    FastZip fz = new FastZip();
                    fz.ExtractZip(dataPath+"/lua.data", dataPath+"/lua", null);
                    File.WriteAllText(tagFile, "ok");
                    return true;
                }
                catch(Exception e)
                {
                    Log.e(e);
                    return false;
                }
            }

            void recvXml(string savePath, Stream st, int size)
            {
                string tmp = savePath+".tmp";
                FileStream fs = null;
                try
                {
                    fs = new FileStream(tmp, FileMode.Create);
                    byte[] rBuf = new byte[RecvBuffSize];
                    int n = 0;
                    while(size>0)
                    {
                        n = st.Read(rBuf, 0, size>RecvBuffSize?RecvBuffSize:size);
                        if(n<=0)throw new Exception("HttpWebResponse read exception");
                        fs.Write(rBuf,0,n);
                        size -= n;
                    }
                    if(fs!=null)fs.Close();
                    File.Copy(tmp, savePath, true);
                }
                catch(System.Exception ex)
                {
                    if(fs!=null)fs.Close();
                    throw ex;
                }
            }
        }
        #endregion

        public delegate void OnPatchTaskCallback(PatchTask task);

        public enum State
        {
            Idle,      //空闲
            Doing,     //运行
            Completed, //完成
            Failed,    //失败
        };

        public enum Action
        {
            DownXml,
            CalcSize,
            Unzip,
            DownFile,
        };

        class UpdateEvent
        {
            public PatchTask task;
            public State  state;
            public Action action;
            public float  progress;
            public UpdateEvent(PatchTask task, float progress, State state, Action action)
            {
                this.task = task;
                this.progress = progress;
                this.state = state;
                this.action = action;
            }
        }

        public static string ToSize(long bytes)
        {
            if (bytes > 1024 * 1024 * 1024)
            {
                return (1.0f * bytes / (1024 * 1024 * 1024)).ToString("f2") + "GB";
            }
            else if (bytes > 1024 * 1024)
            {
                return (1.0f * bytes / (1024 * 1024)).ToString("f2") + "MB";
            }
            else if (bytes > 1024)
            {
                return (1.0f * bytes / 1024).ToString("f2") + "KB";
            }
            else
            {
                return (float)bytes + "B";
            }
        }
    }


    #region 单文件下载
    public class FileDownLoad
    {
        const int RecvBuffSize = 256*1024;
        public enum State
        {
            Idle,
            Doing,
            Completed,
            Failed,
        };
        public enum Action
        {
            CalcSize,
            DownFile,
        };
        public delegate void OnDownLoad(FileDownLoad v);
        HttpWebRequest _request;
        HttpWebResponse _response;
        Stream     _stream;
        FileStream _file;
        string     _url;
        string     _path;
        int        _size;
        int        _dsize;
        string     _md5;
        string     _info;
        State      _state;
        Action     _action;
        bool       _cancel;

        byte[]     _buf;
        OnDownLoad _onDownload;
        System.Timers.Timer _timeout;
        int _timecount;

        public int totalSize{
            get{ return _size; }
        }
        public int downSize{
            get{ return _size-_dsize; }
        }
        public float progress{
            get{ return _size==0?0:0.99f*(_size-_dsize)/_size;}
        }
        public Action action{
            get{ return _action; }
        }
        public State state{
            get{ return _state; }
        }
        public string info{
            get{return _info; }
        }
        public string path{
            get{return _path; }
        }

        static int fdlCount;
        public FileDownLoad(string url, string savePath, string md5=null)
        {
            if(fdlCount++==0)EventMgr.single.AddListener("DownFileEvent",DownLoadCallBack);
            _state      = State.Idle;
            _action     = Action.DownFile;
            _url        =  url;
            _path       = savePath;
            _md5        = md5;
        }

        public void Start(OnDownLoad callback, Action action = Action.DownFile)
        {
            try
            {
                if(_state == State.Doing)throw(new Exception("last request not over"));
                _onDownload = callback;
                _state      = State.Doing;
                _action     = action;
                _cancel     = false;

                _request = HttpWebRequest.Create (new Uri (_url))as HttpWebRequest;
                _request.KeepAlive = false;
                //_request.Timeout=TimeOut;//Timeout,ReadWriteTimeout对于异步请求无效,需自己实现超时
                //_request.ReadWriteTimeout=TimeOut;
                _timeout = new System.Timers.Timer(1000);
                _timeout.Elapsed += new ElapsedEventHandler(TimeOutCallBack);
                _timeout.Enabled = true;
                switch(_action)
                {
                    case Action.DownFile:
                        _file = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write);
                        if(_file.Length>0)_file.Position = _file.Length - 1;//range设置到文件尾请求会返回错误
                        _request.AddRange("bytes", (int)_file.Position);
                        if(_buf==null)_buf = new byte[RecvBuffSize];
                        break;
                    case Action.CalcSize:
                        _size = 0;
                        _dsize = 0;
                        break;
                }
                _request.BeginGetResponse(new AsyncCallback(ResponseCallback), this);
                OnProgress(this);
            }
            catch(Exception e) 
            {
                Log.e (e, Log.Tag.Default, e.StackTrace);
                OnDownFailed(this);
            }
        }

        public void Stop()
        {
            _cancel = true;
            innerStop ();
        }

        void innerStop()
        {
            if (_file != null) 
            {
                _file.Close ();
                _file = null;
            }

            if (_stream != null) 
            {
                _stream.Close ();
                _stream = null;
            }

            if (_response != null) 
            {
                _response.Close ();
                _response = null;
            }

            if (_request != null) 
            {
                _request.Abort ();
                _request = null;
            }

            if (_timeout != null)
            {
                _timeout.Enabled = false;
            }
        }

        public void Dispose()
        {
            Stop ();
            if(--fdlCount==0)EventMgr.single.RemoveListener("DownFileEvent",DownLoadCallBack);
            _onDownload = null;
        }

        static void DownLoadCallBack(EventMgr.EventData eb)
        {
            FileDownLoad fdl = eb.data as FileDownLoad;
            if (null == fdl._onDownload)return;
            fdl._onDownload (fdl);
            if (fdl.state==State.Doing)return;
            fdl._onDownload = null;
        }

        void TimeOutCallBack(object source, ElapsedEventArgs e)
        {
            if (7 > Interlocked.Increment (ref _timecount))return;
            innerStop ();
        }

        void ResponseCallback(IAsyncResult result)
        {
            FileDownLoad fdl = null;
            try
            {
                fdl = (FileDownLoad)result.AsyncState;
                Interlocked.Exchange(ref fdl._timecount, 0);
                fdl._response = fdl._request.EndGetResponse(result) as HttpWebResponse;

                switch(_action)
                {
                    case Action.CalcSize:
                        int fsize = File.Exists(_path)?(int)(new FileInfo(_path).Length):0;
                        fdl._size = (int)fdl._response.ContentLength;
                        if(fdl._size<=0)throw new Exception("invalid url");
                        fdl._dsize = fdl._size - fsize;
                        OnDownComplete(fdl);
                        break;
                    case Action.DownFile:
                        fdl._stream = fdl._response.GetResponseStream();
                        fdl._size =  (int)(_file.Position + fdl._response.ContentLength);
                        fdl._dsize = (int)fdl._response.ContentLength;
                        if(_dsize>0)
                            fdl._stream.BeginRead(fdl._buf, 0, _dsize>RecvBuffSize?RecvBuffSize:_dsize, new AsyncCallback(StreamCallback), fdl);
                        else
                            OnDownComplete(fdl);
                        break;
                }
            }
            catch(Exception e) 
            {
                Log.e (e, Log.Tag.Default, e.StackTrace);
                OnDownFailed (fdl);
            }
        }

        void StreamCallback(IAsyncResult result)
        {
            FileDownLoad fdl = null;
            try
            {
                fdl = (FileDownLoad)result.AsyncState;
                Interlocked.Exchange(ref fdl._timecount, 0);
                int n = fdl._stream.EndRead(result);
                if(n<=0)throw new Exception("HttpWebResponse read exception");
                fdl._file.Write(fdl._buf, 0, n);
                fdl._file.Flush();
                _dsize -= n;
                //继续读数据
                if(_dsize>0)
                {
                    OnProgress(fdl);
                    fdl._stream.BeginRead(fdl._buf, 0, _dsize>RecvBuffSize?RecvBuffSize:_dsize, new AsyncCallback(StreamCallback), fdl);
                }
                else
                {
                    OnDownComplete(fdl);
                }
            }
            catch(Exception e) 
            {
                Log.e (e, Log.Tag.Default, e.StackTrace);
                OnDownFailed (fdl);
            }
        }

        void OnDownComplete(FileDownLoad fdl)
        {
            fdl._state = State.Completed;
            fdl.innerStop();
            EventMgr.single.PostEvent("DownFileEvent", fdl);
        }

        void OnDownFailed(FileDownLoad fdl)
        {
            switch (fdl.action)
            {
                case Action.DownFile:
                    fdl._info = _cancel?"下载已暂停":"下载文件失败";
                    break;
                case Action.CalcSize:
                    fdl._info = "获取文件大小失败";
                    break;
            }
            fdl._state = State.Failed;
            fdl.innerStop ();
            EventMgr.single.PostEvent("DownFileEvent", fdl);
        }

        void OnProgress(FileDownLoad fdl)
        {
            EventMgr.single.PostEvent("DownFileEvent", fdl);
        }
    }
    #endregion
}
