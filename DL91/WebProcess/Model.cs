using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonParsingDemo
{
    public class RootModel
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("content")]
        public ContentModel Content { get; set; }
    }

    public class ContentModel
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("per_page")]
        public int PerPage { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        [JsonPropertyName("data")]
        public List<VideoModel> Data { get; set; }
    }

    public class VideoModel
    {
        [JsonPropertyName("video_id")]
        public int VideoId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("video_viewed")]
        public int VideoViewed { get; set; }

        [JsonPropertyName("resolution_type")]
        public int ResolutionType { get; set; }

        [JsonPropertyName("is_private")]
        public int IsPrivate { get; set; }

        [JsonPropertyName("screen_main")]
        public int ScreenMain { get; set; }

        [JsonPropertyName("img_opt")]
        public int ImgOpt { get; set; }

        [JsonPropertyName("is_hd")]
        public int IsHd { get; set; }
    }

    public class helper
    {
        public static RootModel parseJson(string json)
        {
            return JsonSerializer.Deserialize<JsonParsingDemo.RootModel>(json);
        }
    }
}