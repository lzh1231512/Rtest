package com.example.filesharing.utils;

import android.app.Dialog;
import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.TextView;

import com.example.filesharing.R;



/**
 * @author Yingyong Lao
 * @date 2021/3/5 19:50
 **/
public class DialogUtils{
    private static Dialog dialog;

    /**
     * 退出程序的对话框
     * @param context
     */
    public static void exitDialog(Context context){
        dialog=new Dialog(context, R.style.mydialog);
        View dialogView = LayoutInflater.from(context).inflate(R.layout.exit_dialog, null, false);
        dialog.setContentView(dialogView);
        dialog.setCancelable(false);
        dialog.show();
        dialogView.findViewById(R.id.cancelBtn).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                dialog.dismiss();
            }
        });
        dialogView.findViewById(R.id.confirmBtn).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                ActivityCollector.exit();
            }
        });
    }

    public static void showInfo(Context context,String info){
        dialog=new Dialog(context,R.style.mydialog);
        View dialogView = LayoutInflater.from(context).inflate(R.layout.info_dialog, null, false);
        dialog.setContentView(dialogView);
        dialog.setCancelable(false);
        TextView textView=dialogView.findViewById(R.id.infoText);
        textView.setText(info);
        dialog.show();
        dialogView.findViewById(R.id.confirmBtn).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                dialog.dismiss();
            }
        });
    }
}
