package com.example.filesharing.domain;

import java.util.List;

public class JsonResponse<T> {
    private boolean status;
    private String errMsg;
    private String note;
    private List<T> objList;

    public JsonResponse() {
    }

    public JsonResponse(boolean status, String errMsg, String note, List<T> objList) {
        this.status = status;
        this.errMsg = errMsg;
        this.note = note;
        this.objList = objList;
    }

    public boolean isStatus() {
        return status;
    }

    public void setStatus(boolean status) {
        this.status = status;
    }

    public String getErrMsg() {
        return errMsg;
    }

    public void setErrMsg(String errMsg) {
        this.errMsg = errMsg;
    }

    public String getNote() {
        return note;
    }

    public void setNote(String note) {
        this.note = note;
    }

    public List<T> getObjList() {
        return objList;
    }

    public void setObjList(List<T> objList) {
        this.objList = objList;
    }
}
