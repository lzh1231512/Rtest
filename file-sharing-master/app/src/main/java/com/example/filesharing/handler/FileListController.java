package com.example.filesharing.handler;

import com.example.filesharing.domain.JsonResponse;
import com.example.filesharing.domain.MyFile;
import com.example.filesharing.utils.FileUtils;
import com.google.gson.Gson;
import com.yanzhenjie.andserver.annotation.Controller;
import com.yanzhenjie.andserver.annotation.CrossOrigin;
import com.yanzhenjie.andserver.annotation.FormPart;
import com.yanzhenjie.andserver.annotation.GetMapping;
import com.yanzhenjie.andserver.annotation.PathVariable;
import com.yanzhenjie.andserver.annotation.PostMapping;
import com.yanzhenjie.andserver.annotation.QueryParam;
import com.yanzhenjie.andserver.annotation.RequestMethod;
import com.yanzhenjie.andserver.annotation.RequestParam;
import com.yanzhenjie.andserver.annotation.ResponseBody;
import com.yanzhenjie.andserver.annotation.RestController;
import com.yanzhenjie.andserver.framework.body.FileBody;
import com.yanzhenjie.andserver.http.HttpRequest;
import com.yanzhenjie.andserver.http.HttpResponse;
import com.yanzhenjie.andserver.http.RequestBody;
import com.yanzhenjie.andserver.http.StandardRequest;
import com.yanzhenjie.andserver.http.multipart.MultipartFile;

import java.io.File;
import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.net.URLDecoder;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;
@RestController
public class FileListController {

    @GetMapping("/fileList")
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public FileBody download(HttpRequest request, HttpResponse response
            , @RequestParam("filename") String filename
            , @RequestParam(name = "mime",  required = false) String mime)
            throws UnsupportedEncodingException {
        //response.setHeader("Content-Disposition", "attachment;filename="+decode);
        // 不设置这个就没有下载的效果，举例：如果是txt文件，浏览器就会直接展示文字，而不是下载
        if(mime!=null&&!mime.isEmpty()){
            response.setHeader("Content-Type",mime);
        }
        return new FileBody(new File(FileUtils.fileDirectory+"/"+filename));
    }

    @PostMapping("/fileDel/{filename}")
    @ResponseBody
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public String delete(HttpRequest request, HttpResponse response
            , @PathVariable("filename") String filename){
        File file = new File(FileUtils.fileDirectory + "/" + filename);
        boolean delete = file.delete();
        String s="";
        if(delete){
            s="删除文件“"+filename+"”成功！";
        }else{
            s="删除文件“"+filename+"”失败！";
        }
        return s;
    }

    @PostMapping("/fileUpload")
    @ResponseBody
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public String upload(HttpRequest request, HttpResponse response
            , @RequestParam("ff") MultipartFile ff
            , @RequestParam(name = "folder",  required = false) String folder
            , @RequestParam(name = "filename",  required = false) String fileName) throws IOException {

        String path=FileUtils.fileDirectory;
        if(folder!=null&&!folder.isEmpty()){
            path+="/"+folder;
        }
        File file=new File(path);//临时文件的存放目录
        if(!file.exists()){//如果目录还未存在
            boolean mkdir = file.mkdir();
        }
        if(!ff.isEmpty()){
            if(fileName==null||fileName.isEmpty()){
                fileName=ff.getFilename();
            }
            ff.transferTo(new File(file,fileName));
            return "文件上传完成！";
        }
        return "文件上传失败,服务器未接收到任何文件！";
    }
}
