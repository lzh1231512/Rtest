package com.example.filesharing;

import android.graphics.Color;
import android.graphics.drawable.GradientDrawable;
import android.os.Bundle;
import android.view.View;
import android.widget.ExpandableListView;
import android.widget.LinearLayout;

import androidx.appcompat.widget.Toolbar;

import com.example.filesharing.adapter.ExpandableListviewAdapter;
import com.example.filesharing.utils.ToolbarUtils;


public class HelpActivity extends BaseActivity {

    private Toolbar toobar;
    private ExpandableListView mExpandableListView;
    private LinearLayout container;
    private String[] groups = {"服务器无法在后台长时间运行？","如何上传文件？", "如何下载文件？","本软件是否收费？","为什么每次启动服务器，端口号都不一样？","是否支持IPV6？"};
    private String[][] childs = {{"一般是手机的电池优化所致，请在手机的设置里面忽略本应用的电池优化。"},
            {"客户端与此手机（服务器）连接同一个网络，启动服务器之后，客户端在浏览器输入页面显示的访问地址进入网站首页，然后点击“我要上传文件至服务端”即可上传文件。上传速度视网速而定，完成后页面会有成功上传的提示，请您耐心等待。"},
            {"客户端与此手机（服务器）连接同一个网络，启动服务器之后，客户端在浏览器输入页面显示的访问地址进入网站首页，然后点击“浏览服务端文件列表”进入文件浏览界面，再点击对应文件右方的“下载”按钮，即可从服务器上下载文件到本地。"},
            {"“无界共享”采用AndServer框架开发，是一款开源、免费的软件，我们不收取您的任何费用。"},
            {"因为服务器在关闭后，可能会由于端口没有被及时释放掉导致服务器无法及时重启，故采用随机端口号，此法也可以防止他人随意读取服务器的文件，确保服务器的安全性。"},{"暂不支持，敬请期待！"}};

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_help);
        initView();
    }


    private void initView() {

        toobar=findViewById(R.id.toolbar);
        toobar.setNavigationIcon(R.drawable.ic_arrow_back_24dp);
        ToolbarUtils.setTile(this,toobar,getString(R.string.common_problem));
        toobar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });

        container=findViewById(R.id.container);
        mExpandableListView=findViewById(R.id.elv);

        GradientDrawable gradientDrawable=new GradientDrawable();
        gradientDrawable.setShape(GradientDrawable.RECTANGLE);
        gradientDrawable.setCornerRadius(30);
        gradientDrawable.setColor(Color.parseColor("#1781b5"));
        mExpandableListView.setBackground(gradientDrawable);

        GradientDrawable gradientDrawable2=new GradientDrawable();
        gradientDrawable2.setShape(GradientDrawable.RECTANGLE);
        gradientDrawable2.setCornerRadius(30);
        gradientDrawable2.setColor(Color.parseColor("#5e616d"));
        container.setBackground(gradientDrawable2);

        mExpandableListView.setGroupIndicator(null);
        ExpandableListviewAdapter expandableListviewAdapter=new ExpandableListviewAdapter(groups,childs,this);
        mExpandableListView.setAdapter(expandableListviewAdapter);
        mExpandableListView.setOnChildClickListener(new ExpandableListView.OnChildClickListener() {
            @Override
            public boolean onChildClick(ExpandableListView parent, View v, int groupPosition, int childPosition, long id) {
                return false;
            }
        });

    }
}