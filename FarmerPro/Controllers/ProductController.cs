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
                                  join s in db.Specs on p.Id equals s.ProductId into productSpecs
                                  from spec in productSpecs.DefaultIfEmpty()
                                  join a in db.Albums on p.Id equals a.ProductId into productAlbums
                                  from album in productAlbums.DefaultIfEmpty()
                                  join ph in db.Photos on album.Id equals ph.AlbumId into albumPhotos
                                  from photo in albumPhotos.DefaultIfEmpty()
                                  where p.ProductState && !spec.Size //p.ProductState = true && spec.Size = false
                                  orderby p.CreatTime descending
                                  select new
                                  {
                                      product_Id = p.Id,
                                      productTitle = p.ProductTitle,
                                      price = spec.Price,
                                      promotePrice =spec.PromotePrice,
                                      productImg = new
                                      {
                                          src = photo.URL,
                                          alt = $"{p.ProductTitle}{photo.Id}"
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
        public int product_Id { get; set; }

        [Display(Name = "農產品名稱")]
        public string productTitle { get; set; }

        [Display(Name = "上架狀態")]
        public bool productState { get; set; } = false;

        [Display(Name = "上架時間")]
        public DateTime? updateStateTime { get; set; }

        [Display(Name = "建立時間")]
        public DateTime creatTime { get; set; } = DateTime.Now;

        [Display(Name = "原價")]
        public int price { get; set; }

        [Display(Name = "促銷價")]
        public int? promotePrice { get; set; }

        [Display(Name = "規格大小")]
        public bool size { get; set; }

        [Display(Name = "商品Id")]
        public int productId { get; set; }

        [Display(Name = "相簿編號")]
        public int album_Id { get; set; }

        [Display(Name = "相片路徑")]
        public string productImg { get; set; }

        [Display(Name = "相簿Id")]
        public int albumId { get; set; }

        [Display(Name = "相片編號")]
        public int photo_Id { get; set; }

        [Display(Name = "相片alt")]
        public int alt { get; set; }
    }
}

