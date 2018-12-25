using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System;
using ICSharpCode.SharpZipLib.Core;
using UnityEngine;
using Arale.Engine;

namespace ICSharpCode.SharpZipLib.Zip
{

public class UnzipCach{
	public delegate void OnUnzipProgress(float progress, int code);
	OnUnzipProgress onUnzipProgress;
	List<FileItem> files = new List<FileItem>();
	class FileItem
	{
		public string path;
		public int    size;
		public FileItem(string path, int size)
		{
			this.path = path;
			this.size = size;
		}
	}
	
	static bool delayForHuaWeiHonor6Plus;
	const int bufferSize = 8*1024*1024;
	const int minBlockSz = 4096;
	byte[] buffer = new byte[bufferSize];
	int    totalBytes;
	int    writeBytes;
	int    readBytes;
	int    unzipSize;
	int    saveSize;
	int    updateSize;
	bool   overwrite;
	State  mState;
	Thread thread;
	System.Object mLock = new System.Object();
	
	enum State
	{
		Running,
		Error,
		Ok,
	};

	void setState(State state)
	{
		lock (mLock)
		{
			if(mState!=State.Error)mState = state;
		}
	}

	bool isError()
	{
		return mState == State.Error;
	}
	
	void addFile(string path, int sz, Stream stream)
	{
		unzipSize+=sz;
		lock (mLock)
		{
			files.Add(new FileItem(path,sz));
		}
		
		int rest=0;
		int fsize = 0;
		while (fsize<sz && !isError())
		{
			int offset = 0;
			bool wait=false;
			lock (mLock)
			{
				rest =  bufferSize - (writeBytes-readBytes);
				offset = writeBytes%bufferSize;
				wait=rest<1?true:false;
			}
			
			if(wait)
			{
				Thread.Sleep(1);
				continue;
			}

			if(offset+rest>bufferSize)rest=bufferSize-offset;
			rest = stream.Read(buffer, offset, rest);
			if(rest<0)throw new Exception(path);
			
			lock (mLock)
			{
				if(rest>0)
				{
					writeBytes+=rest;
					fsize+=rest;
				}
			}
		}
	}
	
	void write(FileItem file)
	{
		if(delayForHuaWeiHonor6Plus)Thread.Sleep(10);
		FileStream fos = null;
		string path = overwrite?file.path:file.path+".tmp";
		bool ow = overwrite || (!File.Exists (file.path));
		if(ow)fos = new FileStream(path, FileMode.Create);
		while(file.size>0 && !isError())
		{
			int rest = 0;
			int offset = 0;
			bool wait=false;
			lock (mLock)
			{
				rest = writeBytes - readBytes;
				offset = readBytes%bufferSize;
				wait=rest<file.size&&rest<minBlockSz?true:false;
			}

			if(wait)
			{
				Thread.Sleep(1);
				continue;
			}
			
			if(rest>file.size)rest=file.size;
			if(offset+rest>bufferSize)rest=bufferSize-offset;
			if(ow)fos.Write(buffer, offset, rest);
			file.size-=rest;
			saveSize+=rest;
			updateSize+=rest;
			if(updateSize>1024*1024)
			{
				updateSize=0;
				onUnzipProgress(0.99f*saveSize/totalBytes,0);
			}
			lock (mLock)
			{
				readBytes+=rest;
			}
		}
		if(ow)
		{
			fos.Close();
			if(!overwrite)File.Move(path, file.path);
		}
	}
	
	void start(int totalBytes, OnUnzipProgress onUnzipProgress, bool overwrite=true)
	{
		this.totalBytes = totalBytes;
		this.onUnzipProgress = onUnzipProgress;
		this.overwrite = overwrite;
		setState(State.Running);
		thread = new Thread(new ThreadStart(writeThreadFunc));   
		thread.Start();
	}
	
	bool stop()
	{
		thread.Join();
		if(mState==State.Ok)
		{
			//LogManager.GetInstance().LogMessage("unzip ok unzipSize="+unzipSize+",saveSize="+saveSize, LogManager.ModuleFilter.RES);
			return true;
		}
		else
		{
			//LogManager.GetInstance().LogError("unzip error", LogManager.ModuleFilter.RES);
			delayForHuaWeiHonor6Plus=true;
			return false;
		}
		
	}
	
	void writeThreadFunc()
	{
		FileItem fi = null;
		do
		{
			try 
			{
				fi = null;
				lock (mLock)
				{
					if(files.Count>0)
					{
						fi = files[0];
						files.RemoveAt(0);
					}
					else
					{
						if(mState!=State.Running)break;
					}
				}
				
				if(fi!=null)
				{
					write(fi);
				}
				else
				{	
					Thread.Sleep(1);
				}
			}
			catch (Exception e) 
			{
				setState(State.Error);
				Log.e(e);
			}
			
			if(isError())break;
		}
		while(mState==State.Running||fi!=null);
	}
	
	static Stream _inputStream;
	static string _tmpDirectory;
	static string _targetDirectory;
	static OnUnzipProgress _callback;
	public static void unzipFile(Stream inputStream, string tmpDirectory, string targetDirectory, OnUnzipProgress callback)
	{
		_inputStream = inputStream;
		_tmpDirectory = tmpDirectory;
		_targetDirectory = targetDirectory;
		_callback = callback;
		new Thread(new ThreadStart(unzipFileThread)).Start();
	}

	public static bool unzipFile(Stream inputStream, string targetDirectory, bool overwrite, OnUnzipProgress callback)
	{
		bool ret=false;
		callback(0,0);
		using (ZipFile zipFile_ = new ZipFile(inputStream)) 
		{
			int totalBytes = (int)zipFile_.unzipSize;
			UnzipCach cach =  new UnzipCach();
			cach.start(totalBytes, callback, overwrite);
			
			INameTransform extractNameTransform_ = new WindowsNameTransform(targetDirectory);
			System.Collections.IEnumerator enumerator = zipFile_.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					ZipEntry entry = (ZipEntry)enumerator.Current;
					if (entry.IsFile)
					{
						string fileName = extractNameTransform_.TransformFile(entry.Name);
						string dirName = Path.GetDirectoryName(Path.GetFullPath(fileName));
						if(!Directory.Exists(dirName))Directory.CreateDirectory(dirName);
						Stream source = zipFile_.GetInputStream(entry);
						cach.addFile(fileName,(int)entry.Size, source);
						source.Close();
					}
					else
					{
						string dirName = extractNameTransform_.TransformDirectory(entry.Name);
						if(!Directory.Exists(dirName))Directory.CreateDirectory(dirName);
					}
				}
				catch (Exception e)
				{
					cach.setState(UnzipCach.State.Error);
					//LogManager.GetInstance().LogException(e.Message, e, LogManager.ModuleFilter.RES);
				}
				
				if(cach.isError())break;
			}
			
			cach.setState(UnzipCach.State.Ok);
			ret = cach.stop();
			callback(1,ret?0:1);
			return ret;
		}
	}
	
	static void unzipFileThread()
	{
		//LogManager.GetInstance().LogMessage("unzip begin outpath="+_targetDirectory, LogManager.ModuleFilter.RES);
		_callback(0,0);
		bool ret=false;
		using (ZipFile zipFile_ = new ZipFile(_inputStream))
		{
			int totalBytes = (int)zipFile_.unzipSize;
			UnzipCach cach =  new UnzipCach();
			cach.start(totalBytes, _callback);
			
			INameTransform extractNameTransform_ = new WindowsNameTransform(_tmpDirectory);
			System.Collections.IEnumerator enumerator = zipFile_.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					ZipEntry entry = (ZipEntry)enumerator.Current;
					if (entry.IsFile)
					{
						string fileName = extractNameTransform_.TransformFile(entry.Name);
						string dirName = Path.GetDirectoryName(Path.GetFullPath(fileName));
						if(!Directory.Exists(dirName))Directory.CreateDirectory(dirName);
						Stream source = zipFile_.GetInputStream(entry);
						cach.addFile(fileName,(int)entry.Size, source);
						source.Close();
					}
					else
					{
						string dirName = extractNameTransform_.TransformDirectory(entry.Name);
						if(!Directory.Exists(dirName))Directory.CreateDirectory(dirName);
					}
				}
				catch (Exception e)
				{
					cach.setState(UnzipCach.State.Error);
					//LogManager.GetInstance().LogException(e.Message, e, LogManager.ModuleFilter.RES);
				}

				if(cach.isError())break;
			}

			cach.setState(UnzipCach.State.Ok);
			if(cach.stop())
			{
				try
				{
					Directory.Move(_tmpDirectory, _targetDirectory);
					ret=true;
				}
				catch(Exception e)
				{
					//LogManager.GetInstance().LogException("unzip rename dir error.", e);
				}
			}
			_callback(1,ret?0:1);
			//LogManager.GetInstance().LogMessage("unzip end", LogManager.ModuleFilter.RES);
			_inputStream = null;
			_callback = null;
		}
	}
}

}
