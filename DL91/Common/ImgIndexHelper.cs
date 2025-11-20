using Microsoft.Playwright;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using DL91;
using System.Collections.Generic;

public class ImgIndexHelper
{
    public static string ProcessAsync(int index)
    {
        var targetFolder = Path.Combine(AppContext.BaseDirectory, "imgIndex");
        int batchSize = 1000;
        int zeroBased = index;
        int fileIndex = zeroBased / batchSize;
        int innerIndex = zeroBased % batchSize;
        var filePath = Path.Combine(targetFolder, $"output_{fileIndex}.bin");
        if (!File.Exists(filePath)) return null;
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var br = new BinaryReader(fs))
        {
            for (int i = 0; i <= innerIndex; i++)
            {
                var len = br.ReadUInt16();
                var bytes = br.ReadBytes(len);
                if (i == innerIndex)
                    return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
        return null;
    }
}