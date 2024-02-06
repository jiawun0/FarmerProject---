using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using FarmerPro.Securities;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        #region FGC-1 加入購物車(要補上購物車數量欄位)
        [HttpPost]
        //自定義路由
        [Route("api/cart")]
        //[JwtAuthFilter]

        //使用 IHttpActionResult 作為返回 HTTP 回應類型
        public IHttpActionResult AddCartItem([FromBody] GetCartItemClass input)
        {
            try
            {
                int CustomerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);

                //如果沒有就創建
                var cart = db.Carts.FirstOrDefault(c => c.UserId == CustomerId);
                if (cart == null)
                {
                    cart = new Cart { UserId = CustomerId };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                var CartId = cart.Id;
                var SpecId = input.productSpecId; //沒有此商品，請重新輸入
                var Qty = input.cartItemQty;  //數量不可為0
                var SubTotal = (int)input.subtotal;

                var SpecInfo = db.Specs.FirstOrDefault(s => s.Id == SpecId);

                if (SpecInfo == null)
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "沒有商品SpecId，請重新輸入",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else if (Qty <= 0)
                {
                    //result訊息
                    var result = new
                    {
                        statusCode = 402,
                        status = "error",
                        message = "數量不可為0，請重新輸入",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else
                {

                    //加入商品
                    var addcartitem = new CartItem
                    {
                        CartId = cart.Id,
                        SpecId = input.productSpecId, //沒有此商品，請重新輸入
                        Qty = input.cartItemQty,  //數量不可為0
                        SubTotal = (int)input.subtotal,
                    };
                    db.CartItems.Add(addcartitem);
                    db.SaveChanges();

                    var cartItemInfo = db.CartItems.Where(c => c.Cart.UserId == CustomerId).GroupBy(gruop=> gruop.SpecId)
                                               .Select(cartItemGruop => new
                                               {
                                                   productId = cartItemGruop.FirstOrDefault().Spec.ProductId,
                                                   productTitle = cartItemGruop.FirstOrDefault().Spec.Product.ProductTitle, // Spec--Product--Title
                                                   productSpecId = cartItemGruop.Key,
                                                   productSpecSize = cartItemGruop.FirstOrDefault().Spec.Size,
                                                   productSpecWeight = (int)cartItemGruop.FirstOrDefault().Spec.Weight,
                                                   cartItemOriginalPrice = cartItemGruop.FirstOrDefault().Spec.Price,
                                                   cartItemPromotionPrice = cartItemGruop.FirstOrDefault().Spec.PromotePrice,
                                                   cartItemQty = cartItemGruop.FirstOrDefault().Qty,
                                                   subtotal = cartItemGruop.FirstOrDefault().Qty * cartItemGruop.FirstOrDefault().Spec.PromotePrice,
                                                   productImg = new
                                                   {
                                                       src = db.Albums.Where(a => a.ProductId == cartItemGruop.FirstOrDefault().Spec.ProductId).FirstOrDefault().Photo.FirstOrDefault().URL ?? "default-src",
                                                       alt = cartItemGruop.FirstOrDefault().Spec.Product.ProductTitle,
                                                   },
                                               }).ToList();

                    var cartItemQtySum = cartItemInfo.Sum(ci => ci.cartItemQty);

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
                            message = "加入成功",
                            data = new 
                            {
                                cartItemQtySum,
                                cartItemInfo
                            }
                            
                        };
                        return Content(HttpStatusCode.OK, result);
                    }
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


        public class GetCartItemClass
        {
            [Display(Name = "規格spec編號")]
            public int productSpecId { get; set; }

            [Display(Name = "數量")]
            public int cartItemQty { get; set; }

            [Display(Name = "小計")]
            public double subtotal { get; set; }
        }
    }
}
