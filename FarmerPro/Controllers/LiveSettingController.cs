using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using FarmerPro.Securities;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Results;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FarmerPro.Controllers
{
    public class LiveSettingController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();

        #region BFL-1 新增後台直播資訊(新增產品直播價)
        [HttpPost]
        [Route("api/livesetting")]
        [JwtAuthFilter]
        public async Task<IHttpActionResult> CreateNewLiveSetting([FromBody] CreateNewLiveSetting input)
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            //try
            //{
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

                bool HasPhotoFile = false;
                List<string> imglList = new List<string>();
                // 檢查請求是否包含 multipart/form-data.
                if (Request.Content.IsMimeMultipartContent())
                {
                    HasPhotoFile= true;
                    string root = HttpContext.Current.Server.MapPath($"~/upload/livepic/{FarmerId}");
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    // 讀取 MIME 資料
                    var provider = new MultipartMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);
                    //遍歷 provider.Contents 中的每個 content，處理多個圖片檔案
                    foreach (var content in provider.Contents)
                    {
                        
                        // 取得檔案副檔名
                        string fileNameData = content.Headers.ContentDisposition.FileName.Trim('\"');
                        string fileType = fileNameData.Remove(0, fileNameData.LastIndexOf('.')); // .jpg

                        // 定義檔案名稱
                        string fileName = FarmerId.ToString()  + DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileType;

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
                        string url = WebConfigurationManager.AppSettings["Serverurl"].ToString() + $"/upload/livepic/{FarmerId}" + fileName;
                        imglList.Add(url);
                    }
                }


                var NewLiveSetting = new LiveSetting
                {
                    LiveName = input.liveName,
                    LiveDate = input.liveDate.Date,
                    StartTime = input.startTime,
                    EndTime = input.endTime,
                    YTURL = input.yturl,
                    LivePic= HasPhotoFile? imglList[0]:null,
                    ShareURL = input.yturl.Substring(input.yturl.LastIndexOf('/')+1), //這邊可能要改
                    UserId = FarmerId,
                };

                db.LiveSettings.Add(NewLiveSetting);
                db.SaveChanges();
                int LiveSettingId = NewLiveSetting.Id;

                var LiveProduct = input.liveproduct.Select(LP => new LiveProduct
                {
                    IsTop = false,
                    LiveSettingId = LiveSettingId,
                    SpecId = db.Products.Where(x => x.Id == LP.productId).AsEnumerable().FirstOrDefault().Spec.Where(x => x.Size == Convert.ToBoolean(LP.productSize)).FirstOrDefault().Id,
                }).ToList();

                db.LiveProducts.AddRange(LiveProduct);
                db.SaveChanges();

                foreach (var LP in input.liveproduct)
                {
                    var specitem = db.Products.Where(x => x.Id == LP.productId).AsEnumerable().FirstOrDefault().Spec.Where(x => x.Size == Convert.ToBoolean(LP.productSize)).FirstOrDefault();
                    if (specitem != null)
                    {
                        specitem.LivePrice = LP.liveprice;
                    }
                }
                db.SaveChanges();

                var searchLiveSetting = db.LiveSettings.Where(x => x.Id == LiveSettingId)?.FirstOrDefault();

                var resultReturn = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "新增成功",
                    data = new
                    {
                        liveId= searchLiveSetting.Id,
                        liveName = searchLiveSetting.LiveName,
                        liveDate = searchLiveSetting.LiveDate.Date,
                        startTime = searchLiveSetting.StartTime,
                        endTime = searchLiveSetting.EndTime,
                        livePic= searchLiveSetting.LivePic,
                        yturl = searchLiveSetting.YTURL,
                        liveproduct = searchLiveSetting.LiveProduct.Select(x => new
                        {
                            productId = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().ProductId,
                            productName = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Product.ProductTitle,
                            productSize = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Size,
                            liveprice = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().LivePrice,
                            liveproductId= x.Id,
                        }).ToList()
                    }
                };
                return Content(HttpStatusCode.OK, resultReturn);
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


        #region BFL-2 修改後台直播資訊(修改產品直播價)
        [HttpPut]
        [Route("api/livesetting")]
        [JwtAuthFilter]
        public IHttpActionResult ReviseLiveSetting([FromBody] CreateNewLiveSetting input)
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            //try
            //{
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

                var searchLiveSetting = db.LiveSettings.Where(x => x.UserId == FarmerId && x.Id==input.liveId)?.FirstOrDefault();
                searchLiveSetting.LiveName = input.liveName;
                searchLiveSetting.LiveDate = input.liveDate;
                searchLiveSetting.StartTime = input.startTime;
                searchLiveSetting.EndTime = input.endTime;
                searchLiveSetting.YTURL = input.yturl;
                searchLiveSetting.ShareURL = input.yturl.Substring(input.yturl.LastIndexOf('/')+1); //這邊可能要改
                //圖片部分還沒有處理
                db.SaveChanges();
                int LiveSettingId = searchLiveSetting.Id;

                for (int i = 0; i < searchLiveSetting.LiveProduct.Count; i++) 
                {
                    //改資liveproduct料表中的產品SPEC
                    searchLiveSetting.LiveProduct.ToList()[i].SpecId =
                        db.Specs.AsEnumerable().Where(x => x.ProductId == input.liveproduct[i].productId && x.Size == Convert.ToBoolean(input.liveproduct[i].productSize)).FirstOrDefault().Id;
                    //改變spec資料表中的產品價格
                    var ReviseSpecLivePrice = db.Products.AsEnumerable().Where(x => x.Id == input.liveproduct[i].productId).FirstOrDefault().Spec.Where(x => x.Size == Convert.ToBoolean(input.liveproduct[i].productSize)).FirstOrDefault();
                    if (ReviseSpecLivePrice != null)
                    {
                        ReviseSpecLivePrice.LivePrice = input.liveproduct[i].liveprice;
                    }
                }
                db.SaveChanges();
    

                    var GetUpdateLiveSetting = db.LiveSettings.Where(x => x.UserId == FarmerId && x.Id == input.liveId)?.FirstOrDefault();
                var resultReviser = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "修改成功",
                    data = new
                    {
                        liveId = GetUpdateLiveSetting.Id,
                        liveName = GetUpdateLiveSetting.LiveName,
                        liveDate = GetUpdateLiveSetting.LiveDate.Date,
                        startTime = GetUpdateLiveSetting.StartTime,
                        endTime = GetUpdateLiveSetting.EndTime,
                        livePic= GetUpdateLiveSetting.LivePic,   //要補上livePic
                        yturl = GetUpdateLiveSetting.LivePic,
                        liveproudct = GetUpdateLiveSetting.LiveProduct.Select(x => new
                        {
                            productId = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().ProductId,
                            productName = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Product.ProductTitle,
                            productSize = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Size,
                            liveprice = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().LivePrice,
                            liveproductId = x.Id,
                        }).ToList()
                    }
                };
                return Content(HttpStatusCode.OK, resultReviser);
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


        #region BFL-3 取得後台直播資訊(包含產品直播價)
        [HttpGet]
        [Route("api/livesetting")]
        [JwtAuthFilter]
        public IHttpActionResult RenderLiveSettingInfor(int liveId)
        {
            // 解密後會回傳 Json 格式的物件 (即加密前的資料)
            var jwtObject = JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter);
            int FarmerId = (int)jwtObject["Id"];

            try
            {
                var getUserLiveSetting = db.LiveSettings.Where(x => x.UserId == FarmerId && x.Id== liveId)?.FirstOrDefault();

                var resultReturn = new
                {
                    statusCode = 200,
                    status = "success",
                    message = "取得成功",
                    data = new
                    {
                        liveId = getUserLiveSetting.Id,
                        liveName = getUserLiveSetting.LiveName,
                        liveDate = getUserLiveSetting.LiveDate.Date,
                        startTime = getUserLiveSetting.StartTime,
                        endTime = getUserLiveSetting.EndTime,
                        livepic= getUserLiveSetting.LivePic,
                        yturl = getUserLiveSetting.LivePic,
                        liveproudct = getUserLiveSetting.LiveProduct?.Select(x => new
                        {
                            productId = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().ProductId,
                            productName = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Product.ProductTitle,
                            productSize = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().Size,
                            liveprice = db.Specs.Where(y => y.Id == x.SpecId)?.FirstOrDefault().LivePrice,
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
