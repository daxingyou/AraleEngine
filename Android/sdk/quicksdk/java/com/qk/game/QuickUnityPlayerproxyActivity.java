package com.qk.unity;

import java.util.Dictionary;
import java.util.Enumeration;
import java.util.HashMap;

import org.json.JSONObject;

import android.Manifest;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.res.Configuration;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.os.Handler.Callback;
import android.os.Message;
import android.provider.Settings;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.text.TextUtils;
import android.util.Log;
import android.widget.Toast;

import com.quicksdk.Extend;
import com.quicksdk.FuncType;
import com.quicksdk.Payment;
import com.quicksdk.QuickSDK;
import com.quicksdk.Sdk;
import com.quicksdk.User;
import com.quicksdk.entity.GameRoleInfo;
import com.quicksdk.entity.OrderInfo;
import com.quicksdk.entity.ShareInfo;
import com.quicksdk.entity.UserInfo;
import com.quicksdk.notifier.ExitNotifier;
import com.quicksdk.notifier.InitNotifier;
import com.quicksdk.notifier.LoginNotifier;
import com.quicksdk.notifier.LogoutNotifier;
import com.quicksdk.notifier.PayNotifier;
import com.quicksdk.notifier.SwitchAccountNotifier;
import com.quicksdk.utility.AppConfig;
import com.unity3d.player.UnityPlayer;

public abstract class QuickUnityPlayerproxyActivity extends com.unity3d.player.UnityPlayerActivity implements Callback {

	private static final String TAG = "unity.support";

	private static final int MSG_LOGIN = 101;
	private static final int MSG_LOGOUT = 102;
	private static final int MSG_PAY = 103;
	private static final int MSG_EXIT = 104;
	private static final int MSG_ROLEINFO = 105;
	private static final int MSG_EXTEND_FUNC = 106;
	private static final int MSG_EXTEND_CALLPLUGIN = 107;
	private static final int MSG_EXTEND_FUNC_SHARE = 108;
	
	private final int REQUEST_RECORD_PERMISSION_SETTING = 999;

	private static final int INIT_SUCCESS = 1;
	private static final int INIT_FAILED = -1;
	private static final int INIT_DEFAULT = 0;

	private QuickUnityInitNotify initNotify = new QuickUnityInitNotify();
	private QuickUnityLoginNotify loginNotify = new QuickUnityLoginNotify();
	private QuickUnitySwitchAccountNotify switchAccountNotify = new QuickUnitySwitchAccountNotify();
	private QuickUnityLogoutNotify logoutNotify = new QuickUnityLogoutNotify();
	private QuickUnityPayNotify payNotify = new QuickUnityPayNotify();
	private QuickUnityExitNotiry exitNotiry = new QuickUnityExitNotiry();

	private String gameObjectName;

	private int initState = INIT_DEFAULT;

	private String mInitMsg = "";

	Handler mHandler = new Handler(this);
	
	boolean isLancScape=true ;
	
//	

	@Override
	protected void onCreate(Bundle bundle) {
		Sdk.getInstance().onCreate(this);
		super.onCreate(bundle);

		doInit();
	}

	@Override
	protected void onRestart() {
		Sdk.getInstance().onRestart(this);
		super.onRestart();
	
	}

	@Override
	protected void onStart() {
		Sdk.getInstance().onStart(this);
		super.onStart();

	}

	@Override
	protected void onResume() {
		Sdk.getInstance().onResume(this);
		super.onResume();

	}

	@Override
	protected void onNewIntent(Intent intent) {
		Sdk.getInstance().onNewIntent(intent);
		super.onNewIntent(intent);

	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {

		super.onActivityResult(requestCode, resultCode, data);
		Sdk.getInstance().onActivityResult(this, requestCode, resultCode, data);

	}

	@Override
	protected void onPause() {
		Sdk.getInstance().onPause(this);
		super.onPause();

	}

	@Override
	protected void onStop() {
		Sdk.getInstance().onStop(this);
		super.onStop();

	}

	@Override
	protected void onDestroy() {

		Sdk.getInstance().onDestroy(this);
		super.onDestroy();

	}

	public void doInit() {
		 isLancScape = QuickUnityPlayerproxyActivity.this.getResources().getConfiguration().orientation == Configuration.ORIENTATION_LANDSCAPE;
		
	try {
	
		
			//check权限
			if ((ContextCompat.checkSelfPermission(QuickUnityPlayerproxyActivity.this, Manifest.permission.READ_PHONE_STATE) != PackageManager.PERMISSION_GRANTED)
					|| (ContextCompat.checkSelfPermission(QuickUnityPlayerproxyActivity.this, Manifest.permission.WRITE_EXTERNAL_STORAGE) != PackageManager.PERMISSION_GRANTED)) {
		//没有 ，  申请权限  权限数组
				ActivityCompat.requestPermissions(QuickUnityPlayerproxyActivity.this, new String[] { Manifest.permission.READ_PHONE_STATE ,Manifest.permission.WRITE_EXTERNAL_STORAGE}, 1);
			} else {
				QuickSDK.getInstance().setInitNotifier(initNotify).setLoginNotifier(loginNotify).setLogoutNotifier(logoutNotify).setPayNotifier(payNotify)
				.setExitNotifier(exitNotiry).setIsLandScape(isLancScape).setSwitchAccountNotifier(switchAccountNotify);
				Sdk.getInstance().init(this, getProductCode(), getProductKey());
			}
		} catch (Exception e) {		
			QuickSDK.getInstance().setInitNotifier(initNotify).setLoginNotifier(loginNotify).setLogoutNotifier(logoutNotify).setPayNotifier(payNotify)
			.setExitNotifier(exitNotiry).setIsLandScape(isLancScape).setSwitchAccountNotifier(switchAccountNotify);
			Sdk.getInstance().init(this, getProductCode(), getProductKey());
		}


	
	}
	
	//申请权限的回调（结果）
	@Override
	public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
		if (grantResults[0] == PackageManager.PERMISSION_GRANTED) {
	//申请成功
			QuickSDK.getInstance().setInitNotifier(initNotify).setLoginNotifier(loginNotify).setLogoutNotifier(logoutNotify).setPayNotifier(payNotify)
			.setExitNotifier(exitNotiry).setIsLandScape(isLancScape).setSwitchAccountNotifier(switchAccountNotify);
			Sdk.getInstance().init(this, getProductCode(), getProductKey());
		} else {
			//失败  这里逻辑以游戏为准 这里只是模拟申请失败 退出游戏    cp方可改为继续申请 或者其他逻辑
		
			
			Log.e("Unity", "onRequestPermissionsResult Fail");
			if (ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.WRITE_EXTERNAL_STORAGE)&&ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.READ_PHONE_STATE)) {
				Log.d("Unity", "ActivityCompat shouldShowRequestPermissionRationale true");
			} else {
				Log.e("Unity", "ActivityCompat shouldShowRequestPermissionRationale false");
				final AlertDialog.Builder normalDialog = new AlertDialog.Builder(QuickUnityPlayerproxyActivity.this);
				normalDialog.setTitle("权限设置");
				normalDialog.setMessage("请在设置中打开权限");
				normalDialog.setPositiveButton("前往应用设置", new DialogInterface.OnClickListener() {
					@Override
					public void onClick(DialogInterface dialog, int which) {
						// ...To-do
						Intent intent = new Intent(Settings.ACTION_APPLICATION_DETAILS_SETTINGS);
						Uri uri = Uri.fromParts("package", getPackageName(), null);
						intent.setData(uri);
						startActivityForResult(intent, REQUEST_RECORD_PERMISSION_SETTING);
						QuickSDK.getInstance().setInitNotifier(initNotify).setLoginNotifier(loginNotify).setLogoutNotifier(logoutNotify).setPayNotifier(payNotify)
						.setExitNotifier(exitNotiry).setIsLandScape(isLancScape).setSwitchAccountNotifier(switchAccountNotify);
						Sdk.getInstance().init(QuickUnityPlayerproxyActivity.this, getProductCode(), getProductKey());
					}
				});
				normalDialog.setNegativeButton("关闭", new DialogInterface.OnClickListener() {
					@Override
					public void onClick(DialogInterface dialog, int which) {
						Toast.makeText(QuickUnityPlayerproxyActivity.this, "权限被拒绝", Toast.LENGTH_SHORT).show();
						QuickSDK.getInstance().setInitNotifier(initNotify).setLoginNotifier(loginNotify).setLogoutNotifier(logoutNotify).setPayNotifier(payNotify)
						.setExitNotifier(exitNotiry).setIsLandScape(isLancScape).setSwitchAccountNotifier(switchAccountNotify);
						Sdk.getInstance().init(QuickUnityPlayerproxyActivity.this, getProductCode(), getProductKey());
					}
				});
				// 显示
				normalDialog.show();
			}
		}
	}

	// ------------------------------------------------------------------------------------
	// -------------------------------------------------------------------------------unity
	// interface
	// ------------------------------------------------------------------------------------

	public void requestLogin() {
		mHandler.sendEmptyMessage(MSG_LOGIN);

	}

	public void requestLogout() {
		mHandler.sendEmptyMessage(MSG_LOGOUT);
		
	}

	public void requestPay(String goodsId, String goodsName, String goodsDesc, String quantifier, String cpOrderId, String callbackUrl,
			String extraParams, String price, String amount, String count, String serverName, String serverId, String roleName, String roleId,
			String roleBalance, String vipLevel, String roleLevel, String partyName, String roleCreateTime) {

		OrderInfo orderInfo = new OrderInfo();
		orderInfo.setGoodsName(goodsName);
		orderInfo.setGoodsID(goodsId);
		orderInfo.setGoodsDesc(goodsDesc);
		orderInfo.setQuantifier(quantifier);
		orderInfo.setCpOrderID(cpOrderId);
		orderInfo.setCallbackUrl(callbackUrl);
		orderInfo.setExtrasParams(extraParams);
		orderInfo.setPrice(Double.valueOf(price));
		orderInfo.setAmount(Double.valueOf(amount));
		orderInfo.setCount(Integer.valueOf(count));

		GameRoleInfo roleInfo = new GameRoleInfo();
		roleInfo.setServerID(serverId);
		roleInfo.setServerName(serverName);
		roleInfo.setGameRoleID(roleId);
		roleInfo.setGameRoleName(roleName);
		roleInfo.setGameUserLevel(roleLevel);
		roleInfo.setGameBalance(roleBalance);
		roleInfo.setVipLevel(vipLevel);
		roleInfo.setPartyName(partyName);
		roleInfo.setRoleCreateTime(roleCreateTime);

		Message msg = mHandler.obtainMessage(MSG_PAY);
		HashMap<String, Object> mapObj = new HashMap<String, Object>();
		mapObj.put("orderInfo", orderInfo);
		mapObj.put("roleInfo", roleInfo);
		msg.obj = mapObj;
		msg.sendToTarget();

	}

	public void requestExit() {
		mHandler.sendEmptyMessage(MSG_EXIT);
	}
	
	public void requestCallSDKShare(String title,String content,String imgPath,String imgUrl,String url,String type,String shareTo,String extenal) {
		ShareInfo shareInfo=new ShareInfo();
		shareInfo.setTitle(title);
		shareInfo.setContent(content);
		shareInfo.setImgPath(imgPath);
		shareInfo.setImgUrl(imgUrl);
		shareInfo.setUrl(url);
		shareInfo.setType(type);
		shareInfo.setShareTo(shareTo);
		shareInfo.setExtenal(extenal);
		
		Message msg = mHandler.obtainMessage(MSG_EXTEND_FUNC_SHARE);
		msg.obj = shareInfo;
		msg.sendToTarget();
	
	}
	
	
	public void requestCallCustomPlugin(final String roleId,final String roleName,final String serverName,final String vip) {
		QuickUnityPlayerproxyActivity.this.runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
			Extend.getInstance().callPlugin(QuickUnityPlayerproxyActivity.this, FuncType.CUSTOM, roleId,roleName,serverName,vip);
			}
		});
	}
	
	public String getUserId() {

		if (User.getInstance().getUserInfo(this) != null) {
			Log.e(TAG, "userId->" + User.getInstance().getUserInfo(this).getUID());
			return User.getInstance().getUserInfo(this).getUID();
		} else {
			Log.e(TAG, "user is null");
			return null;
		}
	}

	public void requestUpdateRole(String serverId, String serverName, String roleName, String roleId, String roleBalance, String vipLevel,
			String roleLevel, String partyName, String roleCreateTime, String roleGender, String rolePower, String partId, String professionId,
			String profession, String partyRoleId, String partyRoleName, String friendlist, String isCreate) {

		GameRoleInfo roleInfo = new GameRoleInfo();
		roleInfo.setServerID(serverId);
		roleInfo.setServerName(serverName);
		roleInfo.setGameBalance(roleBalance);
		roleInfo.setGameRoleID(roleId);
		roleInfo.setGameRoleName(roleName);
		roleInfo.setVipLevel(vipLevel);
		roleInfo.setGameUserLevel(roleLevel);
		roleInfo.setPartyName(partyName);
		roleInfo.setRoleCreateTime(roleCreateTime);

		roleInfo.setGameRoleGender(roleGender);
		roleInfo.setGameRolePower(rolePower);
		roleInfo.setPartyId(partId);

		roleInfo.setProfessionId(professionId);
		roleInfo.setProfession(profession);
		roleInfo.setPartyId(partyRoleId);
		roleInfo.setPartyRoleName(partyRoleName);
		roleInfo.setFriendlist(friendlist);

		boolean isCreateRole = false;
		if (TextUtils.isEmpty(isCreate) || "false".equalsIgnoreCase(isCreate)) {
			isCreateRole = false;
		} else if ("true".equalsIgnoreCase(isCreate)) {
			isCreateRole = true;
		}

		Message msg = mHandler.obtainMessage(MSG_ROLEINFO);
		if (isCreateRole) {
			msg.arg1 = 1;
		} else {
			msg.arg1 = 0;
		}
		msg.obj = roleInfo;
		msg.sendToTarget();

	}

	public int callFunc(int funcType) {
		if (isFuncSupport(funcType)) {

			Message msg = mHandler.obtainMessage(MSG_EXTEND_FUNC);
			msg.arg1 = funcType;
			msg.sendToTarget();

			return 1;
		} else {
			return 0;
		}
	}

	public boolean isFuncSupport(int funcType) {

		return Extend.getInstance().isFunctionSupported(funcType);
	}

	public void setUnityGameObjectName(String gameObjectName) {
		this.gameObjectName = gameObjectName;
		Log.d("lyy", "gameObjectName=" + gameObjectName);
		switch (initState) {
		case INIT_SUCCESS:
			callUnityFunc("onInitSuccess", new JSONObject().toString());
			break;
		case INIT_FAILED:
			JSONObject json = new JSONObject();
			try {

				json.put("msg", mInitMsg);

			} catch (Exception e) {
				e.printStackTrace();
			}
			callUnityFunc("onInitFailed", new JSONObject().toString());
			break;

		default:
			break;
		}
		initState = INIT_DEFAULT;

	}

	public String getChannelName() {
		return AppConfig.getInstance().getConfigValue("qu" + "ic" + "ks" + "dk_cha" + "nnel_name");
	}

	public String getChannelVersion() {
		return AppConfig.getInstance().getChannelSdkVersion();
	}

	public int getChannelType() {
		return AppConfig.getInstance().getChannelType();
	}

	public String getSDKVersion() {
		return AppConfig.getInstance().getSdkVersion();
	}

	public String getConfigValue(String key) {
		return AppConfig.getInstance().getConfigValue(key);
	}

	public boolean isChannelHasExitDialog() {
		return QuickSDK.getInstance().isShowExitDialog();
	}

	public void exitGame() {
		Log.d("lyy", "调用了exitGame()");
		this.finish();
		System.exit(0);
	}

	// ------------------------------------------------------------------------------------
	// ------------------------------------------------------------------------------------
	// notifiers
	// ------------------------------------------------------------------------------------
	private class QuickUnityInitNotify implements InitNotifier {

		@Override
		public void onSuccess() {

			if (!TextUtils.isEmpty(gameObjectName)) {
				callUnityFunc("onInitSuccess", new JSONObject().toString());
			} else {
				initState = INIT_SUCCESS;
			}

		}

		@Override
		public void onFailed(String msg, String trace) {

			if (!TextUtils.isEmpty(gameObjectName)) {
				JSONObject json = new JSONObject();
				try {

					json.put("msg", msg);

				} catch (Exception e) {
					e.printStackTrace();
				}
				callUnityFunc("onInitFailed", new JSONObject().toString());
			} else {
				mInitMsg = msg;
				initState = INIT_FAILED;
			}

		}
	}

	private class QuickUnityLoginNotify implements LoginNotifier {

		@Override
		public void onSuccess(UserInfo userInfo) {

			JSONObject json = new JSONObject();
			try {

				json.put("userName", userInfo.getUserName() == null ? "" : userInfo.getUserName());
				json.put("userId", userInfo.getUID() == null ? "" : userInfo.getUID());
				json.put("userToken", userInfo.getToken() == null ? "" : userInfo.getToken());
				json.put("channelToken", userInfo.getChannelToken());

				json.put("msg", "success");

			} catch (Exception e) {
				e.printStackTrace();
			}
			callUnityFunc("onLoginSuccess", json.toString());
		}

		@Override
		public void onCancel() {
			JSONObject json = new JSONObject();
			try {

				json.put("msg", "cancel");

			} catch (Exception e) {
			}

			callUnityFunc("onLoginFailed", json.toString());
		}

		@Override
		public void onFailed(String msg, String trace) {

			JSONObject json = new JSONObject();
			try {

				json.put("msg", msg);

			} catch (Exception e) {
			}

			callUnityFunc("onLoginFailed", json.toString());
		}
	}

	private class QuickUnitySwitchAccountNotify implements SwitchAccountNotifier {

		@Override
		public void onCancel() {

		}

		@Override
		public void onFailed(String msg, String trace) {

		}

		@Override
		public void onSuccess(UserInfo userInfo) {
			Log.d("lyy", "切换账号成功");
			JSONObject json = new JSONObject();
			try {

				json.put("userName", userInfo.getUserName() == null ? "" : userInfo.getUserName());
				json.put("userId", userInfo.getUID() == null ? "" : userInfo.getUID());
				json.put("userToken", userInfo.getToken() == null ? "" : userInfo.getToken());
				json.put("channelToken", userInfo.getChannelToken());

				json.put("msg", "success");

			} catch (Exception e) {
				e.printStackTrace();
			}
			callUnityFunc("onSwitchAccountSuccess", json.toString());

		}

	}

	private class QuickUnityLogoutNotify implements LogoutNotifier {

		@Override
		public void onSuccess() {

			callUnityFunc("onLogoutSuccess", "success");
		}

		@Override
		public void onFailed(String msg, String trace) {

		}
	}

	private class QuickUnityPayNotify implements PayNotifier {

		@Override
		public void onSuccess(String quickOrderId, String cpOrderId, String extrasParams) {

			JSONObject json = new JSONObject();
			try {
				json.put("orderId", quickOrderId);
				json.put("cpOrderId", cpOrderId);
				json.put("extraParam", extrasParams);

			} catch (Exception e) {
			}

			callUnityFunc("onPaySuccess", json.toString());
		}

		@Override
		public void onCancel(String cpOrderID) {
			JSONObject json = new JSONObject();
			try {
				json.put("orderId", "");
				json.put("cpOrderId", cpOrderID);
				json.put("extraParam", "");

			} catch (Exception e) {
			}

			callUnityFunc("onPayCancel", json.toString());
		}

		@Override
		public void onFailed(String cpOrderID, String message, String trace) {
			JSONObject json = new JSONObject();
			try {
				json.put("orderId", "");
				json.put("cpOrderId", cpOrderID);
				json.put("extraParam", message);

			} catch (Exception e) {
			}

			callUnityFunc("onPayFailed", json.toString());
		}
	}

	private class QuickUnityExitNotiry implements ExitNotifier {

		@Override
		public void onSuccess() {
			callUnityFunc("onExitSuccess", "success");
		}

		@Override
		public void onFailed(String msg, String trace) {
		}
	}

	public void callUnityFunc(String funcName, String paramStr) {
		if (TextUtils.isEmpty(gameObjectName)) {
			Log.e(TAG, "gameObject is null, please set gameObject first");
			return;
		}

		UnityPlayer.UnitySendMessage(gameObjectName, funcName, paramStr);
	}

	@SuppressWarnings("unchecked")
	@Override
	public boolean handleMessage(Message msg) {
		switch (msg.what) {
		case MSG_LOGIN:

			Log.e(TAG, "login");
			User.getInstance().login(QuickUnityPlayerproxyActivity.this);
			break;
		case MSG_LOGOUT:

			Log.e(TAG, "logout");

			User.getInstance().logout(this);
			break;
		case MSG_PAY: {

			Log.e(TAG, "pay");

			HashMap<String, Object> mapObj = (HashMap<String, Object>) msg.obj;
			OrderInfo orderInfo = (OrderInfo) mapObj.get("orderInfo");
			GameRoleInfo roleInfo = (GameRoleInfo) mapObj.get("roleInfo");
			Payment.getInstance().pay(this, orderInfo, roleInfo);
			break;
		}
		case MSG_EXIT: {

			Log.e(TAG, "exit");

			Sdk.getInstance().exit(this);
			break;
		}
		case MSG_ROLEINFO: {

			Log.d(TAG, "update role info");

			GameRoleInfo roleInfo = (GameRoleInfo) msg.obj;
			boolean isCreate = (msg.arg1 == 1);

			User.getInstance().setGameRoleInfo(this, roleInfo, isCreate);
			break;
		}
		case MSG_EXTEND_FUNC: {

			int funcType = msg.arg1;
			Extend.getInstance().callFunction(this, funcType);
			break;
		}
		case MSG_EXTEND_FUNC_SHARE: {
			Log.e(TAG, "call channel share");
			ShareInfo shareInfo=(ShareInfo)msg.obj;
			Extend.getInstance().callFunctionWithParams(this, FuncType.SHARE, shareInfo);
			break;
		}
		
		default:
			break;
		}
		return false;
	}


	public abstract String getProductCode();

	public abstract String getProductKey();

}
