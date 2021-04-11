using CarShowroom.Models;
using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CarShowroom.Controllers
{
    public class HomeController : Controller
    {
        CarShowroomDbContext DbContext = new CarShowroomDbContext();
        [HttpGet]
        public ViewResult Index(int? soCho,int? maHangXe, string currentFilter, string searchString, int? page)
        {

            CarShowroomDbContext context = new CarShowroomDbContext();
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var xe = from s in context.Xe
                     select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                xe = xe.Where(s => s.TenXe.Contains(searchString));
            }
            if (maHangXe.HasValue)
            {
                xe = xe.Where(s => s.MaHangXe == maHangXe);
            }
            if (soCho.HasValue)
            {
                xe = xe.Where(s => s.SoCho == soCho);
            }
            int pageSize = 6;
            int pageNumber = (page ?? 1);
            //
            return View(xe.OrderBy(x => x.MaXe).ToPagedList(pageNumber, pageSize));
        }
        [HttpGet]
        public ActionResult Detail(int? maXe)
        {
            var result = DbContext.Xe.Find(maXe);
            if (result != null)
            {
                return View(result);
            }
            return HttpNotFound();
        }
        public PartialViewResult MenuHangXe()
        {
         
                var dsHangXe = DbContext.HangXe.ToList();
                ViewBag.dsHangXe = dsHangXe;
                return PartialView();
            
        }

        public PartialViewResult MenuSoCho()
        {

            var dsSoCho =(from P in DbContext.Xe.AsEnumerable() select P.SoCho).Distinct().ToList();
            ViewBag.dsSoCho = dsSoCho;
            return PartialView();

        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        public ActionResult ThemVaoGioHang(int? maXe, int? soLuong)
        {
            if(soLuong <= 0||soLuong == null)
            {
                TempData["ThongBao"] = "Số lượng cần thuê phải lớn hơn 0. Mời thử lại";
                return RedirectToAction("Detail", new { maXe = maXe });
            }
            var xe = DbContext.Xe.Find(maXe);
            if(xe == null)
            {
                TempData["ThongBao"] = "Không tìm thấy xe trong database. Mời thử lại";
                return RedirectToAction("Detail",new { maXe = maXe});
            }
            if(xe.SoLuongXe < soLuong)
            {
                TempData["ThongBao"] = "Số lượng cần thuê không thể lớn hơn số lượng còn trong kho. Mời thử lại";
                return RedirectToAction("Detail", new { maXe = maXe });
            }
            return RedirectToAction("Them","GioHang",new { id = maXe, quantity =soLuong});
        }
        public ActionResult XeMoiNhat()
        {
            var listXe = DbContext.Xe.Take(5).OrderByDescending(m => m.MaXe).ToList();
            return PartialView("_XeMoiNhatPartial", listXe);
        }
    }
}