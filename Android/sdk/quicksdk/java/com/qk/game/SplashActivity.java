package com.qk.game;

import android.content.Intent;

import com.quicksdk.QuickSdkSplashActivity;

public class SplashActivity extends QuickSdkSplashActivity {

    @Override
    public int getBackgroundColor() {
        return 0;
    }

    @Override
    public void onSplashStop() {
        Intent intent = new Intent(this, MainActivity.class);
        startActivity(intent);
        finish();
    }
}
