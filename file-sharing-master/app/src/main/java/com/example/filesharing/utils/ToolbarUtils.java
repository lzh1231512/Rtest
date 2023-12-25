package com.example.filesharing.utils;

import android.view.Gravity;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import androidx.core.content.ContextCompat;

import com.example.filesharing.R;

public class ToolbarUtils {
    public static void setTile(AppCompatActivity activity, Toolbar toolbar, String title){
        toolbar.setTitle("");
        TextView titleTv = new TextView(activity);
        titleTv.setTextColor(ContextCompat.getColor(activity, R.color.white));
        titleTv.setText(title);
        titleTv.setTextSize(18);
        titleTv.setGravity(Gravity.CENTER);
        titleTv.getPaint().setFakeBoldText(true);
        Toolbar.LayoutParams layoutParams = new Toolbar.LayoutParams(Toolbar.LayoutParams.WRAP_CONTENT,
                Toolbar.LayoutParams.WRAP_CONTENT);
        layoutParams.gravity = Gravity.CENTER;
        titleTv.setLayoutParams(layoutParams);
        toolbar.addView(titleTv);
        activity.setSupportActionBar(toolbar);

    }
}
