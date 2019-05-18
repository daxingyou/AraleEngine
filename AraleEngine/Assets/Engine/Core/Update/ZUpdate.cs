/*#define CDN_UPDATE
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
using ICSharpCode.SharpZipLib.Zip;

namespace Arale.Engine
{
	public class ZUpdate{
		const int RecvBuffSize = 256*1024;
		const int TimeOut = 5000;
		string dataPath;
		string updateUrl;
		int    mVersion   = 0;
		public int version{ get { return mVersion; } }
		bool   bCancel;
		OnUpdate onUpdate = null;
		Thread downThread = null;
		HttpWebRequest mHWR;
		public delegate void OnUpdate(UpdateEventValue v);
		
		//download info
		int    totalBytes;
		int    downBytes;
		public void init(string uid, string appVer, int resVer)
		{
			mVersion = resVer;
			dataPath  = Application.persistentDataPath+"/Res/";
            EventMgr.single.AddListener("UpdateEvent",updateCallBack);
		}
		
		public void deinit()
		{
			EventMgr.single.RemoveListener("UpdateEvent",updateCallBack);
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
		
		public void updateAssetBundle(string url, OnUpdate callback, int newResVer=0)
		{
			bCancel    = false;
			totalBytes = 1;
			downBytes  = 0;
			onUpdate   = callback;
			updateUrl  = url;
			Log.i ("start update url="+updateUrl+",dataPath="+dataPath, Log.Tag.Update);
			Log.i ("local version:"+mVersion, Log.Tag.Update);
			if(!Directory.Exists(dataPath))Directory.CreateDirectory(dataPath);

	#if CDN_UPDATE
			mNewResVer=newResVer;
			#if UNITY_ANDROID
			updateUrl=url+"res/android"+newResVer+"/";
			#elif UNITY_IPHONE
			updateUrl=url+"res/iphone"+newResVer+"/";
			#else
			updateUrl=url+"res/standalone"+newResVer+"/";
			#endif
			downThread = new Thread(new ThreadStart(CDNUpdateThread));
	#endif
			downThread.Start();
		}

		//download callback func/
        void updateCallBack(EventMgr.EventData eb)
		{
            UpdateEventValue v = eb.data as UpdateEventValue;
			if(null!=onUpdate)onUpdate(v);
			if(v.evt == Event.Success)
			{
				RecordMgr.single.system.Save();
				UnityEngine.PlayerPrefs.SetInt ("Res.Ver",mVersion);
				UnityEngine.PlayerPrefs.Save();
				Log.i("^_^ update ok", Log.Tag.Update);
			}
		}

		public static int localResVersion
		{
			get{
				return UnityEngine.PlayerPrefs.GetInt ("Res.Ver",0);
			}
		}

		void onCheckFileProgress(float percent)
		{
			string thing = string.Format (GHelper.id2Str (240298), 100*percent);
            EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(thing, 0, Event.CheckFile));
		}
		
		void recvXml(string savePath, Stream st, int size)
		{
			string tmp = savePath+".tmp";
			FileStream fs = new FileStream(tmp, FileMode.Create);
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
		//============================

	#region CDN下载
	#if CDN_UPDATE
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
				if(mVersion>=mNewResVer)
				{
                    EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(thing = GHelper.id2Str(240312), 1.0f, Event.Success));
					return;
				}

				//获取版本配置文件/
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(thing = GHelper.id2Str(240295), 0, Event.DownConfig));
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
				mDFList = mPatch.listDownFiles(onCheckFileProgress,ref bCancel);
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
				int updateBytes = getTotalBytes(mNewResVer, totalBytes);
				if(updateBytes>=totalBytes)
				{
					downBytes = updateBytes - totalBytes;
					totalBytes = updateBytes;
				}
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(thing=GHelper.id2Str(240302), 0, Event.CalcSize, totalBytes));
				System.GC.Collect();
				//开启文件下载线程/
				for (int i=0; i<mHWRS.Length; ++i) {
					Thread t = new Thread(new ParameterizedThreadStart(CDNFileDownThread));
					threadLs.Add(t);
					t.Start(i);
				}
			}
			catch(Exception e)
			{
				mError=true;
				mHWR=null;
				Log.e (e);
			}

			//通知显示资源下载进度/
			while (mExitCount<threadLs.Count)
			{
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue("", 0.99f*downBytes/totalBytes, Event.DownFile));
				Thread.Sleep(100);
			}

			//等待下载线程结束/
			for (int i=0; i<threadLs.Count; ++i)
			{
				threadLs[i].Join();
			}

			//通知下载结果/
			mPatch=null;
			Log.i("cdn main thread exit", Log.Tag.Update);
			if (!mError)
			{
				mVersion=mNewResVer;
                EventMgr.single.PostEvent ("UpdateEvent", new UpdateEventValue (GHelper.id2Str (240311), 0.99f, Event.DownFile));
				//清除无效的tmp文件
				DirectoryInfo dir = new DirectoryInfo(dataPath);
				FileInfo[] fis = dir.GetFiles("*.tmp",SearchOption.AllDirectories);
				for (int i=0,max=fis.Length; i<max; ++i) {fis[i].Delete();}
				//通知更新完成
                EventMgr.single.PostEvent ("UpdateEvent", new UpdateEventValue (GHelper.id2Str (240311), 1.0f, Event.Success));
			}
			else
			{
				Thread.Sleep(500);
				if(mErrorFile!=null)thing=mErrorFile;
				thing = bCancel?GHelper.id2Str(240310):thing+=GHelper.id2Str(240299)+GHelper.id2Str(240965);
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(thing, 0.99f*downBytes/totalBytes, Event.Failure));
			}
		}

		void CDNFileDownThread(object param)
		{
			int threadIdx = (int)param;
			Log.i("cdn sub thread start", Log.Tag.Update);
			XmlPatch.DFileInfo df = null;
			byte[] buf = new byte[RecvBuffSize];
			int pLen = dataPath.Length;
			while(!mError)
			{
				lock(mLock)
				{
					if(mDFList.Count<1)break;
					df = mDFList [0];
					mDFList.RemoveAt(0);
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
						mHWRS[threadIdx].KeepAlive = true;//保持连接，否则消耗服务器的连接数/
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
					if(!mPatch.verifyFile(savePath))
					{
						File.Delete(savePath);
						throw new Exception("data md5 error");
					}
				}
				catch(Exception e)
				{
					if(mErrorFile==null)mErrorFile="download "+System.IO.Path.GetFileName(df.path);
					mError=true;
					Log.e (e);
				}
				finally
				{
					if(null!=fs)fs.Close();
					if(null!=st)st.Close();
					if(null!=rt)rt.Close();
				}
			}
			Interlocked.Increment (ref mExitCount);
			Log.i("cdn sub thread exit", Log.Tag.Update);
		}

		int getTotalBytes(int ver, int total)
		{
			string path = dataPath+ver.ToString();
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
	#endif
	#endregion

	#region ZIP下载
		int partZipLength;
		string partZipMd5;
		public void downAssetBundleZip(string url, OnUpdate callback, int appResVersion, int len, string md5)
		{
			bCancel    = false;
			totalBytes = 1;
			downBytes  = 0;
			onUpdate   = callback;
			updateUrl  = url;
			partZipLength = len;
			partZipMd5 = md5;
			#if UNITY_ANDROID
			updateUrl=url+"res/android"+appResVersion+".part.zip";
			#elif UNITY_IPHONE
			updateUrl=url+"res/iphone"+appResVersion+".part.zip";
			#else
			updateUrl=url+"res/standalone"+appResVersion+".part.zip";
			#endif
			downThread = new Thread(new ThreadStart(zipUpdateThread));
			downThread.Start();
		}

		void zipUpdateThread()
		{
			int totalBytes = 0;
			int downBytes  = 0;
			string thing = "";
			HttpWebResponse rt = null;
			FileStream fs = null;
			bool error = false;
			try
			{
				//下载
				thing=GHelper.id2Str(240657);
				string fileName = System.IO.Path.GetFileName(updateUrl);
				string filePath = dataPath+fileName;
				fs = File.Exists(filePath)?new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Write):new FileStream(filePath, FileMode.Create);
				downBytes=(int)fs.Length;
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(GHelper.id2Str(240660), 0.99f*downBytes/partZipLength, Event.DownFile));
				mHWR = HttpWebRequest.Create (new Uri (updateUrl)) as HttpWebRequest;
				mHWR.KeepAlive=false;
				mHWR.Timeout=TimeOut;
				mHWR.ReadWriteTimeout=TimeOut;
				if(downBytes<partZipLength)
				{
					mHWR.AddRange("bytes",downBytes);//必须在GetResponse之前设置
					rt=mHWR.GetResponse() as HttpWebResponse;
					byte[] rBuf = new byte[RecvBuffSize];
					Stream st = rt.GetResponseStream();
					int len = (int)rt.ContentLength;
					totalBytes = len+downBytes;
                    EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(null, 0, Event.CalcSize, totalBytes));
					while(len>0)
					{
						int n = st.Read(rBuf, 0, len>RecvBuffSize?RecvBuffSize:len);
						if(n<=0)throw new Exception("HttpWebResponse read exception");
						fs.Write(rBuf,0,n);
						len -= n;
						downBytes+=n;
                        EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(null, 0.99f*downBytes/totalBytes, Event.DownFile));
					}
					rt.Close();
					rt=null;
				}
				fs.Close();
				fs=null;

				//校验
				thing=GHelper.id2Str(240658);
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(GHelper.id2Str(240661), 0, Event.CheckFile));
				if(partZipMd5!=FileUtils.GetMd5Hash(filePath))
				{
					File.Delete(filePath);
					throw new Exception("file md5 not match");
				}

				if(bCancel)throw new Exception("user cancel");

				//解压
				thing=GHelper.id2Str(240659);
                EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(GHelper.id2Str(240662), 0, Event.CalcSize, totalBytes));
				FileInfo f = new FileInfo (filePath);
				Stream stream = f.OpenRead ();
				if(!UnzipCach.unzipFile(stream, dataPath, false, onUnzipProgress))throw new Exception("unzip file failed");

				//删除
				File.WriteAllText(dataPath+"res.part","ok");
				File.Delete(filePath);
			}
			catch(Exception e)
			{
				error=true;
				if(null!=fs)fs.Close();
				if(null!=rt)rt.Close();
				Log.e (e);
			}

			mHWR = null;
			if(error)
			{
				thing = bCancel?GHelper.id2Str(240310):thing+=GHelper.id2Str(240299)+","+GHelper.id2Str(240663);
                EventMgr.single.PostEvent("UpdateEvent", new UpdateEventValue(thing, 0.99f*downBytes/totalBytes, Event.Failure));
			}
			else
			{
                EventMgr.single.PostEvent ("UpdateEvent", new UpdateEventValue (GHelper.id2Str (240311), 1.0f, Event.Success));
			}
		}

		void onUnzipProgress(float progress, int code)
		{
            if(code==0)EventMgr.single.PostEvent("UpdateEvent",new UpdateEventValue(null, progress, Event.UnzipFile));
		}
	#endregion
	}
}*/