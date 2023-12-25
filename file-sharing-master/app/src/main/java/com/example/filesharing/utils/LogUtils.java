package com.example.filesharing.utils;

import android.util.Log;

public class LogUtils {

    private static final int CONTROL=0;

    private static final int V=1;
    private static final int D=2;
    private static final int I=3;
    private static final int W=4;
    private static final int E=5;

    public static void logV(String TAG,String content){
        if(V>CONTROL){
            Log.v(TAG,content);
        }
    }

    public static void logD(String TAG,String content){
        if(D>CONTROL){
            Log.d(TAG, content);
        }
    }

    public static void logI(String TAG,String content){
        if(I>CONTROL){
            Log.i(TAG, content);
        }
    }

    public static void logW(String TAG,String content){
        if(W>CONTROL){
            Log.w(TAG, content);
        }
    }

    public static void logE(String TAG,String content){
        if (E>CONTROL){
            Log.e(TAG, content);
        }
    }
}
