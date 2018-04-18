package com.heartbrers.air;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.support.v7.widget.CardView;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by nrgz on 11.04.2018.
 */

public class RobotAdapter extends RecyclerView.Adapter<RobotAdapter.RoboViewHolder> {
    private List<RoboInfo> arrayList;
    private Context ctx;

    public RobotAdapter(Context ctx, List<RoboInfo> arrayList){
        this.ctx = ctx; this.arrayList=arrayList;
    }


    @Override
    public RoboViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        View view= LayoutInflater.from(parent.getContext()).inflate(R.layout.listitem_robots, parent,false);
        RoboViewHolder roboViewHolder=new RoboViewHolder(view);
        return roboViewHolder;
    }

    @Override
    public void onBindViewHolder(RoboViewHolder holder, int position) {
        final RoboInfo roboInfo = arrayList.get(position);
        holder.robot_icon.setBackgroundResource(roboInfo.getIconId());
        holder.r_name.setText(roboInfo.getTitle());

        holder.card.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(ctx, UnityPlayerActivity.class);
                ctx.startActivity(intent);
            }
        });
    }

    @Override
    public int getItemCount() {
        return arrayList.size();
    }



    public class RoboViewHolder extends RecyclerView.ViewHolder {

        CardView card;
        View robot_icon, delete_icon, copy_icon, wrench_icon;
        TextView r_name;
        int position;
        RoboInfo currentObject;
        public RoboViewHolder (View view){
            super(view);
            robot_icon= (View)view.findViewById(R.id.robot_icon);
            r_name= (TextView)view.findViewById(R.id.robot_name);

            card = (CardView)view.findViewById(R.id.layout_robot_card);
        }

        /*public void onClick(View view){
            switch (view.getId()){
                case R.id.delete_icon:
                    removeItem(position);
                    break;
                case R.id.copy_icon:
                    addItem(position, currentObject);
                    break;
            }

        }*/

    }

    public void removeItem(int position) {
        arrayList.remove(position);
        notifyItemInserted(position);
        notifyItemChanged(position, arrayList.size());
    }

    public void addItem(RoboInfo currentObject) {
        arrayList.add(currentObject);
        notifyDataSetChanged();
    }


}
