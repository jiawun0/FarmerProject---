using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using FarmerPro.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Web.Http;
using System.Web.Http.Results;

namespace FarmerPro.Controllers
{
    public class LiveSettingController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region BFL-1 新增後台直播資訊(新增產品直播價)
        [HttpPost]
        [Route("api/livesetting")]
        [JwtAuthFilter]
        public IHttpActionResult CreateNewLiveSetting([FromBody] CreateNewLiveSetting input)
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            try
            {
                if (!ModelState.IsValid)
                {
                    var result = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "欄位輸入格式不正確，請重新輸入",
                    };
                    return Content(HttpStatusCode.OK, result);
                }

                var NewLiveSetting = new LiveSetting
                {
                    LiveName = input.liveName,
                    LiveDate = input.liveDate.Date,
                    StartTime = input.startTime,
                    EndTime = input.endTime,
                    YTURL = input.yturl,
                    ShareURL = input.yturl.Substring(input.yturl.LastIndexOf('.')), //這邊可能要改
                    UserId = FarmerId,
                };

                db.LiveSettings.Add(NewLiveSetting);
                db.SaveChanges();
                int LiveSettingId = NewLiveSetting.Id;

                var LiveProduct = input.liveproudct.Select(LP => new LiveProduct
                {
                    IsTop = false,
                    LiveSettingId = LiveSettingId,
                    SpecId = db.Products.Where(x => x.Id == LP.prodcutId).FirstOrDefault().Spec.Where(x => x.Size == Convert.ToBoolean(LP.productSize)).FirstOrDefault().Id,
                }).ToList();

                db.LiveProducts.AddRange(LiveProduct);
                db.SaveChanges();

                foreach (var LP in input.liveproudct)
                {
                    var specitem = db.Products.Where(x => x.Id == LP.prodcutId).FirstOrDefault().Spec.Where(x => x.Size == Convert.ToBoolean(LP.productSize)).FirstOrDefault();
                    if (specitem != null)
                    {
                        specitem.LivePrice = LP.liveprice;

                    }
                }
                db.SaveChanges();

                var searchLiveSetting = db.LiveSettings.Where(x => x.Id == LiveSettingId).FirstOrDefault();

                var resultReturn = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "新增成功",
                    data = new
                    {
                        userId = FarmerId,
                        liveId= searchLiveSetting.Id,
                        liveName = searchLiveSetting.LiveName,
                        liveDate = searchLiveSetting.LiveDate.Date,
                        startTime = searchLiveSetting.StartTime,
                        endTime = searchLiveSetting.EndTime,
                        //要補上livePic
                        yturl = searchLiveSetting.LivePic,
                        liveproudct = searchLiveSetting.LiveProduct.Select(x => new
                        {
                            productId = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().ProductId,
                            productName = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().Product.ProductTitle,
                            productSize = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().Size,
                            liveprice = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().LivePrice,
                            liveproductId= x.Id,
                        }).ToList()
                    }
                };
                return Content(HttpStatusCode.OK, resultReturn);
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



        #region BFL-3 取得後台直播資訊(包含產品直播價)
        [HttpGet]
        [Route("api/livesetting")]
        [JwtAuthFilter]
        public IHttpActionResult RenderLiveSettingInfor()
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            try
            {
                var getUserLiveSetting = db.LiveSettings.Where(x => x.UserId == FarmerId).FirstOrDefault();

                var resultReturn = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "取得成功",
                    data = new
                    {
                        userId = FarmerId,
                        liveId = getUserLiveSetting.Id,
                        liveName = getUserLiveSetting.LiveName,
                        liveDate = getUserLiveSetting.LiveDate.Date,
                        startTime = getUserLiveSetting.StartTime,
                        endTime = getUserLiveSetting.EndTime,
                        //要補上livePic
                        yturl = getUserLiveSetting.LivePic,
                        liveproudct = getUserLiveSetting.LiveProduct.Select(x => new
                        {
                            productId = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().ProductId,
                            productName = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().Product.ProductTitle,
                            productSize = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().Size,
                            liveprice = db.Specs.Where(y => y.Id == x.SpecId).FirstOrDefault().LivePrice,
                            liveproductId = x.Id,
                        }).ToList()
                    }
                };
                return Content(HttpStatusCode.OK, resultReturn);
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



    }
}
