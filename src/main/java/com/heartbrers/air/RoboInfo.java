package com.heartbrers.air;

/**
 * Created by nrgz on 10.04.2018.
 */

public class RoboInfo {
    private int iconId, delete, copy;
    private String r_name;
    private String r_identifier;

    public RoboInfo(int iconId, String r_name, String r_identifier) {
        this.iconId = iconId;
        this.r_name= r_name;
        this.r_identifier = r_identifier;
    }


    public int getIconId() {
        return iconId;
    }

    public void setIconId(int iconId) {
        this.iconId = iconId;
    }

    public String getTitle() {
        return r_name;
    }

    public void setR_name(String title) {
        this.r_name = title;
    }

    public int getDelete() {
        return delete;
    }

    public void setDelete(int delete) {
        this.delete = delete;
    }

    public int getCopy() {
        return copy;
    }

    public void setCopy(int copy) {
        this.copy = copy;
    }
}

