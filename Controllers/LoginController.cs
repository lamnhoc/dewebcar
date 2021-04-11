using CarShowroom.Models;
using CarShowroom.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CarShowroom.Controllers
{
    public class LoginController : Controller
    {
        private CarShowroomDbContext Dbcontext = new CarShowroomDbContext();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangNhap(string tenTaiKhoan, string matKhau)
        {
            if (ModelState.IsValid)
            {
                var result = Dbcontext.TaiKhoan.Where(m => m.TenTaiKhoan == tenTaiKhoan && m.MatKhau == matKhau).FirstOrDefault();
                if (result != null)
                {
                    var khachHang = Dbcontext.KhachHang.Find(result.MaTaiKhoan);
                    Session.Add(UserSession.USER_SESSION, khachHang);
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["ThongBaoLoi"] = "Tài khoản hoặc mật khẩu không đúng. Mời thử lại!";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public ActionResult DangKy()
        {
            List<string> listNH = new List<string>()
            {
                "TPBank",
                "VietcomBank",
                "ViettinBank",
                "SeABank",
                "VPBank",
                "SacomBank",
                "TechcomBank",
                "BIDV",
                "DongABank"
            };
            SelectList selectList = new SelectList(listNH);
            ViewBag.listNH = selectList;
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(DangKyViewModel dangKyViewModel)
        {
            TaiKhoan taiKhoan = Dbcontext.TaiKhoan.Where(m => m.TenTaiKhoan == dangKyViewModel.TaiKhoan.TenTaiKhoan).FirstOrDefault();
            if (taiKhoan == null)
            {
                if (ModelState.IsValid)
                {
                    if (dangKyViewModel.TaiKhoan.MatKhau == dangKyViewModel.NhapLaiMatKhau)
                    {
                        TaiKhoan newUser = new TaiKhoan()
                        {
                            TenTaiKhoan = dangKyViewModel.TaiKhoan.TenTaiKhoan,
                            MatKhau = dangKyViewModel.TaiKhoan.MatKhau,
                            QuyenHan = Models.Enum.QuyenHan.Member,
                            KhachHang = new KhachHang()
                            {
                                TenKhachHang = dangKyViewModel.KhachHang.TenKhachHang,
                                DiaChi = dangKyViewModel.KhachHang.DiaChi,
                                NamSinh = dangKyViewModel.KhachHang.NamSinh,
                                SoTaiKhoan = dangKyViewModel.KhachHang.SoTaiKhoan,
                                CMND = dangKyViewModel.KhachHang.CMND,
                                NganHang = dangKyViewModel.KhachHang.NganHang,
                                SoDienThoai = dangKyViewModel.KhachHang.SoDienThoai,
                            }
                        };
                        Dbcontext.TaiKhoan.Add(newUser);
                        Dbcontext.SaveChanges();
                        return View("Index");
                    }
                }
            }
            List<string> listNH = new List<string>()
            {
                "TPBank",
                "VietcomBank",
                "ViettinBank",
                "SeABank",
                "VPBank",
                "SacomBank",
                "TechcomBank",
                "BIDV",
                "DongABank"
            };

            SelectList selectList = new SelectList(listNH);
            ViewBag.listNH = selectList;

            return View();
        }
    }
}