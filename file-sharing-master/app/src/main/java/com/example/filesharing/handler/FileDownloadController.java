package com.example.filesharing.handler;

import com.example.filesharing.domain.JsonResponse;
import com.example.filesharing.domain.MyFile;
import com.example.filesharing.utils.FileUtils;
import com.google.gson.Gson;
import com.yanzhenjie.andserver.annotation.Controller;
import com.yanzhenjie.andserver.annotation.GetMapping;
import com.yanzhenjie.andserver.annotation.PostMapping;
import com.yanzhenjie.andserver.annotation.ResponseBody;
import com.yanzhenjie.andserver.framework.body.StringBody;
import com.yanzhenjie.andserver.http.HttpResponse;
import com.yanzhenjie.andserver.http.RequestBody;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;

@Controller
public class FileDownloadController {
    @PostMapping("/fileDownload")
    @ResponseBody
    public String body(RequestBody body, HttpResponse response) {
        boolean status=true;
        String errMsg="";
        String note="";
        List<MyFile> myFileList=new ArrayList<>();

        String fileDirectory = FileUtils.fileDirectory;
        File file = new File(fileDirectory);
        File[] files = file.listFiles();
        if(files==null){
            status=false;
            errMsg="无法获取服务器中的文件，请确认服务器已经授予读写权限！";
        }
        else if(files.length!=0){
            Arrays.sort(files, new Comparator<File>() {//按文件的修改日期递减排序
                public int compare(File f1, File f2) {
                    long diff = f1.lastModified() - f2.lastModified();
                    if (diff > 0)
                        return -1;
                    else if (diff == 0)
                        return 0;
                    else
                        return 1;
                }

                public boolean equals(Object obj) {
                    return true;
                }
            });


            for (int i = 0; i < files.length; i++) {
                File fi=files[i];
                String name = fi.getName();
                long length =fi.length();
                double len=(double)length/1024;//kb
                boolean directory = fi.isDirectory();
                MyFile myFile=new MyFile(name,len,directory);
                myFileList.add(myFile);
            }
        }else {
            note="服务器“"+FileUtils.fileDirectory+"”目录下没有任何文件或目录。";
        }

        Gson gson=new Gson();
        JsonResponse<MyFile> myFileJsonResponse=new JsonResponse<>(status,errMsg,note,myFileList);
        response.setHeader("Content-Type","application/json;chartset=utf-8");
        response.setHeader("Access-Control-Allow-Origin","*");
        return gson.toJson(myFileJsonResponse);
        //response.setBody(new StringBody(s));
        //response.setEntity(new StringEntity(s,"utf-8"));
        //response.setHeader("Content-Type","application/json;chartset=utf-8");
    }
}
