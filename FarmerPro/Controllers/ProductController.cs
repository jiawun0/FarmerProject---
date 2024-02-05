﻿using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
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
        [Route("api/product")]
        //[Route("api/product/{liveqty}/{topsalesqty}/{promoteqty}/{fruitqyt}/{vegatqty}")]
        ///api/product?(liveqty=3)&(topsalesqty=6)&(promoteqty=4)&(fruitqyt)&(vegatqty)
        //使用 IHttpActionResult 作為返回 HTTP 回應類型

        public IHttpActionResult productindex(int topsalesqty = 6, int promoteqty = 4, int fruitqty = 3, int vegatqty = 3)
        {
            try
            {
                var topSaleProduct = (from p in db.Products
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
                                      }).Take(topsalesqty);

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
                var randomPromotionProducts = promotionProducts.OrderBy(x => Guid.NewGuid()).Take(promoteqty).ToList();


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
                var randomFruitProducts = fruitProducts.OrderBy(x => Guid.NewGuid()).Take(fruitqty).ToList();

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
                var randomVegetableProducts = vegetableProducts.OrderBy(x => Guid.NewGuid()).Take(vegatqty).ToList();


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
                            topSaleProduct = topSaleProduct.ToList(),
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

        #region FGP-3 取得特定商品細節資訊(包含小農介紹、小農產品推薦4筆)
        [HttpGet]
        //自定義路由
        [Route("api/product/{productId}")]
        //使用 IHttpActionResult 作為返回 HTTP 回應類型
        public IHttpActionResult productdetail(int productId)
        {
            try
            {
                var detailProduct = from p in db.Products
                                    join user in db.Users on p.UserId equals user.Id
                                    join s in db.Specs on p.Id equals s.ProductId
                                    from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                    where p.ProductState && p.Id == productId
                                    orderby p.CreatTime descending
                                    let largeSpec = db.Specs.FirstOrDefault(s => s.ProductId == p.Id && !s.Size) //大= F
                                    let smallSpec = db.Specs.FirstOrDefault(s => s.ProductId == p.Id && s.Size)  //小 = T

                                    select new
                                    {
                                        productId = p.Id,
                                        productTitle = p.ProductTitle,
                                        category = p.Category.ToString(),
                                        period = p.Period.ToString(),
                                        origin = p.Origin.ToString(),
                                        storage = p.Storage.ToString(),
                                        productDescription = p.Description,
                                        introduction = p.Introduction,
                                        largeOriginalPrice = largeSpec != null ? (int?)largeSpec.Price : null,
                                        largePromotionPrice = largeSpec != null ? (int?)largeSpec.PromotePrice : null,
                                        largeWeight = largeSpec != null ? (int?)largeSpec.Weight : null,
                                        largeStock = largeSpec != null ? (int?)largeSpec.Stock : null,
                                        smallOriginalPrice = smallSpec != null ? (int?)smallSpec.Price : null,
                                        smallPromotionPrice = smallSpec != null ? (int?)smallSpec.PromotePrice : null,
                                        smallWeight = smallSpec != null ? (int?)smallSpec.Weight : null,
                                        smallStock = smallSpec != null ? (int?)smallSpec.Stock : null,
                                        productImages = db.Photos.Where(ph => album != null && album.Id == ph.AlbumId).Select(ph => new
                                        {
                                            src = ph.URL,
                                            alt = p.ProductTitle
                                        }).ToList(),
                                        farmerName = user.NickName,
                                        farmerVision = user.Vision,
                                        farmerDescription = user.Description,
                                        farmerImg = new
                                        {
                                            src = user.Photo != null ? user.Photo : "default-src",
                                            alt = user.NickName
                                        }
                                    };

                //取得productId的UserId
                var productUserId = db.Products
                                .Where(p => p.Id == productId && p.ProductState)
                                .Select(p => new { p.Id, p.UserId })
                                .FirstOrDefault();

                //取得Product、Spec、Album、Photo的聯合資料
                var productInfoByUser = from p in db.Products
                                        join s in db.Specs on p.Id equals s.ProductId
                                        from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                        let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                        where p.UserId == productUserId.UserId
                                              && p.Id != productId
                                              && p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
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

                if (!detailProduct.Any())
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
                            detailProduct = detailProduct.FirstOrDefault(),
                            productInfoByUser = productInfoByUser.ToList(),
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

        #region FCI-1 搜尋特定產品(input)
        [HttpPost]
        //自定義路由
        [Route("api/product/search")]
        //使用 IHttpActionResult 作為返回 HTTP 回應類型
        public IHttpActionResult productsearch([FromBody] SerchProduct input)
        {
            try
            {
                string searchCheck = input.serchQuery;

                var searchProduct = from p in db.Products
                                    join s in db.Specs on p.Id equals s.ProductId
                                    from album in db.Albums.Where(a => p.Id == a.ProductId).DefaultIfEmpty()
                                    let photo = db.Photos.FirstOrDefault(ph => album != null && album.Id == ph.AlbumId)
                                    where p.ProductState && !s.Size // 確認p.ProductState = true && s.Size = false
                                       && p.ProductTitle.Contains(searchCheck)
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
                                        },
                                    };

                if (!searchProduct.Any())
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 400,
                        status = "error",
                        message = "沒有結果",
                        data = new object[] { }
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
                        data = searchProduct.ToList(),

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

    public class GetProductDetail
    {
        [Display(Name = "商品編號")]
        public int productId { get; set; }

        [Display(Name = "農產品名稱")]
        public string productTitle { get; set; }

        [Required]
        [Display(Name = "產品分類")]
        public ProductCategory category { set; get; }

        [Display(Name = "季節")]
        public ProductPeriod period { set; get; }

        [Display(Name = "產地")]
        public ProductOrigin origin { set; get; }

        [Display(Name = "保存方式")]
        public ProductStorage storage { set; get; }

        [Display(Name = "農產品簡述")]
        public string productDescription { get; set; }

        [Display(Name = "農產品介紹")]
        public string introduction { get; set; }

        [Display(Name = "相片路徑物件")]
        public string productImg { get; set; }

        [Display(Name = "相片路徑")]
        public string src { get; set; }

        [Display(Name = "相片alt")]
        public string alt { get; set; }

        [Display(Name = "小農姓名")]
        public int farmerName { get; set; }

        [Display(Name = "小農照片物件")]
        public string farmerImg { get; set; }

        [Display(Name = "小農理念")]
        public string farmerVision { get; set; }

        [Display(Name = "自我介紹")]
        public string farmerescription { get; set; }

        [Display(Name = "大原價")]
        public int? largeOriginalPrice { get; set; }

        [Display(Name = "大促銷價")]
        public int? largePromotionPrice { get; set; }

        [Display(Name = "大規格重量")]
        public double? largeWeight { get; set; }

        [Display(Name = "小規格重量")]
        public double? smallWeight { get; set; }

        [Display(Name = "大庫存量")]
        public int? largeStock { get; set; }

        [Display(Name = "小庫存量")]
        public int? smallStock { get; set; }
    }
    public class SerchProduct
    {
        [Display(Name = "搜尋")]
        public string serchQuery { get; set; }
    }
}

