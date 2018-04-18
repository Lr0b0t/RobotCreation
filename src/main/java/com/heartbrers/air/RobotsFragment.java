package com.heartbrers.air;

import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.preference.PreferenceManager;
import android.support.v4.app.Fragment;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.support.v7.app.AlertDialog;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by nrgz on 06.04.2018.
 */
public class RobotsFragment extends Fragment {

    RecyclerView recyclerView;
    RobotAdapter adapter;
    RecyclerView.LayoutManager layoutManager;

    @Nullable
    @Override
    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_robots, container, false);
        recyclerView= (RecyclerView)view.findViewById(R.id.recycler_view_robots);

        List<RoboInfo> info = new ArrayList<>();
        SharedPreferences prefs = PreferenceManager.getDefaultSharedPreferences(getContext());

        for(int i = 0; i < prefs.getInt("robotCount", 0); i++) {
            info.add(new RoboInfo(R.drawable.industrial_robot, prefs.getString("robotName_" + i, "null"), prefs.getString("robotID_" + i, "null")));
        }
        adapter = new RobotAdapter(getContext(), info);


        recyclerView.setHasFixedSize(false);
        layoutManager=new LinearLayoutManager(getContext());
        recyclerView.setLayoutManager(layoutManager);
        recyclerView.setAdapter(adapter);

        Button btn = view.findViewById(R.id.button_add);
        btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                final View dialogView = getActivity().getLayoutInflater().inflate(R.layout.dialog_new_robot, null);
                Log.i("Created", "Created dialogview");
                AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());

                builder.setView(dialogView)
                        .setTitle("Add new robot")
                        .setPositiveButton("Add", new DialogInterface.OnClickListener() {
                            @Override
                            public void onClick(DialogInterface dialogInterface, int i) {
                                EditText nameEdit = dialogView.findViewById(R.id.robot_name_edittext);
                                EditText idEdit = dialogView.findViewById(R.id.robot_id_edittext);

                                if(nameEdit.getText().length() != 0 && idEdit.getText().length() != 0) {
                                    SharedPreferences sprefs = PreferenceManager.getDefaultSharedPreferences(getContext());
                                    sprefs.edit().putString("robotName_" + adapter.getItemCount(), nameEdit.getText().toString())
                                    .putString("robotID_" + adapter.getItemCount(), idEdit.getText().toString()).apply();
                                    adapter.addItem(new RoboInfo(R.drawable.industrial_robot, nameEdit.getText().toString(), idEdit.getText().toString()));
                                    sprefs.edit().putInt("robotCount", adapter.getItemCount()).apply();
                                }
                            }
                        })
                .setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialogInterface, int i) {
                        dialogInterface.dismiss();
                    }
                });

                builder.create().show();
            }
        });
        return view;


    }



}
