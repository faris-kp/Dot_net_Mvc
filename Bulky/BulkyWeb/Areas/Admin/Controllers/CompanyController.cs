using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Company)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
      
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id)
        {
         
            if (id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else
            {
                //Update
                Company companyobj = _unitOfWork.Company.Get(u => u.Id == id);
                return View(companyobj);
            }

        }
        [HttpPost]
        public IActionResult Upsert(Company Companyobj)
        {
            if (ModelState.IsValid)
            {
                
                if (Companyobj.Id == 0)
                {
                    _unitOfWork.Company.Add(Companyobj);
                }
                else
                {
                    _unitOfWork.Company.Update(Companyobj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                return View(Companyobj);
            }


        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? CompanyDelete = _unitOfWork.Company.Get(u => u.Id == id);
        //    if (CompanyDelete == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CompanyDelete);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{
        //    Company? CompanyDelete = _unitOfWork.Company.Get(u => u.Id == id);
        //    if (CompanyDelete == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Company.Remove(CompanyDelete);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Company Deleted Successfully";
        //    return RedirectToAction("Index");

        //}

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (CompanyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });
            }
            _unitOfWork.Company.Remove(CompanyToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successfully" });
        }
    }
}
#endregion
