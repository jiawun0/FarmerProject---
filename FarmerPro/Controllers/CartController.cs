using FarmerPro.Models;
using FarmerPro.Securities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FarmerPro.Controllers
{
    public class CartController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region FGC-2 取得購物車清單
        [HttpGet]
        //自定義路由
        [Route("api/cart")]
        //[JwtAuthFilter]
        
        //使用 IHttpActionResult 作為返回 HTTP 回應類型
        public IHttpActionResult GetCartItem()
        {
            try
            {
                int CustomerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);
                
                var cartInfo = db.Carts.Where(c => c.UserId == CustomerId).Include(c=> c.CartItem)
                                  .Select(cart => new
                                  {
                                      totalOriginalPrice = cart.CartItem.Sum(c => c.Spec.Price * c.Qty),
                                      totalPromotionPrice = cart.CartItem.Sum(c => c.Spec.PromotePrice * c.Qty),
                                  }).Select(d => new
                                  {
                                      d.totalOriginalPrice,
                                      d.totalPromotionPrice,
                                      totalDiscount = d.totalOriginalPrice - d.totalPromotionPrice,
                                  });

                var cartItemInfo = db.Carts.Where(c => c.UserId == CustomerId).SelectMany(c=>c.CartItem)
                                           .Select(cartItem => new
                                               {
                                                   productId = cartItem.Spec.ProductId,
                                                   productTitle = cartItem.Spec.Product.ProductTitle, // Spec--Product--Title
                                                   productSpecId = cartItem.Spec.Id,
                                                   productSpecSize = cartItem.Spec.Size,
                                                   productSpecWeight = (int)cartItem.Spec.Weight,
                                                   cartItemOriginalPrice = cartItem.Spec.Price,
                                                   cartItemPromotionPrice = cartItem.Spec.PromotePrice,
                                                   cartItemQty = cartItem.Qty,
                                                   subtotal = cartItem.Qty * cartItem.Spec.PromotePrice,
                                                   //ProductImg = cartItem.Spec.Product.,photo != null ? photo.URL : "default-src",
                                                   productImg = new
                                                   {
                                                       src = db.Albums.Where(a => a.ProductId == cartItem.Spec.ProductId).FirstOrDefault().Photo.FirstOrDefault().URL ?? "default-src",
                                                       alt = cartItem.Spec.Product.ProductTitle,
                                                   },
                                               }).ToList();

                var cartItemLength = cartItemInfo.Count();

                if (!cartItemInfo.Any())
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
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
                        data = new
                        {
                            cartItemLength = cartItemLength,
                            cartInfo = cartInfo,
                            cartItemInfo = cartItemInfo
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

        #region FGC-1 加入購物車(要補上購物車數量欄位)
        [HttpPost]
        //自定義路由
        [Route("api/cart")]
        //[JwtAuthFilter]

        //使用 IHttpActionResult 作為返回 HTTP 回應類型
        public IHttpActionResult cartItemAll()
        {
            try
            {
                int CustomerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);

                var cartInfo = db.Carts.Where(c => c.UserId == CustomerId).Include(c => c.CartItem)
                                  .Select(cart => new
                                  {
                                      totalOriginalPrice = cart.CartItem.Sum(c => c.Spec.Price * c.Qty),
                                      totalPromotionPrice = cart.CartItem.Sum(c => c.Spec.PromotePrice * c.Qty),
                                  }).Select(d => new
                                  {
                                      d.totalOriginalPrice,
                                      d.totalPromotionPrice,
                                      totalDiscount = d.totalOriginalPrice - d.totalPromotionPrice,
                                  });

                var cartItemInfo = db.Carts.Where(c => c.UserId == CustomerId).SelectMany(c => c.CartItem)
                                           .Select(cartItem => new
                                           {
                                               productId = cartItem.Spec.ProductId,
                                               productTitle = cartItem.Spec.Product.ProductTitle, // Spec--Product--Title
                                               productSpecId = cartItem.Spec.Id,
                                               productSpecSize = cartItem.Spec.Size,
                                               productSpecWeight = (int)cartItem.Spec.Weight,
                                               cartItemOriginalPrice = cartItem.Spec.Price,
                                               cartItemPromotionPrice = cartItem.Spec.PromotePrice,
                                               cartItemQty = cartItem.Qty,
                                               subtotal = cartItem.Qty * cartItem.Spec.PromotePrice,
                                               //ProductImg = cartItem.Spec.Product.,photo != null ? photo.URL : "default-src",
                                               productImg = new
                                               {
                                                   src = db.Albums.Where(a => a.ProductId == cartItem.Spec.ProductId).FirstOrDefault().Photo.FirstOrDefault().URL ?? "default-src",
                                                   alt = cartItem.Spec.Product.ProductTitle,
                                               },
                                           }).ToList();

                var cartItemLength = cartItemInfo.Count();

                if (!cartItemInfo.Any())
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
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
                        data = new
                        {
                            cartItemLength = cartItemLength,
                            cartInfo = cartInfo,
                            cartItemInfo = cartItemInfo
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
}
