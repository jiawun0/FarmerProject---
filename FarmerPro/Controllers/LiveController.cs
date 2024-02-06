using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;

namespace FarmerPro.Controllers
{
    public class LiveController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region FCL-1 取得目前直播內容(包含取得近期直播內容)
        [HttpGet]
        [Route("api/live/")]
        public IHttpActionResult RenderLiveSession()
        {
            try
            {
                var CurrentLiveEvent = db.LiveSettings.AsEnumerable().
                    Where(x => x.LiveDate.Date == DateTime.Now.Date && (x.StartTime.Hours <= DateTime.Now.Hour && x.EndTime.Hours > DateTime.Now.Hour && x.EndTime.Minutes <= DateTime.Now.Minute)).
                    FirstOrDefault();

                var UpcomingLiveEvent = db.LiveSettings.AsEnumerable().
                    Where(x => (x.LiveDate.Date == DateTime.Now.Date && x.StartTime.Hours > DateTime.Now.Hour) || x.LiveDate.Date > DateTime.Now.Date).
                    OrderBy(x => x.LiveDate).
                    Take(6);

                if (CurrentLiveEvent == null)  // 沒有現正直播
                {
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "沒有直播",
                        data = new
                        {
                            upcomingLive = UpcomingLiveEvent?.Select(eventItem => new
                            {
                                liveId = eventItem.Id,
                                liveProductId = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Id,
                                liveProductName = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.ProductTitle,
                                livePrice = eventItem.LiveProduct.FirstOrDefault()?.Spec.LivePrice,
                                liveFarmer = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Users.NickName,
                                liveFarmerPic = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Users.Photo,
                                livePic = eventItem.LivePic?.FirstOrDefault(),
                                liveTime = eventItem.LiveDate.Date.ToString("yyyy.MM.dd") + " " + SwitchDayofWeek(eventItem.LiveDate.DayOfWeek) + " " + eventItem.StartTime.ToString().Substring(0, 5),
                            }).ToList()
                        }

                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else                                        // 有現正直播
                {
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "有直播資料",
                        data = new
                        {
                            liveId = CurrentLiveEvent.Id,
                            liveProductId = CurrentLiveEvent.LiveProduct.FirstOrDefault()?.Spec.Product.Id,
                            liveProductName = CurrentLiveEvent.LiveProduct.FirstOrDefault()?.Spec.Product.ProductTitle,
                            livePrice = CurrentLiveEvent.LiveProduct.FirstOrDefault()?.Spec.LivePrice,
                            description = CurrentLiveEvent.LiveProduct.FirstOrDefault()?.Spec.Product.Description,
                            upcomingLive = UpcomingLiveEvent?.Select(eventItem => new
                            {
                                liveId = eventItem.Id,
                                liveProductId = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Id,
                                liveProductName = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.ProductTitle,
                                livePrice = eventItem.LiveProduct.FirstOrDefault()?.Spec.LivePrice,
                                liveFarmer = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Users.NickName,
                                liveFarmerPic = eventItem.LiveProduct.FirstOrDefault()?.Spec.Product.Users.Photo,
                                livePic = eventItem.LivePic?.FirstOrDefault(),
                                liveTime = eventItem.LiveDate.Date.ToString("yyyy.MM.dd") + " " + SwitchDayofWeek(eventItem.LiveDate.DayOfWeek) + " " + eventItem.StartTime.ToString().Substring(0, 5),
                            }).ToList()
                        }
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch
            {
                var result = new
                {
                    statusCode = 500,
                    status = "error",
                    message = "其他錯誤",
                };
                return Content(HttpStatusCode.OK, result);
            }
        }
        #endregion


        public string SwitchDayofWeek(DayOfWeek input)
        {
            string chineseDayOfWeek = "";
            switch (input)
            {
                case DayOfWeek.Sunday:
                    chineseDayOfWeek = "(日)";
                    break;
                case DayOfWeek.Monday:
                    chineseDayOfWeek = "(一)";
                    break;
                case DayOfWeek.Tuesday:
                    chineseDayOfWeek = "(二)";
                    break;
                case DayOfWeek.Wednesday:
                    chineseDayOfWeek = "(三)";
                    break;
                case DayOfWeek.Thursday:
                    chineseDayOfWeek = "(四)";
                    break;
                case DayOfWeek.Friday:
                    chineseDayOfWeek = "(五)";
                    break;
                case DayOfWeek.Saturday:
                    chineseDayOfWeek = "(六)";
                    break;
            }
            return chineseDayOfWeek.ToString();
        }//星期的中文轉換方法



    }
}
