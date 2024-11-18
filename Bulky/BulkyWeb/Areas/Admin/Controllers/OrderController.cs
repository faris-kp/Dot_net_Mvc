using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{
		
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public Ordervm Ordervm { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId) 
        {
            Ordervm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includePropeties: "ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId == orderId, includePropeties:"Product")
            };
            return View(Ordervm);
        }
        [HttpPost]
        [Authorize(Roles =SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail(int orderId)
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == Ordervm.OrderHeader.Id);
            orderHeaderFromDb.Name = Ordervm.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = Ordervm.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = Ordervm.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = Ordervm.OrderHeader.City;
            orderHeaderFromDb.State = Ordervm.OrderHeader.State;
            orderHeaderFromDb.PostalCode = Ordervm.OrderHeader.PostalCode;
            if(!string.IsNullOrEmpty(Ordervm.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = Ordervm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(Ordervm.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = Ordervm.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new {orderId =orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(Ordervm.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderId =Ordervm.OrderHeader.Id});

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader =  _unitOfWork.OrderHeader.Get(u=>u.Id == Ordervm.OrderHeader.Id);  
            orderHeader.TrackingNumber =Ordervm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = Ordervm.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymenDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderId = Ordervm.OrderHeader.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == Ordervm.OrderHeader.Id);
            if (orderHeader.PaymentStatus == SD.StatusApproved) 
            {
                //need to write the logic of refund
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(Ordervm.OrderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cannelled Successfully";
            return RedirectToAction(nameof(Details), new { orderId = Ordervm.OrderHeader.Id });
        }
            #region API CALLS

            [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> objorderHeader;

            if(User.IsInRole(SD.Role_Admin)|| User.IsInRole(SD.Role_Employee))
            {
                objorderHeader = _unitOfWork.OrderHeader.GetAll(includePropeties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objorderHeader = _unitOfWork.OrderHeader.GetAll(u=>u.ApplicationUserId == userId, includePropeties: "ApplicationUser").ToList();

            }
            switch (status)
            {
                case "pending":
                    objorderHeader = objorderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objorderHeader = objorderHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objorderHeader = objorderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objorderHeader = objorderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = objorderHeader });
		}

	
		#endregion
	}
}
