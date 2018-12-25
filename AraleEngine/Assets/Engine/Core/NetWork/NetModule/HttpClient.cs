using UnityEngine;
using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace Arale.Engine
{
    public class HttpClient : NetClient
    {
		public class HttpWebClient : WebClient
		{
			private CookieContainer mCookieContainer;

			public HttpWebClient()
			{
				mCookieContainer = new CookieContainer();
			}

			protected override WebRequest GetWebRequest(System.Uri address)
			{
				WebRequest request = base.GetWebRequest(address);
				if (request is HttpWebRequest)
				{
					HttpWebRequest httprequest = request as HttpWebRequest;
					httprequest.CookieContainer = mCookieContainer;
					httprequest.KeepAlive = true;
					request.Timeout = (int)(1000 * NetworkMgr.single.mTimeout);
				}
				return request;
			}
		}

        protected HttpWebClient mWebClient;
        protected Uri mUrl;
        protected string mSession;
        protected DateTime mLastTime;

        internal HttpClient(string url)
        {
            mWebClient = new HttpWebClient();
            mWebClient.UploadDataCompleted += httpWebClient_UploadDataCompleted;
            mWebClient.Headers.Set("Content-Type", "application/x-www-form-urlencoded");
            mWebClient.Headers.Set("KeepAlive", "TRUE");

            mUrl = new Uri(url);
            mReadySend = true;
        }

        // 发送消息队列中的第一条消息
        protected override void processSendData()
        {
            try
            {
                if (mSendList.Count <= 0)
                    return;

                if (null != mSession)
                {
                    mWebClient.Headers.Set("SESSIONID", mSession);
                }
				Packet packet = mSendList[0] as Packet;
				mWebClient.UploadDataAsync(mUrl, packet.mNetData);
                mLastTime = DateTime.Now;
            }
            catch (System.Exception e)
            {
                Log.e(e,  Log.Tag.Net);
				//addErrorPacket(ErrorType.SendException);
            }
            finally
            {
                mReadySend = false;
            }
        }

        protected virtual void httpWebClient_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                WebClient client = (WebClient)sender;
                // 正常收到服务器回复的消息
                if (e.Result != null && e.Result.Length > 0)
                {                   
                    Log.d("<<<<<<<<<Http MSG Len=" + e.Result.Length, Log.Tag.Net);
                    //更新session
                    string session = client.ResponseHeaders.Get("SESSIONID");
                    if (null == mSession && null != session)
                    {
                        mSession = session;
                        Log.d("SESSIONID:" + session, Log.Tag.Net);
                    }
                    // 加入到等待处理的消息列表
                    AddPacketResult(e.Result);
                    mSendList.RemoveAt(0);
					mReadySend = true;
                }
                else
                {
                    // 服务器返回消息的内容错误，尝试重连
                    Log.d("Http Received Packet Context Error[1].", Log.Tag.Net);
                    Reconnection();
                }
            }
            else
            {
                // 链接错误，尝试重连
                Log.d("Http Received Packet Error[2]: " + e.Error, Log.Tag.Net);
                Reconnection();
            }

            mReadySend = true;
        }

        private void Reconnection()
        {
        }

        public override void clear()
        {
            mSendList.Clear();
            mRecvList.Clear();
            mWebClient.CancelAsync();
        }
    }
}

