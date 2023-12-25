package com.example.filesharing.service;

import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;

import androidx.core.app.NotificationCompat;

import com.example.filesharing.MainActivity;
import com.example.filesharing.receiver.MybroadcastReceiver;
import com.yanzhenjie.andserver.AndServer;
import com.yanzhenjie.andserver.Server;

import java.util.concurrent.TimeUnit;

public class ServerManager {

    private Server mServer;
    public static String hostAddress="";
    public static int port=1080;
    /**
     * Create server.
     */
    public ServerManager(Context context) {
        mServer = AndServer.webServer(context)
                .port(port)
                .timeout(10, TimeUnit.SECONDS)
                .listener(new Server.ServerListener() {
                    @Override
                    public void onStarted() {
                        // TODO The server started successfully.
                        hostAddress = mServer.getInetAddress().getHostAddress();
                        System.out.println("主机地址是："+hostAddress);
                        MybroadcastReceiver.onServerStart(context,hostAddress);
                    }

                    @Override
                    public void onStopped() {
                        // TODO The server has stopped.
                        MybroadcastReceiver.onServerStop(context);
                    }

                    @Override
                    public void onException(Exception e) {
                        // TODO An exception occurred while the server was starting.
                        MybroadcastReceiver.onServerError(context,e.getMessage());
                    }
                })
                .build();
    }

    /**
     * Start server.
     */
    public void startServer() {
        if (mServer.isRunning()) {
            // TODO The server is already up.
        } else {
            mServer.startup();
        }
    }

    /**
     * Stop server.
     */
    public void stopServer() {
        if (mServer.isRunning()) {
            mServer.shutdown();
        } else {
            //Log.w("AndServer", "The server has not started yet.");
        }
    }
}