package com.example.filesharing.adapter;

import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.GradientDrawable;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.core.content.ContextCompat;

import com.example.filesharing.R;




public class ExpandableListviewAdapter extends BaseExpandableListAdapter {

    private String[] groups;
    private String[][] childs;
    private Context context;

    public ExpandableListviewAdapter(String[] groups, String[][] childs, Context context) {
        this.groups = groups;
        this.childs = childs;
        this.context = context;
    }

    @Override
    public int getGroupCount() {
        return groups.length;
    }

    @Override
    public int getChildrenCount(int groupPosition) {
        return childs[groupPosition].length;
    }

    @Override
    public Object getGroup(int groupPosition) {
        return groups[groupPosition];
    }

    @Override
    public Object getChild(int groupPosition, int childPosition) {
        return childs[groupPosition][childPosition];
    }

    @Override
    public long getGroupId(int groupPosition) {
        return groupPosition;
    }

    @Override
    public long getChildId(int groupPosition, int childPosition) {
        return childPosition;
    }

    @Override
    public boolean hasStableIds() {
        return true;
    }

    @Override
    public View getGroupView(int groupPosition, boolean isExpanded, View convertView, ViewGroup parent) {
        GroupViewHolder groupViewHolder;
        if (convertView == null){
            convertView = LayoutInflater.from(parent.getContext()).inflate(R.layout.ques_normal,parent,false);
            groupViewHolder = new GroupViewHolder();
            groupViewHolder.parent_textview_id = convertView.findViewById(R.id.question);
            groupViewHolder.parent_image = convertView.findViewById(R.id.iv);
            groupViewHolder.line=convertView.findViewById(R.id.line);
            if(groupPosition==0){
                groupViewHolder.line.setVisibility(View.GONE);
            }
            convertView.setTag(groupViewHolder);
        }else {
            groupViewHolder = (GroupViewHolder)convertView.getTag();
        }
        groupViewHolder.parent_textview_id.setText(groups[groupPosition]);
        //如果是展开状态，
        if (isExpanded){
            groupViewHolder.parent_image.setImageDrawable(ContextCompat.getDrawable(context,R.drawable.ic_expand_more_black_24dp));
        }else{
            groupViewHolder.parent_image.setImageDrawable(ContextCompat.getDrawable(context,R.drawable.ic_chevron_right_black_24dp));
        }
        return convertView;
    }

    @Override
    public View getChildView(int groupPosition, int childPosition, boolean isLastChild, View convertView, ViewGroup parent) {
        ChildViewHolder childViewHolder;
        convertView = LayoutInflater.from(parent.getContext()).inflate(R.layout.ques_chil,parent,false);
        if(groupPosition!=groups.length-1){
            GradientDrawable gradientDrawable=new GradientDrawable();
            gradientDrawable.setShape(GradientDrawable.RECTANGLE);
            gradientDrawable.setColor(Color.parseColor("#ffffff"));
            convertView.setBackground(gradientDrawable);
        }else {
            GradientDrawable gradientDrawable=new GradientDrawable();
            gradientDrawable.setShape(GradientDrawable.RECTANGLE);
            gradientDrawable.setCornerRadii(new float[]{0,0,0,0,30,30,30,30});
            gradientDrawable.setColor(Color.parseColor("#ffffff"));
            convertView.setBackground(gradientDrawable);
        }

        childViewHolder = new ChildViewHolder();
        childViewHolder.chidren_item = (TextView)convertView.findViewById(R.id.question);
        convertView.setTag(childViewHolder);

        //childViewHolder = (ChildViewHolder) convertView.getTag();

        childViewHolder.chidren_item.setText(childs[groupPosition][childPosition]);
        return convertView;
    }

    @Override
    public boolean isChildSelectable(int groupPosition, int childPosition) {
        return false;
    }

    static class GroupViewHolder {
        TextView parent_textview_id;
        ImageView parent_image;
        View line;
    }

    static class ChildViewHolder {
        TextView chidren_item;

    }

}
