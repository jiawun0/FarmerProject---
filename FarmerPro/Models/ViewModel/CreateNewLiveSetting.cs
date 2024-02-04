using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace FarmerPro.Models.ViewModel
{
    public class CreateNewLiveSetting
    {

        [Required]
        [MaxLength(100)]
        [Display(Name = "直播名稱")]
        public string liveName { get; set; }

        [Display(Name = "直播日期")]
        public DateTime liveDate { get; set; }

        [Display(Name = "開始時間")]
        public TimeSpan startTime { get; set; }

        [Display(Name = "結束時間")]
        public TimeSpan endTime { get; set; }

        [Display(Name = "直播圖片")]
        public string livePic { get; set; }   //Figma好像缺乏這個欄位

        [Required]
        [Display(Name = "YT直播網址")]   //可以加入網址的正規表達式
        public string yturl { get; set; }

        [Display(Name = "直播產品")]//其他表-外鍵
        public List<CreateNewLiveProudct> liveproudct { get; set; }
    }

    public class CreateNewLiveProudct
    {
        [Display(Name = "產品Id")]//其他表-外鍵
        public int prodcutId { get; set; }

        [Display(Name = "產品尺寸")]//其他表-外鍵
        public int productSize { get; set; }

        [Display(Name = "直播價格")]//其他表-外鍵
        public int liveprice { get; set; }

    }
}