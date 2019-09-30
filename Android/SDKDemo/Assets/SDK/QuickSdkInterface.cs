using quicksdk;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSdkInterface : SDKInterface
{
    private void Start()
    {
        QuickSDK.getInstance().init();
        QuickSDK.getInstance().setListener(this);
    }
    protected override int DoLogin(string jsonParam)
    {
        QuickSDK.getInstance().login();
        return 0;
    }

    protected override int DoLogout(string jsonParam)
    {
        QuickSDK.getInstance().logout();
        return 0;
    }

    protected override int DoPay(string jsonParam)
    {
        quicksdk.OrderInfo orderInfo = new quicksdk.OrderInfo();
        GameRoleInfo gameRoleInfo = new GameRoleInfo();
        /*orderInfo.goodsID = "1";
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
        gameRoleInfo.gameRoleID = 
        gameRoleInfo.gameRoleLevel = "1";
        gameRoleInfo.gameRoleName = "钱多多";
        gameRoleInfo.partyName = "同济会";
        gameRoleInfo.serverID = "1";
        gameRoleInfo.serverName = "火星服务器";
        gameRoleInfo.vipLevel = "1";
        gameRoleInfo.roleCreateTime = "roleCreateTime";*/
        QuickSDK.getInstance().pay(orderInfo, gameRoleInfo);
        return 0;
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
    }

    public override void onLoginFailed(ErrorMsg message)
    {
        Debug.LogError("登录失败, msg: " + message);
    }

    public override void onLogoutSuccess()
    {
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
        Debug.Log("成功:" + message);
    }

    public override void onFailed(string message)
    {
        Debug.LogError("失败:" + message);
    }
    #endregion
}
