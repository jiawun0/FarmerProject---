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
using System.Web.Http.Results;

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
                                  let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
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



                if (!productInfo.Any())
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
        #endregion

        #region FGP-2 取得live產品、熱銷產品、特價促銷產品、水果產品、蔬菜產品
        [HttpGet]
        //自定義路由
        [Route("api/product/liveProduct")]
        ///api/product?(liveqty=3)&(topsalesqty)&(promoteqty=6)&(fruitqyt)&(vegatqty)
        //使用 IHttpActionResult 作為返回 HTTP 回應類型

        public IHttpActionResult productindex()
        {
            try
            {
                var dayOfWeek = new Dictionary<DayOfWeek, string>
                {
                    { DayOfWeek.Sunday, "日" },
                    { DayOfWeek.Monday, "一" },
                    { DayOfWeek.Tuesday, "二" },
                    { DayOfWeek.Wednesday, "三" },
                    { DayOfWeek.Thursday, "四" },
                    { DayOfWeek.Friday, "五" },
                    { DayOfWeek.Saturday, "六" }
                };

                var latestLiveSets = (from livesetDate in db.LiveSettings
                                      where livesetDate.LiveDate >= DateTime.Today
                                      orderby livesetDate.LiveDate ascending
                                      select livesetDate)
                                      .Take(3)
                                      .ToList();

                var liveProduct = from p in db.Products
                                  join user in db.Users on p.UserId equals user.Id
                                  join s in db.Specs on p.Id equals s.ProductId
                                  join livepro in db.LiveProducts on s.Id equals livepro.SpecId
                                  //join liveset in latestLiveSets on livepro.LiveSettingId equals liveset.Id
                                  from liveset in db.LiveSettings.Where(ls => user.Id == ls.UserId).DefaultIfEmpty()
                                  from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                  let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                  where p.ProductState && s != null && s.LivePrice.HasValue && s.LivePrice.Value != 0
                                  orderby p.CreatTime descending
                                  select new
                                  {
                                      productId = p.Id,
                                      productTitle = p.ProductTitle,
                                      livePrice = s.LivePrice,
                                      farmerName = user.NickName,
                                      //liveDate = liveset != null ? liveset.LiveDate.ToString("yyyy.MM.dd") + dayOfWeek[liveset.LiveDate.DayOfWeek] + liveset.StartTime.ToString("HH:mm") : "",
                                      productImg = new
                                      {
                                          src = photo != null ? photo.URL : "default-src",
                                          alt = p.ProductTitle
                                      },
                                      farmerImg = new
                                      {
                                          src = user.Photo != null ? user.Photo : "default-src",
                                          alt = user.NickName
                                      }
                                  };

                var hotSaleProduct = from p in db.Products
                                     join s in db.Specs on p.Id equals s.ProductId
                                     from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                     let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                     where p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
                                     orderby s.Sales descending, p.CreatTime descending
                                     select new
                                     {
                                         productId = p.Id,
                                         productTitle = p.ProductTitle,
                                         description = p.Description,
                                         smallOriginalPrice = s.Price,
                                         smallPromotionPrice = s.PromotePrice,
                                         productImg = new
                                         {
                                             src = photo != null ? photo.URL : "default-src",
                                             alt = p.ProductTitle
                                         },
                                     };

                var promotionProduct = from p in db.Products
                                       join user in db.Users on p.UserId equals user.Id
                                       join s in db.Specs on p.Id equals s.ProductId
                                       from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                       let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                       where p.ProductState
                                       orderby p.CreatTime descending
                                       select new
                                       {
                                           productId = p.Id,
                                           productTitle = p.ProductTitle,
                                           farmerName = user.NickName,
                                           origin = p.Origin,
                                           smallOriginalPrice = s.Price,
                                           smallPromotionPrice = s.PromotePrice,
                                           productImg = new
                                           {
                                               src = photo != null ? photo.URL : "default-src",
                                               alt = p.ProductTitle
                                           },
                                           farmerImg = new
                                           {
                                               src = user.Photo != null ? user.Photo : "default-src",
                                               alt = user.NickName
                                           }
                                       };

                // 执行查询并将结果转换为列表
                var promotionProducts = promotionProduct.ToList();

                // 在内存中随机排序并取前四条记录
                var randomPromotionProducts = promotionProducts.OrderBy(x => Guid.NewGuid()).Take(4).ToList();

                
                var fruitProduct = from p in db.Products
                                  join s in db.Specs on p.Id equals s.ProductId
                                  from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                  let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                  where ((int)p.Category) == 1 && p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
                                   orderby p.CreatTime descending
                                  select new
                                  {
                                      productId = p.Id,
                                      productTitle = p.ProductTitle,
                                      description = p.Description,
                                      smallOriginalPrice = s.Price,
                                      smallPromotionPrice = s.PromotePrice,
                                      productImg = new
                                      {
                                          src = photo != null ? photo.URL : "default-src",
                                          alt = p.ProductTitle
                                      }

                                  };

                var fruitProducts = fruitProduct.ToList();
                var randomFruitProducts = fruitProducts.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

                var vegetableProduct = from p in db.Products
                                   join s in db.Specs on p.Id equals s.ProductId
                                   from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                   let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                   where ((int)p.Category) == 0 && p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
                                   orderby p.CreatTime descending
                                   select new
                                   {
                                       productId = p.Id,
                                       productTitle = p.ProductTitle,
                                       description = p.Description,
                                       smallOriginalPrice = s.Price,
                                       smallPromotionPrice = s.PromotePrice,
                                       productImg = new
                                       {
                                           src = photo != null ? photo.URL : "default-src",
                                           alt = p.ProductTitle
                                       }

                                   };

                var vegetableProducts = vegetableProduct.ToList();
                var randomVegetableProducts = vegetableProducts.OrderBy(x => Guid.NewGuid()).Take(3).ToList();


                if (!promotionProduct.Any())
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
                        data = new
                        {
                            liveProduct = liveProduct.ToList(),
                            hotSaleProduct = hotSaleProduct.ToList(),
                            promotionProduct = promotionProduct.ToList(),
                            fruitProduct = fruitProduct.ToList(),
                            vegetableProduct = vegetableProduct.ToList()
                        }
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
        #endregion
    }




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

    public class GetProductIndex
    {
        [Display(Name = "商品編號")]
        public int productId { get; set; }

        [Display(Name = "農產品名稱")]
        public string productTitle { get; set; }

        [Display(Name = "直播價")]
        public int livePrice { get; set; }

        [Display(Name = "小農姓名")]
        public int farmerName { get; set; }

        [Display(Name = "直播日期")]
        public int liveDate { get; set; }

        [Display(Name = "相片路徑物件")]
        public string productImg { get; set; }

        [Display(Name = "相片路徑")]
        public string src { get; set; }

        [Display(Name = "相片alt")]
        public string alt { get; set; }

        [Display(Name = "小農照片物件")]
        public string farmerImg { get; set; }

        [Display(Name = "農產品簡述")]
        public string description { get; set; }

        [Display(Name = "產地")]
        public ProductOrigin origin { set; get; }
    }
}

