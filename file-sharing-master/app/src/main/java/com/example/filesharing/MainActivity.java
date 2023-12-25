package com.example.filesharing;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.provider.Settings;
import android.text.TextUtils;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.appcompat.widget.Toolbar;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;


import com.example.filesharing.receiver.MybroadcastReceiver;
//import com.example.filesharing.service.CoreService;
import com.example.filesharing.service.CoreService;
//import com.example.filesharing.service.ServerManager;
import com.example.filesharing.utils.DialogUtils;
import com.example.filesharing.utils.FileUtils;
import com.example.filesharing.utils.LogUtils;
import com.example.filesharing.utils.NetUtils;
import com.example.filesharing.utils.ToolbarUtils;

import java.io.File;


public class MainActivity extends BaseActivity implements View.OnClickListener {

    public Button controlBtn,openBrowserBtn;
    public TextView urlTxt;
    private Toolbar toolbar;
    public boolean isRunning=false;//服务器是否处于运行状态
    MybroadcastReceiver mMybroadcastReceiver;
     private String url="";
    private static final String TAG = "MainActivity";
    //private ServerManager serverManager;

    @RequiresApi(api = Build.VERSION_CODES.JELLY_BEAN_MR1)
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        startService(new Intent(this, CoreService.class));//启动服务
        //serverManager = new ServerManager(this);
        //serverManager.startServer();
        initView();
        mMybroadcastReceiver=new MybroadcastReceiver(this);
        registerReceiver(mMybroadcastReceiver,new IntentFilter("lyy"));//注册广播
        checkStorageManagerPermission();

    }

    @Override
    protected void onPause() {
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        unregisterReceiver(mMybroadcastReceiver);//取消注册广播
        LogUtils.logD(TAG,"onDestroy: 方法执行了");
    }


    @SuppressLint("ResourceType")
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.toolbar,menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        switch (item.getItemId()){
            case R.id.help:
                startActivity(new Intent(MainActivity.this, HelpActivity.class));
                break;
            case R.id.exitNow:
                DialogUtils.exitDialog(MainActivity.this);
                break;
            default:
        }
        return true;
    }


    @RequiresApi(api = Build.VERSION_CODES.JELLY_BEAN_MR1)
    private void initView() {
        controlBtn=findViewById(R.id.controlBtn);
        controlBtn.setOnClickListener(this);
        openBrowserBtn=findViewById(R.id.openBrowserBtn);
        openBrowserBtn.setOnClickListener(this);
        toolbar=findViewById(R.id.toolbar);
        ToolbarUtils.setTile(this,toolbar,getString(R.string.app_name));
        setSupportActionBar(toolbar);

        urlTxt=findViewById(R.id.urlTxt);
        url="服务器已经启动，访问地址是：\nhttp://"+NetUtils.getLocalIPAddress()+":1080/";
        urlTxt.setText(url);

        int i = ContextCompat.checkSelfPermission(this, Manifest.permission.WRITE_EXTERNAL_STORAGE);
        if(Build.VERSION.SDK_INT>=23){
            if(i!= PackageManager.PERMISSION_GRANTED){//如果没有授权
                ActivityCompat.requestPermissions(this,new String[]{Manifest.permission.WRITE_EXTERNAL_STORAGE},1);//申请读写权限，WRITE_EXTERNAL_STORAGE既然可以写，已经包含了“读”权限
            }
        }
        //创建保存文件的目录
        File file=new File(FileUtils.fileDirectory);
        if(!file.exists()){//如果目录还未存在
            boolean mkdir = file.mkdir();
            LogUtils.logD(TAG,"目录是否创建成功："+mkdir);
        }

    }

    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if(requestCode==1){
            if(grantResults[0]!=PackageManager.PERMISSION_GRANTED){
                DialogUtils.showInfo(MainActivity.this,"你拒绝了读写文件的权限,将无法正常读写本服务器上的文件！");
            }
        }
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()){
            case R.id.controlBtn:
                LogUtils.logD(TAG,"服务是否在运行："+isRunning);
                if(isRunning){
                    LogUtils.logD(TAG,"您点击了停止服务");
                    stopService(new Intent(MainActivity.this, CoreService.class));//停止服务
                    //serverManager.stopServer();
                }else {
                    LogUtils.logD(TAG,"您点击了启动服务");
                    startService(new Intent(MainActivity.this, CoreService.class));//启动服务
                    //serverManager.startServer();
                }

                break;
            case R.id.openBrowserBtn:
                Intent intent=new Intent();
                intent.setAction("android.intent.action.VIEW");
                url="http://localhost:1080/";
                if(!TextUtils.isEmpty(url)){
                    intent.setData(Uri.parse(url));
                    startActivity(intent);
                }
                break;
            default:
        }
    }

    private void checkStorageManagerPermission(){
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.R || Environment.isExternalStorageManager()){
            Log.d(TAG, "已获得访问所有文件权限");
        }else {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.setMessage("本程序需要您同意允许访问所有文件权限");
            builder.setCancelable(false);
            builder.setPositiveButton("确定", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialog, int which) {
                    Intent intent =new Intent(Settings.ACTION_MANAGE_ALL_FILES_ACCESS_PERMISSION);
                    startActivity(intent);
                }
            });
            builder.setNegativeButton("取消", new DialogInterface.OnClickListener() {
                @Override
                public void onClick(DialogInterface dialog, int which) {
                    Toast.makeText(MainActivity.this, "取消授权将无法正常使用文件上传、下载等功能。", Toast.LENGTH_LONG).show();
                }
            });
            builder.show();
        }
    }
}