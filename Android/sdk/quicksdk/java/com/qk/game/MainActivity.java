package com.qk.game;

import android.app.Activity;
import android.os.Bundle;

import com.qk.unity.QuickUnityPlayerproxyActivity;
import com.quicksdk.utility.AppConfig;

public class MainActivity extends QuickUnityPlayerproxyActivity {
	
 public Activity activity;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
	}

	@Override
	public String getProductCode() {
		return AppConfig.getInstance().getConfigValue("product_code");
	}

	@Override
	public String getProductKey() {
		return AppConfig.getInstance().getConfigValue("product_key");
	}

}
