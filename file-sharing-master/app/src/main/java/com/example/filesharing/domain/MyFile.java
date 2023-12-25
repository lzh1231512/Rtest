package com.example.filesharing.domain;

/**
 * @author Yingyong Lao
 * @date 2021/3/3 20:01
 **/
public class MyFile {
    private String name;//文件的名称
    private double length;//文件的大小，单位kb
    private boolean isDir;//是否是目录

    public MyFile() {
    }

    public MyFile(String name, double length, boolean isDir) {
        this.name = name;
        this.length = length;
        this.isDir = isDir;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }

    public double getLength() {
        return length;
    }

    public void setLength(double length) {
        this.length = length;
    }

    public boolean isDir() {
        return isDir;
    }

    public void setDir(boolean dir) {
        isDir = dir;
    }
}
