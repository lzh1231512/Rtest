package com.example.filesharing.receiver;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

import com.example.filesharing.MainActivity;
import com.example.filesharing.R;


/**
 *告知MainActivity一些关于服务器的状态信息
 */
public class MybroadcastReceiver extends BroadcastReceiver {
    private MainActivity mMainActivity;

    //构造器
    public MybroadcastReceiver(MainActivity mainActivity){
        mMainActivity=mainActivity;
    }

    //接收到广播要做的事情
    @Override
    public void onReceive(Context context, Intent intent) {
        String action = intent.getAction();
        if(action.equals("lyy")){
            int status = intent.getIntExtra("status", 0);
            switch (status){
                //启动标志
                case 1:
                    String hostAdress = intent.getStringExtra("msg");
                    mMainActivity.urlTxt.setText("服务器已经启动，访问地址是：\nhttp://"+hostAdress+":1080");
                    mMainActivity.controlBtn.setText(mMainActivity.getString(R.string.stop_server));
                    mMainActivity.isRunning=true;
                    break;
                //停止标志
                case 2:
                    mMainActivity.urlTxt.setText("服务器已经停止");
                    mMainActivity.controlBtn.setText(mMainActivity.getString(R.string.boot_server));
                    mMainActivity.isRunning=false;
                    break;
                //错误标志
                case 3:
                    String errMsg= intent.getStringExtra("msg");
                    mMainActivity.urlTxt.setText("抱歉，服务器出现错误了，错误信息："+errMsg);
                    mMainActivity.controlBtn.setText(mMainActivity.getString(R.string.boot_server));
                    mMainActivity.isRunning=false;
                    break;
                default:
            }
        }

    }

    /**
     * Service向MainActivity发送广播，告诉MainActivity服务器Server启动了，并携带了主机地址信息
     * @param context
     * @param hostAdress
     */
    public static void onServerStart(Context context,String hostAdress){
        sendBroadcast(context,1,hostAdress);
    }

    /**
     * Service向MainActivity发送广播，告诉MainActivity服务器Server停止了
     * @param context
     */
    public static void onServerStop(Context context){
        sendBroadcast(context,2);
    }

    /**
     * Service向MainActivity发送广播，告诉MainActivity服务器Server启动了，并携带服务器错误信息
     * @param context
     * @param errMsg
     */
    public static void onServerError(Context context,String errMsg){
        sendBroadcast(context,3,errMsg);
    }

    private static void sendBroadcast(Context context,int status,String msg){
        Intent intent=new Intent("lyy");
        intent.putExtra("status",status);
        intent.putExtra("msg",msg);
        context.sendBroadcast(intent);
    }

    private static void sendBroadcast(Context context,int status){
        Intent intent=new Intent("lyy");
        intent.putExtra("status",status);
        context.sendBroadcast(intent);
    }

    public void register(){
    }
}
