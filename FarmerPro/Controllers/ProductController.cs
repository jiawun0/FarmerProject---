using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FarmerPro.Controllers
{
    public class ProductController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        /// <summary>
        /// 取得所有商品
        /// </summary>

        #region FGP-1 取得所有產品
        [HttpGet]
        //自定義路由
        [Route("api/product/all")]
        //使用 IHttpActionResult 作為返回 HTTP 回應類型

        public IHttpActionResult productall()
        {
            try
            {
                //取得Product、Spec、Album、Photo的聯合資料
                var productInfo = from p in db.Products
                                  join s in db.Specs on p.Id equals s.ProductId
                                  from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                  from photo in db.Photos.Where(ph => album != null && album.Id == ph.AlbumId).DefaultIfEmpty()
                                  where p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
                                  orderby p.CreatTime descending
                                  select new
                                  {
                                      productId = p.Id,
                                      productTitle = p.ProductTitle,
                                      smallOriginalPrice = s.Price,
                                      smallPromotionPrice = s.PromotePrice,
                                      productImg = new 
                                      {
                                          src = photo != null ? photo.URL : "default-src",
                                          alt = p.ProductTitle 
                                      }

                                  };

                if (productInfo.FirstOrDefault() == null)
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 400,
                        status = "error",
                        message = "取得失敗",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else
                {
                    // result 訊息
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "取得成功",
                        data = productInfo.ToList()
                    };
                    return Content(HttpStatusCode.OK, result);
                }
            }
            catch
            {
                //result訊息
                var result = new
                {
                    statusCode = 500,
                    status = "error",
                    message = "其他錯誤",
                };
                return Content(HttpStatusCode.OK, result);
            }
        }
    }
    #endregion

    public class GetAllProduct
    {
        [Display(Name = "商品編號")]
        public int productId { get; set; }

        [Display(Name = "農產品名稱")]
        public string productTitle { get; set; }

        [Display(Name = "原價")]
        public int smallOriginalPrice { get; set; }

        [Display(Name = "促銷價")]
        public int? smallPromotionPrice { get; set; }

        [Display(Name = "相簿編號")]
        public int album_Id { get; set; }

        [Display(Name = "相片路徑物件")]
        public string productImg { get; set; }

        [Display(Name = "相片路徑")]
        public string src { get; set; }

        [Display(Name = "相片alt")]
        public string alt { get; set; }
    }
}

