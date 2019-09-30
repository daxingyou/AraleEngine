using MyEngine;
using quicksdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*=====json param key=====
token  sdk登录返回秘钥
uid    sdk登录用户id
server 游戏服务器id
level  角色等级
name   角色名称
==========================*/
public abstract class SDKInterface : QuickSDKListener
{
    protected static SDKInterface sdk;
    // Start is called before the first frame update
   void Awake()
    {
        sdk = this;
    }

    public static int DoCall(string method, string jsonParam)
    {
        Log.D("SDK DoCall metho="+method+",param="+jsonParam);
        switch(method)
        {
            case "pay"://支付
                return sdk.DoPay(jsonParam);
            case "login"://登录
                return sdk.DoLogin(jsonParam);
            case "logout"://注销
                return sdk.DoLogout(jsonParam);
            case "exit"://退出游戏
                return sdk.DoExit(jsonParam);
            case "role"://角色创建或更新
                return sdk.DoRole(jsonParam);
            default:
                return sdk.DoCallEx(method, jsonParam);
        }
    }

    protected virtual int DoCallEx(string metho, string jsonParam)
    {
        Debug.LogError("SDKInterface DoCall not support metho=" + metho);
        return 0;
    }

    protected virtual bool hasMethod(string name)
    {
        return false;
    }

    protected virtual int DoLogin(string jsonParam)
    {
        return 0;
    }

    protected virtual int DoLogout(string jsonParam)
    {
        return 0;
    }

    protected virtual int DoPay(string jsonParam)
    {
        return 0;
    }

    protected virtual int DoRole(string jsonParam)
    {
        return 0;
    }

    protected virtual int DoExit(string jsonParam)
    {
        return 0;
    }
}
