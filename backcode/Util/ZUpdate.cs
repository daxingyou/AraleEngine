using UnityEngine;
using System.Collections;
using System.Xml;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using ICSharpCode.SharpZipLib.Zip;
using Scripts.CoreScripts.Core.Platform;

namespace Scripts.CoreScripts.Core
{
    public class ZUpdate
    {
	    const int RecvBuffSize = 256*1024;
	    const int TimeOut = 5000;
	    string dataPath;
	    string updateUrl;
	    int    mVersion   = 0;
		int    mPart      = 0;
	    bool   bCancel;
	    OnUpdate onUpdate = null;
	    Thread downThread = null;
	    HttpWebRequest mHWR;
	    public delegate void OnUpdate(UpdateEventValue v);
	
	    //download info
	    int    totalBytes;
	    int    downBytes;
		public void init(string uid, string appVer)
	    {
			mVersion = ResLoad.version;
			mPart    = ResLoad.part;
		    dataPath = Application.persistentDataPath+"/Res/";
		    EventManager.single.registEventListener("UpdateEvent",updateCallBack);
	    }
	
	    public void deinit()
	    {
			onUpdate = null;
		    EventManager.single.unregistEventListener("UpdateEvent",updateCallBack);
	    }
	    //==========================================================================
	    public enum Event
	    {
		    DownConfig, //获取配置/
		    CheckFile,  //校验文件/
		    CalcSize,   //计算大小/
		    DownFile,   //下载文件/
		    UnzipFile,  //解压文件/
		    Success,    //更新成功/
		    Failure,    //更新失败/
	    }

	    public class UpdateEventValue
	    {
		    public string info;
		    public float  progress;
		    public Event  evt;
		    public int    resSize;
		    public UpdateEventValue(string info, float progress, Event evt, int resSize=0)
		    {
			    this.info = info;
			    this.progress = progress;
			    this.evt = evt;
			    this.resSize = resSize;
		    }
	    }
	
	    public void cancel()
	    {
		    bCancel = true;
		    if(null!=mHWR)mHWR.Abort();
		    cancelCDN ();
		    if(downThread==null)return;
		    downThread.Join();
		    downThread = null;
	    }
	
		public void updateAssetBundle(string url, OnUpdate callback, int newResVer, int newResPart)
	    {
		    bCancel    = false;
		    totalBytes = 1;
		    downBytes  = 0;
		    onUpdate   = callback;
		    updateUrl  = url;
			string appVersionName = Device.getVersionName ();
			Log.I("start update url="+updateUrl+appVersionName+",dataPath="+dataPath, Log.Tag.Update);
			Log.I("update versionpart:"+mVersion+"."+mPart+"->"+newResVer+"."+newResPart, Log.Tag.Update);
		    if(!Directory.Exists(dataPath))Directory.CreateDirectory(dataPath);

		    mNewResVer=newResVer;
			mNewResPart=newResPart;
		    #if UNITY_ANDROID
			updateUrl = string.Format("{0}{1}/{2}{3}/", url, appVersionName, "android", newResVer);
		    #elif UNITY_IPHONE
			updateUrl = string.Format("{0}{1}/{2}{3}/", url, appVersionName, "iphone", newResVer);
		    #else
			updateUrl = string.Format("{0}{1}/{2}{3}/", url, appVersionName, "standalone", newResVer);
		    #endif
		    downThread = new Thread(new ThreadStart(CDNUpdateThread));
		    downThread.Start();
	    }

	    //download callback func/
	    void updateCallBack(EventManager.EventBase eb)
	    {
		    UpdateEventValue v = eb.eventValue as UpdateEventValue;
			if(v.evt == Event.Success)
			{
				ResLoad.SetVersionPart(mVersion,mPart);
				Log.I("^_^ update ok",Log.Tag.Update);
			}
		    if(null!=onUpdate)onUpdate(v);
	    }

	    void onCheckFileProgress(float percent)
	    {
		    string thing = string.Format ("校验资源文件", 100*percent);
		    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue(thing, 0, Event.CheckFile));
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
	    //============================

    #region CDN下载
	    HttpWebRequest[] mHWRS = new HttpWebRequest[2];
	    void cancelCDN()
	    {
		    mError = true;
		    for (int i=0; i<mHWRS.Length; ++i) {
			    if(mHWRS[i]==null)continue;
			    mHWRS[i].Abort();
			    mHWRS[i]=null;
		    }
	    }
	    List<XmlPatch.DFileInfo> mDFList;
	    System.Object mLock = new System.Object();
	    bool mError = false;
	    int  mNewResVer = 0;
		int  mNewResPart= 0;
	    int  mExitCount = 0;
	    string mErrorFile=null;
	    XmlPatch mPatch;
	    void CDNUpdateThread()
	    {
		    string thing = "update";
		    mExitCount = 0;
		    mError = false;
		    mErrorFile = null;
		    List<Thread> threadLs = new List<Thread> ();
		    try
		    {
				if(mVersion>=mNewResVer&&mPart>=mNewResPart)
			    {
				    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue(thing = "已经是最新资源版本无需更新", 1.0f, Event.Success));
				    return;
			    }

			    //获取版本配置文件/
			    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue(thing = "获取资源配置文件", 0, Event.DownConfig));
			    mPatch = new XmlPatch(dataPath);
			    if(mPatch.getVersion()!=mNewResVer)
			    {
				    mHWR = HttpWebRequest.Create (new Uri (updateUrl + "version.xml"))as HttpWebRequest;
				    mHWR.KeepAlive = false;
				    mHWR.Timeout=TimeOut;
				    mHWR.ReadWriteTimeout=TimeOut;
				    HttpWebResponse rt = mHWR.GetResponse() as HttpWebResponse;
				    recvXml(dataPath+"version.xml", rt.GetResponseStream(), (int)rt.ContentLength);
				    rt.Close();
			    }
			    mHWR=null;

			    //校验文件,统计下载文件列表/
				mDFList = mPatch.listDownFiles(onCheckFileProgress,ref bCancel,mNewResPart);
			    if(mDFList==null)throw new Exception("user cancel checking");
			    totalBytes=0;
			    for(int i=0,max=mDFList.Count;i<max;++i)
			    {
				    XmlPatch.DFileInfo df = mDFList[i];
				    FileInfo f = new FileInfo(df.path+"."+mNewResVer+".tmp");
				    if(f.Exists)
				    {
					    totalBytes+=(int)(df.size - f.Length);
				    }
				    else
				    {
					    totalBytes+=(int)df.size;
				    }
			    }

			    //重新设置下载进度/
			    int updateBytes = getTotalBytes(mNewResVer, mNewResPart, totalBytes);
			    if(updateBytes>=totalBytes)
			    {
				    downBytes = updateBytes - totalBytes;
				    totalBytes = updateBytes;
			    }
			    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue(thing="更新资源文件", 0, Event.CalcSize, totalBytes));
			    System.GC.Collect();

				//开启文件下载线程/
			    for (int i=0; i<mHWRS.Length; ++i)
				{
				    Thread t = new Thread(new ParameterizedThreadStart(CDNFileDownThread));
				    threadLs.Add(t);
				    t.Start(i);
			    }
		    }
		    catch(Exception e)
		    {
			    mError=true;
			    mHWR=null;
                Log.E(e, Log.Tag.Update);
		    }

		    //通知显示资源下载进度/
		    while (mExitCount<threadLs.Count)
		    {
			    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue("", 0.99f*downBytes/totalBytes, Event.DownFile));
			    Thread.Sleep(100);
		    }

		    //等待下载线程结束/
		    for (int i=0; i<threadLs.Count; ++i)
		    {
			    threadLs[i].Join();
		    }

			//解压lua脚本
			if (!mError && !unzipLua ()) 
			{
				mErrorFile = "脚本解压";
				mError = true;
			}

		    //通知下载结果/
		    mPatch=null;
			Log.I ("cdn main thread exit", Log.Tag.Update);
		    if (!mError)
		    {
			    EventManager.single.pushEvent ("UpdateEvent", new UpdateEventValue ("清除临时文件", 0.99f, Event.DownFile));
			    //清除无效的tmp文件
			    DirectoryInfo dir = new DirectoryInfo(dataPath);
			    FileInfo[] fis = dir.GetFiles("*.tmp",SearchOption.AllDirectories);
			    for (int i=0,max=fis.Length; i<max; ++i) {fis[i].Delete();}
			    //通知更新完成
				mVersion=mNewResVer;
				mPart   =mNewResPart;
				EventManager.single.pushEvent ("UpdateEvent", new UpdateEventValue ("资源更新完成", 1.0f, Event.Success));
		    }
		    else
		    {
			    Thread.Sleep(500);
			    if(mErrorFile!=null)thing=mErrorFile;
			    thing = bCancel?"用户取消了更新":thing+="失败,"+"请重试";
			    EventManager.single.pushEvent("UpdateEvent",new UpdateEventValue(thing, 0.99f*downBytes/totalBytes, Event.Failure));
		    }
	    }

	    void CDNFileDownThread(object param)
	    {
		    int threadIdx = (int)param;
		    Log.I("cdn sub thread start",Log.Tag.Update);
		    XmlPatch.DFileInfo df = null;
		    byte[] buf = new byte[RecvBuffSize];
		    int pLen = dataPath.Length;
			int  retryTimes = 0;
		    while(!mError)
		    { 
				if(retryTimes<=0)
				{
				    lock(mLock)
				    {
					    if(mDFList.Count<1)break;
					    df = mDFList [0];
					    mDFList.RemoveAt(0);
				    }
				}
					
			    HttpWebResponse rt=null;
			    FileStream fs=null;
			    Stream st=null;
			    try
			    {
				    string savePath = df.path;
				    string tmpPath  = savePath+"."+mNewResVer+".tmp";
				    fs = File.Exists(tmpPath)?new FileStream(tmpPath, FileMode.Append, FileAccess.Write, FileShare.Write):new FileStream(tmpPath, FileMode.Create);
				    int tmpSize = (int)fs.Length;
				    if(tmpSize<df.size)
				    {
					    mHWRS[threadIdx] = HttpWebRequest.Create (new Uri (updateUrl + savePath.Substring(pLen)))as HttpWebRequest;
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
						    Interlocked.Add(ref downBytes, n);
					    }
				    }
				    fs.Close();

				    if(File.Exists(savePath))File.Delete(savePath);
				    File.Move(tmpPath,savePath);
					if(df.md5!=FileUtils.GetMd5Hash (savePath))
				    {
					    File.Delete(savePath);
					    throw new Exception("data md5 error");
				    }
					retryTimes=0;
			    }
			    catch(Exception e)
			    {
					++retryTimes;
					if(bCancel || retryTimes>=3)
					{
						if(mErrorFile==null)mErrorFile="更新资源文件";//mErrorFile="download "+System.IO.Path.GetFileName(df.path);
				    	mError=true;
                    	Log.E(e, Log.Tag.Update);
					}
					else
					{
						Log.I("retry download file="+System.IO.Path.GetFileName(df.path), Log.Tag.Update);
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
            Log.I("cdn sub thread exit", Log.Tag.Update);
        }


	    int getTotalBytes(int ver, int part, int total)
	    {
		    string path = dataPath+"downinfo"+ver+"_"+part;
		    if(File.Exists(path))
		    {
			    string s = File.ReadAllText(path);
			    int n=0;
			    if(int.TryParse(s, out n))
				    return n;
			    else
				    return 0;
		    }
		    else
		    {
			    File.WriteAllText(path,total.ToString());
			    return total;
		    }
	    }

		bool unzipLua()
		{
			Log.I ("begin unzip lua", Log.Tag.RES);
			try
			{
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
				Log.E(e);
				return false;
			}
		}
    #endregion

	#region 单文件下载
		public class FileDownLoad
		{
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
				if(fdlCount++==0)EventManager.single.registEventListener("DownFileEvent",DownLoadCallBack);
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
					Log.E (e, Log.Tag.Default, e.StackTrace);
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
				if(--fdlCount==0)EventManager.single.unregistEventListener("DownFileEvent",DownLoadCallBack);
				_onDownload = null;
			}

			static void DownLoadCallBack(EventManager.EventBase eb)
			{
				FileDownLoad fdl = eb.eventValue as FileDownLoad;
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
					Log.E (e, Log.Tag.Default, e.StackTrace);
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
					Log.E (e, Log.Tag.Default, e.StackTrace);
					OnDownFailed (fdl);
				}
			}

			void OnDownComplete(FileDownLoad fdl)
			{
				fdl._state = State.Completed;
				fdl.innerStop();
				EventManager.single.pushEvent("DownFileEvent", fdl);
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
				EventManager.single.pushEvent("DownFileEvent", fdl);
			}

			void OnProgress(FileDownLoad fdl)
			{
				EventManager.single.pushEvent("DownFileEvent", fdl);
			}
		}
	#endregion
    }
}