using FarmerPro.Models;
using FarmerPro.Models.ViewModel;
using FarmerPro.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Security.Principal;
using System.Web.Http;

namespace FarmerPro.Controllers
{
    public class OrderController : ApiController
    {
        private FarmerProDB db = new FarmerProDB();


        #region FCO-1 新增訂單(未付款)
        [HttpGet]
        [Route("api/order/")]
        [JwtAuthFilter]
        public IHttpActionResult CreateNewOrder([FromBody] CreateNewOrder input)  //只有驗證部分資訊可能會錯
        {
            try
            {
                //先驗證取得使用者ID
                int CustomerId = Convert.ToInt16(JwtAuthFilter.GetToken(Request.Headers.Authorization.Parameter)["Id"]);
                if (!ModelState.IsValid)
                {
                    var result = new
                    {
                        statusCode = 401,
                        status = "error",
                        message = "欄位格式不正確，請重新輸入",
                    };
                    return Content(HttpStatusCode.OK, result);
                }
                else
                {

                    var newOrder = new Order
                    {
                        Receiver = input.receiver,
                        Photo = input.phone,    //這個model要改名稱,
                        City = input.city,
                        District = input.district,
                        ZipCode = input.zipCode,
                        Address = input.address,
                        DeliveryFee = 100,
                        OrderSum = input.orderSum,
                        Shipment = false,
                        Guid = Guid.NewGuid(),
                        UserId = CustomerId,
                    };

                    db.Orders.Add(newOrder);
                    db.SaveChanges();
                    int OrderID = newOrder.Id;

                    var newOrderDetail = input.cartList.Select(x => new OrderDetail
                    {
                        Qty = x.qty,
                        SpecId = x.specId,
                        SubTotal = x.subTotal,
                        OrderId = OrderID,
                    }).ToList();

                    db.OrderDetails.AddRange(newOrderDetail);
                    db.SaveChanges();


                    var result = new
                    {
                        statusCode = 200,
                        status = "success",
                        message = "新增成功",
                    };
                    return Content(HttpStatusCode.OK, result);

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
}