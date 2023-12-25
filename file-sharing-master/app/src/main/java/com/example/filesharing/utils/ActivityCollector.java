package com.example.filesharing.utils;

import android.app.Activity;
import android.os.Process;

import java.util.ArrayList;
import java.util.List;

public class ActivityCollector {
    private static List<Activity> mList=new ArrayList<>();

    public static void add(Activity activity){
        mList.add(activity);
    }

    public static void remove(Activity activity){
        mList.remove(activity);
    }

    public static void exit(){
        for (int i = 0; i < mList.size(); i++) {//遍历所有活动，统统销毁
            Activity activity = mList.get(i);
            activity.finish();
        }
        Process.killProcess(Process.myPid());//杀掉当前的进程
    }
}
