package com.example.filesharing.service;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.IBinder;

import androidx.annotation.Nullable;
import androidx.core.app.NotificationCompat;

import com.example.filesharing.MainActivity;
import com.example.filesharing.R;
import com.example.filesharing.receiver.MybroadcastReceiver;
import com.example.filesharing.utils.LogUtils;
import com.example.filesharing.utils.NetUtils;
import com.yanzhenjie.andserver.AndServer;
import com.yanzhenjie.andserver.Server;

import java.net.InetAddress;
import java.net.UnknownHostException;
import java.util.Random;
import java.util.concurrent.TimeUnit;

public class CoreService extends Service {
    private Server mServer ;
    public static String hostAddress="";
    public static int port=1080;
    private static final String TAG = "CoreService";


    @Override
    public void onCreate() {
        super.onCreate();

        Random random=new Random();
        //port=random.nextInt(9000)+1000;//生成[1000,9999]范围内的随机数作为端口号，防止端口占用的问题

        LogUtils.logD(TAG,"service已经onCreate");

        String ID = "com.example.filesharing";	//这里的id里面输入自己的项目的包的路径
        String NAME = "Channel One";
        Intent intent = new Intent(this, MainActivity.class);
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, intent, 0);
        NotificationCompat.Builder notiBuilder; //创建服务对象
        NotificationManager manager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(ID, NAME, manager.IMPORTANCE_HIGH);
            channel.enableLights(true);
            channel.setShowBadge(true);
            channel.setLockscreenVisibility(Notification.VISIBILITY_PUBLIC);
            manager.createNotificationChannel(channel);
            notiBuilder = new NotificationCompat.Builder(this).setChannelId(ID);
        } else {
            notiBuilder = new NotificationCompat.Builder(this);
        }
        notiBuilder.setContentTitle("无界共享")
                .setContentText("服务已经启动")
                .setWhen(System.currentTimeMillis())
                .setSmallIcon(R.mipmap.ic_launcher)
                .setLargeIcon(BitmapFactory.decodeResource(getResources(),R.mipmap.ic_launcher))
                .setContentIntent(pendingIntent)
                .build();
        Notification notification = notiBuilder.build();
        startForeground(1,notification);//作为前台服务，防止服务在后台运行时被系统回收


        mServer = AndServer.webServer(this)
                .port(port)
                .timeout(10, TimeUnit.SECONDS)
                .listener(new Server.ServerListener() {
                    @Override
                    public void onStarted() {
                        // TODO The server started successfully.
                        hostAddress = NetUtils.getLocalIPAddress().toString();
                        System.out.println("主机地址是："+hostAddress);
                        MybroadcastReceiver.onServerStart(CoreService.this,hostAddress);
                    }

                    @Override
                    public void onStopped() {
                        // TODO The server has stopped.
                        MybroadcastReceiver.onServerStop(CoreService.this);
                    }

                    @Override
                    public void onException(Exception e) {
                        // TODO An exception occurred while the server was starting.
                        MybroadcastReceiver.onServerError(CoreService.this,e.getMessage());
                    }
                })
                .build();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (!mServer.isRunning()) {
            mServer.startup();
        }

        LogUtils.logD(TAG,"service已经onStartCommand");
        return START_NOT_STICKY;
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        LogUtils.logD(TAG,"service已经destroy了");
        if (mServer.isRunning()) {
            mServer.shutdown();
        }
        stopForeground(true);// 停止前台服务--参数：表示是否移除之前的通知

    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
