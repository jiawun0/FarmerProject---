using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using FarmerPro.Models;
using FarmerPro.Models.ViewModel;


namespace FarmerPro.Controllers
{
    public class FarmerController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region BFP-02 新增小農單一商品資料(不包含上傳相片)
        [HttpPost]
        [Route("api/farmer/product")]
        [JwtAuthFilter]
        public IHttpActionResult CreateProduct([FromBody] CreateProduct CreateProduct)
        {
            try
            {
                if (!ModelState.IsValid) // ViewModel沒有通過驗證
                {
                    var result = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "欄位輸入格式不正確，請重新輸入",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else
                {
                    //要確認上傳者
                    var HasProduct = db.Products.Where(x => x.ProductTitle == CreateProduct.productTitle).FirstOrDefault();
                    if (HasProduct != null)
                    {
                        var result = new
                        {
                            statusCode = 401,
                            status = "error",
                            message = "產品名稱已存在，請重新輸入",
                        };
                        return Content(HttpStatusCode.OK, result);
                    }
                    else
                    {
                        var newproduct = new Product // 加入新產品
                        {
                            ProductTitle = CreateProduct.productTitle,
                            Category = CreateProduct.category,
                            Period = CreateProduct.period,
                            Origin = CreateProduct.origin,
                            Storage = CreateProduct.storage,
                            Description = CreateProduct.description,
                            Introduction = CreateProduct.introduction,
                            ProductState = CreateProduct.productState,
                            UpdateStateTime = CreateProduct.productState == false ? (DateTime?)null : CreateProduct.updateStateTime,
                        };
                        db.Products.Add(newproduct);
                        db.SaveChanges();
                        int newProductId = newproduct.Id;

                        var newproductsmall = new Spec // 加入小產品Spec
                        {
                            Price = CreateProduct.smallOriginalPrice,
                            Stock = CreateProduct.smallStock,
                            PromotePrice = CreateProduct.smallPromotionPrice,
                            Weight = CreateProduct.smallWeight,
                            Size = false,
                            ProductId = newProductId,
                            Sales = 0,
                        };
                        db.Specs.Add(newproductsmall);
                        db.SaveChanges();
                        int newProductsmallId = newproductsmall.Id;

                        var newproductlarge = new Spec // 加入大產品Spec
                        {
                            Price = CreateProduct.largeOriginalPrice,
                            Stock = CreateProduct.largeStock,
                            PromotePrice = CreateProduct.largePromotionPrice,
                            Weight = CreateProduct.largeWeight,
                            Size = true,
                            ProductId = newProductId,
                            Sales = 0,
                        };
                        db.Specs.Add(newproductlarge);
                        db.SaveChanges();
                        int newProductlargeId = newproductlarge.Id;

                        var CreateNewProduct = db.Products.Where(x => x.Id == newProductId).FirstOrDefault();
                        var CreateNewProductSmall = db.Specs.Where(x => x.Id == newProductsmallId).FirstOrDefault();
                        var CreateNewProductLarge = db.Specs.Where(x => x.Id == newProductlargeId).FirstOrDefault();

                        var result = new
                        {
                            statusCode = 200,
                            status = "success",
                            message = "新增成功",
                            data = new
                            {
                                productId = CreateNewProduct.Id,
                                productTitle = CreateNewProduct.ProductTitle,
                                category = CreateNewProduct.Category,
                                period = CreateNewProduct.Period,
                                origin = CreateNewProduct.Origin,
                                storage = CreateNewProduct.Storage,
                                description = CreateNewProduct.Description,
                                introduction = CreateNewProduct.Introduction,
                                productState = CreateNewProduct.ProductState,
                                largeOriginalPrice = CreateNewProductLarge.Price,
                                largePromotionPrice = CreateNewProductLarge.PromotePrice,
                                largeWeight = CreateNewProductLarge.Weight,
                                largeStock = CreateNewProductLarge.Stock,
                                smallOriginalPrice = CreateNewProductSmall.Price,
                                smallPromotionPrice = CreateNewProductSmall.PromotePrice,
                                smallWeight = CreateNewProductSmall.Weight,
                                smallStock = CreateNewProductSmall.Stock,
                            }
                        };
                        return Content(HttpStatusCode.OK, result);

                    }
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
    }

    internal class JwtAuthFilterAttribute : Attribute
    {
    }
}
