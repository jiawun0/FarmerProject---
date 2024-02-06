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
using FarmerPro.Securities;
using System.Web.Http.Controllers;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Validation;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Security.Policy;
using static System.Net.WebRequestMethods;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Web.Http.Results;
using System.Web.Configuration;


namespace FarmerPro.Controllers
{
    public class FarmerController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region BFP-01 取得小農單一商品資料(有包含相片)
        [HttpGet]
        [Route("api/farmer/product")]
        [JwtAuthFilter]
        public IHttpActionResult GetSingleFarmerProduct([FromUri] int Id)
        {
            //try
            //{
            //先取得小農Id
            int farmerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);
            var SingleProduct = db.Products.Where(x => x.Id == Id)?.FirstOrDefault();
            if (SingleProduct != null)
            {
                var SingleProductLarge = db.Specs.Where(x => x.Size ==true && x.ProductId == SingleProduct.Id)?.FirstOrDefault();
                var SingleProductSmall = db.Specs.Where(x => x.Size==false && x.ProductId == SingleProduct.Id)?.FirstOrDefault();
                var ProductPhoto = db.Albums.Where(x => x.ProductId == SingleProduct.Id).FirstOrDefault()?.Photo;

                //如果產品id=0，刪除相簿=>這可能要獨立一隻API
                var linkProductandAlbum = db.Albums.Where(x => x.UserId == farmerId && x.ProductId == 0)?.FirstOrDefault();
                if (linkProductandAlbum != null)
                {
                        //刪除實體資料夾的東西
                        string root = HttpContext.Current.Server.MapPath($"~/upload/farmer/{linkProductandAlbum.UserId}/{linkProductandAlbum.Id}"); //建立假的ID
                        if (Directory.Exists(root))
                        {
                            Directory.Delete(root, true);
                        }

                        //刪除 linkProductandAlbum-資料庫資料
                        db.Albums.Remove(linkProductandAlbum);
                        db.SaveChanges();
                }
                   


                    var result = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "取得成功",
                    data = new
                    {
                        productId = SingleProduct?.Id,
                        productTitle = SingleProduct?.ProductTitle,
                        category = SingleProduct?.Category.ToString(),
                        period = SingleProduct?.Period.ToString(),
                        origin = SingleProduct?.Origin.ToString(),
                        storage = SingleProduct?.Storage.ToString(),
                        description = SingleProduct?.Description,
                        introduction = SingleProduct?.Introduction,
                        productState = SingleProduct?.ProductState,
                        largeOriginalPrice = SingleProductLarge?.Price,
                        largePromotionPrice = SingleProductLarge?.PromotePrice,
                        largeWeight = SingleProductLarge?.Weight,
                        largeStock = SingleProductLarge?.Stock,
                        smallOriginalPrice = SingleProductSmall?.Price,
                        smallPromotionPrice = SingleProductSmall?.PromotePrice,
                        smallWeight = SingleProductSmall?.Weight,
                        smallStock = SingleProductSmall?.Stock,
                        photos = ProductPhoto?.Select(pic => new
                        {
                            photoId= pic.Id,
                            src = pic.URL,
                            alt = SingleProduct.ProductTitle.ToString() + pic.Id.ToString(),
                        }).ToList(),
                    }
                };
                return Content(HttpStatusCode.OK, result);
            }
            else
            {
                var result = new
                {
                    statusCode = 401,
                    status = "error",
                    message = "產品Id不存在",
                };
                return Content(HttpStatusCode.OK, result);
            }

            //}
            //catch
            //{
            //    var result = new
            //    {
            //        statusCode = 500,
            //        status = "error",
            //        message = "其他錯誤",
            //    };
            //    return Content(HttpStatusCode.OK, result);
            //}
        }

        #endregion

        #region BFP-02 新增小農單一商品資料(不包含上傳相片)
        [HttpPost]
        [Route("api/farmer/product")]
        [JwtAuthFilter]
        public IHttpActionResult CreateSingleFarmerProduct([FromBody] CreateProduct CreateProduct)
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
                    //先取得小農Id
                    int farmerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);
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
                            UserId= farmerId,
                        };
                        db.Products.Add(newproduct);
                        db.SaveChanges();
                        int newProductId = newproduct.Id;

                        //鏈結產品和相簿
                        var linkProductandAlbum = db.Albums.Where(x => x.UserId == farmerId && x.ProductId == 0)?.FirstOrDefault();
                        if (linkProductandAlbum != null) 
                        {
                            linkProductandAlbum.ProductId = newproduct.Id;
                            db.SaveChanges();
                        }


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
                                category = CreateNewProduct.Category.ToString(),
                                period = CreateNewProduct.Period.ToString(),
                                origin = CreateNewProduct.Origin.ToString(),
                                storage = CreateNewProduct.Storage.ToString(),
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

        #region BFP-03 上傳回拋小農單一商品圖片(多張，及時渲染)
        [HttpPost]
        [Route("api/farmer/product/pic")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> UploadSingleFarmerProductPhoto()
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            var userExist = db.Users.Any(u => u.Id == FarmerId);

            if (userExist)//使用者存在
            {
                // 檢查請求是否包含 multipart/form-data.
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var checkalbum=db.Albums.Where(x=>x.UserId== FarmerId&& x.ProductId==0)?.FirstOrDefault();
                int AlbumIdCreate;
                if (checkalbum == null)
                {
                    var newalbum = new Album // 建立相簿
                    {
                        ProductId = 0, //初始化相簿ID而已，這個ProductId後續要再改
                        UserId = FarmerId //UserId= FarmerId,才能鏈結!!!
                    };
                    db.Albums.Add(newalbum);
                    db.SaveChanges();
                    AlbumIdCreate = newalbum.Id;
                }
                else 
                {
                    AlbumIdCreate = checkalbum.Id;
                }

            // 檢查資料夾是否存在，若無則建立
            string root = HttpContext.Current.Server.MapPath($"~/upload/farmer/{FarmerId}/{AlbumIdCreate}"); 
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                }

                try
                {
                    // 讀取 MIME 資料
                    var provider = new MultipartMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var content in provider.Contents) //檢查附檔名類型
                {
                    string fileNameData = content.Headers.ContentDisposition.FileName.Trim('\"');
                    string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')).ToLower(); // .jpg
                    if (fileType != ".jpg" && fileType != ".jpeg" && fileType != ".png") 
                    {
                        var resultfileType = new
                        {
                            statusCode = 400,
                            status = "error",
                            message = "上傳失敗，請確認圖檔格式",
                        };
                        return Content(HttpStatusCode.OK, resultfileType);
                    }
                }

                //檢查上傳數量
                var photoNumberCheck = db.Photos.Where(x => x.AlbumId == AlbumIdCreate)?.Count();
                if (photoNumberCheck != null && (photoNumberCheck+ provider.Contents.Count)>5) 
                {
                    var resultfileType = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "上傳失敗，請確認上傳圖片數量",
                    };
                    return Content(HttpStatusCode.OK, resultfileType);
                }
                else if(photoNumberCheck==null && provider.Contents.Count>5)
                {
                    var resultfileType = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "上傳失敗，請確認上傳圖片數量",
                    };
                    return Content(HttpStatusCode.OK, resultfileType);
                }

                    List<List<string>> imglList = new List<List<string>>();
                //遍歷 provider.Contents 中的每個 content，處理多個圖片檔案
                foreach (var content in provider.Contents)
                    {
                         List<string> imgdetail = new List<string>();
                        // 取得檔案副檔名
                        string fileNameData = content.Headers.ContentDisposition.FileName.Trim('\"');
                        string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                        // 定義檔案名稱
                        string fileName = FarmerId.ToString() + AlbumIdCreate.ToString() + DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileType;

                        // 儲存圖片
                        var fileBytes = await content.ReadAsByteArrayAsync();
                        var outputPath = Path.Combine(root, fileName);
                        using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                        {
                            await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                        }

                        //// 載入原始圖片，直接存入伺服器(未裁切)
                        //using (var image = Image.Load<Rgba32>(outputPath))
                        //{
                        //    // 儲存裁切後的圖片
                        //    image.Save(outputPath);
                        //}

                        // 載入原始圖片，調整圖片大小
                        using (var image = Image.Load<Rgba32>(outputPath))
                        {

                            // 設定最大檔案大小 (2MB)
                            long maxFileSizeInBytes = 2 * 1024 * 1024;

                            // 計算目前圖片的檔案大小
                            using (var memoryStream = new MemoryStream())
                            {
                                image.Save(memoryStream, new JpegEncoder());
                                long currentFileSize = memoryStream.Length;

                                // 檢查檔案大小是否超過限制
                                if (currentFileSize > maxFileSizeInBytes)
                                {

                                    // 如果超過，可能需要進一步調整，或者進行其他處理
                                    // 這裡僅僅是一個簡單的示例，實際應用可能需要更複雜的處理邏輯
                                    //// 設定裁切尺寸
                                    int MaxWidth = 800;   // 先設定800px
                                    int MaxHeight = 600;  // 先設定600px

                                    // 裁切圖片
                                    image.Mutate(x => x.Resize(new ResizeOptions
                                    {
                                        Size = new Size(MaxWidth, MaxHeight),
                                        Mode = ResizeMode.Max
                                    }));

                                }
                                else { }
                            }
                            // 儲存後的圖片
                            image.Save(outputPath);
                        }

                    //加入至List
                    //imglList.Add(fileName);
                    imgdetail.Add(fileName);
                    string url = WebConfigurationManager.AppSettings["Serverurl"].ToString() + $"/upload/farmer/{FarmerId}/{AlbumIdCreate}/" + fileName;
                    imgdetail.Add(url);
                    string albumId = AlbumIdCreate.ToString();
                    imgdetail.Add(albumId);
                    imglList.Add(imgdetail);
                }

                    //將相片加入資料庫
                    var newPhotos = imglList.Select(imgs => new Photo
                    {  
                        URL = imgs[1],
                        AlbumId = Convert.ToInt16(imgs[2]) //這邊有問題
                    }).ToList();
                    db.Photos.AddRange(newPhotos);
                    db.SaveChanges();
              
                    //撈取相片資料庫
                    var uploadedPhotos = db.Photos.Where(x => x.AlbumId == AlbumIdCreate).AsEnumerable();

                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "上傳成功",
                        data = uploadedPhotos.Select(x => new
                        {
                            photoId=x.Id,
                            src = x.URL,
                            alt = x.URL.Substring(x.URL.LastIndexOf('/')+1),
                        }).ToList(),
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                catch (DbEntityValidationException ex)
                {
                    // Handle entity validation errors
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                    var fullErrorMessage = string.Join("; ", errorMessages);
                    var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                    return BadRequest(exceptionMessage);
                }
            }
            else
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
        //需要再補上圖片delete的API
        //可能要獨立一隻API，空白表單讀取get時候，清空之前的暫存相簿

    }


}
