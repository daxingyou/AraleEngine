using quicksdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AraleSdkDemo : QuickSDKListener
{
    public GameObject pageLogin;
    public GameObject pageMain;
    public GameObject pageExit;
    void Start()
    {
        pageLogin.SetActive(true);
        pageMain.SetActive(false);
        pageExit.SetActive(false);
        QuickSDK.getInstance().init();
        QuickSDK.getInstance().setListener(this);
    }

    public void doSdkLogin()
    {
        QuickSDK.getInstance().login();
    }

    public void doSdKPay()
    {
        OrderInfo orderInfo = new OrderInfo();
        GameRoleInfo gameRoleInfo = new GameRoleInfo();

        orderInfo.goodsID = "1";
        orderInfo.goodsName = "勾玉";
        orderInfo.goodsDesc = "10个勾玉";
        orderInfo.quantifier = "个";
        orderInfo.extrasParams = "extparma";
        orderInfo.count = 10;
        orderInfo.amount = 1;
        orderInfo.price = 0.1f;
        orderInfo.callbackUrl = "";
        orderInfo.cpOrderID = "cporderidzzw";

        gameRoleInfo.gameRoleBalance = "0";
        gameRoleInfo.gameRoleID = "000001";
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "钱多多";
        gameRoleInfo.partyName = "同济会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "火星服务器";
        gameRoleInfo.vipLevel = "1";
        gameRoleInfo.roleCreateTime = "roleCreateTime";
        QuickSDK.getInstance().pay(orderInfo, gameRoleInfo);
    }

    public void doSdkLogout()
    {
        QuickSDK.getInstance().logout();
        pageLogin.SetActive(true);
        pageMain.SetActive(false);
    }

    public void doSdkExitGame()
    {
        if (QuickSDK.getInstance().isChannelHasExitDialog())
        {
            QuickSDK.getInstance().exit();
        }
        else
        {
            //显示自己的退出确认框
            pageExit.SetActive(true);
        }
    }

    public void doExitConfirm()
    {
        pageExit.SetActive(false);
        QuickSDK.getInstance().exit();
    }

    public void doExitCancel()
    {
        pageExit.SetActive(false);
    }

    #region QuickSDKListener
    public override void onInitSuccess()
    {
        Debug.Log("初始化完成");
    }

    public override void onInitFailed(ErrorMsg message)
    {
        Debug.LogError("初始化失败, msg: " + message);
    }

    public override void onLoginSuccess(UserInfo userInfo)
    {
        Debug.Log("登录成功:uid: " + userInfo.uid + " ,username: " + userInfo.userName + " ,userToken: " + userInfo.token + ", msg: " + userInfo.errMsg);
        //发送token到服务器,登录游戏服
        GameRoleInfo gameRoleInfo = new GameRoleInfo();

        gameRoleInfo.gameRoleBalance = "0";
        gameRoleInfo.gameRoleID = "000001";
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "钱多多";
        gameRoleInfo.partyName = "同济会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "火星服务器";
        gameRoleInfo.vipLevel = "1";
        gameRoleInfo.roleCreateTime = "roleCreateTime";//UC与1881渠道必传，值为10位数时间戳

        gameRoleInfo.gameRoleGender = "男";//360渠道参数
        gameRoleInfo.gameRolePower = "38";//360渠道参数，设置角色战力，必须为整型字符串
        gameRoleInfo.partyId = "1100";//360渠道参数，设置帮派id，必须为整型字符串
        gameRoleInfo.professionId = "11";//360渠道参数，设置角色职业id，必须为整型字符串
        gameRoleInfo.profession = "法师";//360渠道参数，设置角色职业名称
        gameRoleInfo.partyRoleId = "1";//360渠道参数，设置角色在帮派中的id
        gameRoleInfo.partyRoleName = "帮主"; //360渠道参数，设置角色在帮派中的名称
        gameRoleInfo.friendlist = "无";//360渠道参数，设置好友关系列表，格式请参考：http://open.quicksdk.net/help/detail/aid/190
        QuickSDK.getInstance().enterGame(gameRoleInfo);//开始游戏

        pageLogin.SetActive(false);
        pageMain.SetActive(true);
    }

    public override void onLoginFailed(ErrorMsg message)
    {
        Debug.LogError("登录失败, msg: " + message);
    }

    public override void onLogoutSuccess()
    {
        pageLogin.SetActive(true);
        pageMain.SetActive(false);
    }

    public override void onSwitchAccountSuccess(UserInfo userInfo)
    {

    }

    public override void onPaySuccess(PayResult payResult)
    {
        Debug.Log("支付成功 orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
    }

    public override void onPayFailed(PayResult payResult)
    {
        Debug.LogError("支付失败 orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
    }

    public override void onPayCancel(PayResult payResult)
    {
        Debug.Log("支付取消 orderId: " + payResult.orderId + ", cpOrderId: " + payResult.cpOrderId + " ,extraParam" + payResult.extraParam);
    }

    public override void onExitSuccess()
    {
        QuickSDK.getInstance().exitGame();
    }

    public override void onSucceed(string message)
    {
        Debug.Log("成功:"+ message);
    }

    public override void onFailed(string message)
    {
        Debug.LogError("失败:" + message);
    }
    #endregion
}
