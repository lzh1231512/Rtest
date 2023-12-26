package com.example.filesharing.handler;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.os.Build;

import com.arthenica.ffmpegkit.FFmpegKit;
import com.arthenica.ffmpegkit.FFmpegSession;
import com.arthenica.ffmpegkit.ReturnCode;
import com.example.filesharing.domain.JsonResponse;
import com.example.filesharing.domain.MyFile;
import com.example.filesharing.utils.FileUtils;
import com.google.gson.Gson;
import com.yanzhenjie.andserver.annotation.Controller;
import com.yanzhenjie.andserver.annotation.CrossOrigin;
import com.yanzhenjie.andserver.annotation.GetMapping;
import com.yanzhenjie.andserver.annotation.PostMapping;
import com.yanzhenjie.andserver.annotation.RequestMethod;
import com.yanzhenjie.andserver.annotation.RequestParam;
import com.yanzhenjie.andserver.annotation.ResponseBody;
import com.yanzhenjie.andserver.http.HttpResponse;
import com.yanzhenjie.andserver.http.RequestBody;
import com.yanzhenjie.andserver.http.multipart.MultipartFile;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Comparator;
import java.util.List;

@Controller
public class CacheController {
    @GetMapping("/getMenu")
    @ResponseBody
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public String getMenu(RequestBody body, HttpResponse response) throws IOException {
        StringBuilder sb=new StringBuilder();
        sb.append("[");
        String fileDirectory = FileUtils.fileDirectory+"/menu";
        File file = new File(fileDirectory);
        if(file.exists()){
            File[] files = file.listFiles();
            if(files!=null){
                for (int i = 0; i < files.length; i++) {
                    File fi=files[i];
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                        List<String> str= Files.readAllLines(Paths.get(fi.getPath()));
                        sb.append(str.get(0));
                    }
                    if(i!=files.length-1){
                        sb.append(",");
                    }
                }
            }
        }
        sb.append("]");
        response.setHeader("Content-Type","text/json;charset=utf-8");
        return sb.toString();
    }

    @PostMapping("/deleteCache")
    @ResponseBody
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public String deleteCache(RequestBody body, HttpResponse response
            , @RequestParam("id") String id) throws IOException {
        File dir = new File(FileUtils.fileDirectory + "/" + id);
        File[] listFiles = dir.listFiles();
        for(File file : listFiles){
            System.out.println("Deleting "+file.getName());
            file.delete();
        }
        dir.delete();
        File menu = new File(FileUtils.fileDirectory + "/menu/" + id);
        menu.delete();
        response.setHeader("Content-Type","text/html;chartset=utf-8");
        return "删除文件“"+id+"”成功！";
    }

    @PostMapping("/uploadMp4")
    @ResponseBody
    @CrossOrigin(methods = {RequestMethod.POST, RequestMethod.GET})
    public String uploadMp4(RequestBody body, HttpResponse response,
                       @RequestParam("ff") MultipartFile ff,
                       @RequestParam(name = "json") String json) throws IOException {
        String result = "";
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O
            &&!ff.isEmpty()) {

            int id=-100000;
            while (Files.exists(Paths.get(FileUtils.fileDirectory + "/"+id))){
                id-=1;
            }
            Files.createDirectory(Paths.get(FileUtils.fileDirectory + "/"+id));

            String path0 = FileUtils.fileDirectory + "/"+id;
            String path1 = path0 + "/1.mp4";
            String path2 = path0 + "/1.m3u8";
            Path P_path2=Paths.get(path2);
            String path3 = path0 + "/%3d.ts";
            String path4 = path0 + "/1.jpg";

            String pathM = path0 + "/m3";
            String pathI = path0 + "/m1";
            String pathE = FileUtils.fileDirectory + "/menu/"+id;
            Path P_pathE=Paths.get(pathE);

            ff.transferTo(new File(path1));

            FFmpegSession session = FFmpegKit.execute("-i \"" + path1
                    + "\" -c copy -map 0 -f segment -segment_list \"" + path2
                    + "\" -segment_time 5 \"" + path3 + "\"");

            if (ReturnCode.isSuccess(session.getReturnCode())) {
                FFmpegKit.execute("-i \"" + path1
                        + "\" -ss 00:00:06 -vframes 1 \"" + path4 + "\"");

                List<String> m3u8Info = Files.readAllLines(P_path2);
                for (int i=0,k=0;i<m3u8Info.size();i++){
                    String line=m3u8Info.get(i);
                    if(!line.startsWith("#")&&!line.isEmpty()){
                        File oldFile = new File(path0+"/"+line);
                        File newFile = new File(path0+"/"+k);
                        oldFile.renameTo(newFile);
                        m3u8Info.set(i,"fileList?filename="+id+"/"+k+"&mime=application/video/MP2T");
                        k++;
                    }
                }
                Files.write(Paths.get(pathM), String.join("\r\n",m3u8Info).getBytes());

                zoomImg(path4,pathI);
                Files.delete(Paths.get(path4));

                Files.delete(Paths.get(path1));
                Files.delete(P_path2);
                json=json.replace("{id}",id+"");
                if(Files.exists(P_pathE)){
                    Files.delete(P_pathE);
                }
                Files.write(P_pathE, json.getBytes());

                result = "success";
            } else if (ReturnCode.isCancel(session.getReturnCode())) {
                result = "cancel";
            } else {
                result = "failed";
            }
        }
        return result;
    }

    private static void zoomImg(String srcPath,String dstPath) throws IOException{
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = false;
        Bitmap bm= BitmapFactory.decodeFile(srcPath, options);
        int srcWidth = bm.getWidth();
        int srcHeight = bm.getHeight();
        int targetWidth,  targetHeight;
        int h = (int)(320 * srcHeight / srcWidth);
        int px=0,py=0;
        if (h < 180){
            targetWidth=320;
            targetHeight=h;
            py=(180-h)/2;
        }
        else{
            targetWidth=(int)(180 * srcWidth / srcHeight);
            targetHeight=180;
            px=(320-targetWidth)/2;
        }
        Bitmap bmpRet = Bitmap.createBitmap(320, 180, Bitmap.Config.RGB_565);
        Canvas canvas = new Canvas(bmpRet);
        Paint paint = new Paint();
        Rect dstRect = new Rect(px,py,px+targetWidth,py+targetHeight);
        canvas.drawBitmap(bm, null,dstRect, paint);

        FileOutputStream fos=new FileOutputStream(dstPath);
        bmpRet.compress(Bitmap.CompressFormat.JPEG,100,fos);
        fos.flush();
        fos.close();
    }
}
