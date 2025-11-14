using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91.WebProcess
{
    using JsonParsingDemo;
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class RootResponse
    {
        [JsonPropertyName("error")]
        public int Error { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("video_id")]
        public int VideoId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("duration")]
        public int Duration { get; set; }

        [JsonPropertyName("post_date")]
        public string PostDate { get; set; }

        [JsonPropertyName("resolution_type")]
        public int ResolutionType { get; set; }

        [JsonPropertyName("is_private")]
        public int IsPrivate { get; set; }

        [JsonPropertyName("file_dimensions")]
        public string FileDimensions { get; set; }

        [JsonPropertyName("screen_main")]
        public int ScreenMain { get; set; }

        [JsonPropertyName("img_opt")]
        public int ImgOpt { get; set; }

        [JsonPropertyName("is_limit_over")]
        public int IsLimitOver { get; set; }

        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }

        [JsonPropertyName("categories")]
        public List<Category> Categories { get; set; }

        [JsonPropertyName("video_viewed")]
        public int VideoViewed { get; set; }

        [JsonPropertyName("favourites_count")]
        public int FavouritesCount { get; set; }

        [JsonPropertyName("good_count")]
        public int GoodCount { get; set; }

        [JsonPropertyName("status_id")]
        public int StatusId { get; set; }

        [JsonPropertyName("vip_expire")]
        public int VipExpire { get; set; }

        [JsonPropertyName("is_hd")]
        public int IsHd { get; set; }

        [JsonPropertyName("video_url")]
        public string VideoUrl { get; set; }

        [JsonPropertyName("can_good")]
        public int CanGood { get; set; }

        [JsonPropertyName("favInfo")]
        public FavInfo FavInfo { get; set; }

        [JsonPropertyName("ads")]
        public List<object> Ads { get; set; }
    }

    public class Tag
    {
        [JsonPropertyName("tag_id")]
        public int TagId { get; set; }

        [JsonPropertyName("tag")]
        public string TagName { get; set; }
    }

    public class Category
    {
        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class FavInfo
    {
        [JsonPropertyName("add_flag")]
        public int AddFlag { get; set; }

        [JsonPropertyName("add_just")]
        public int AddJust { get; set; }

        [JsonPropertyName("playlists")]
        public List<object> Playlists { get; set; }

        [JsonPropertyName("is_good")]
        public int IsGood { get; set; }

        [JsonPropertyName("good_count")]
        public int GoodCount { get; set; }

        [JsonPropertyName("favourites_count")]
        public int FavouritesCount { get; set; }
    }

    public class DetailHelper
    {
        public static RootResponse parseJson(string json)
        {
            return JsonSerializer.Deserialize<RootResponse>(json);
        }
    }
}
