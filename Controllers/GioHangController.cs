using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarShowroom.Models;
using CarShowroom.Session;

namespace CarShowroom.Controllers
{
    public class GioHangController : Controller
    {
        // GET: GioHang
        private const string CartSession = "CartSession";
        private readonly CarShowroomDbContext context = new CarShowroomDbContext();
        public ActionResult Index()
        {
            ViewBag.LoiSoLuong = TempData["LoiSoLuong"] as string;
            ViewBag.ThongBaoTrong = TempData["ThongBaoTrong"] as string;
            var cart = Session[CartSession] as GioHang;
            GioHang listItem = new GioHang();
            if (cart != null && cart.DoTrongGioHangs.Count != 0)
            {

                listItem = cart;
            }
            else
            {
                listItem.DoTrongGioHangs = new List<DoTrongGioHang>();
                listItem.NgayThue = DateTime.Now;
                listItem.NgayTra = DateTime.Now;
                ViewBag.GioHangTrong = "Giỏ hàng hiện tại đang trống, mời quý khách lựa chọn sản phẩm cần thuê!!";
            }

            return View(listItem);
        }
        [HttpPost]
        public ActionResult Tang(int id, int currentQtt = 1, int quantity = 1)
        {

            var xe = context.Xe.FirstOrDefault(m => m.MaXe == id);
            var gioHang = Session[CartSession] as GioHang;
            foreach (var item in gioHang.DoTrongGioHangs)
            {
                if (item.Xe.MaXe == xe.MaXe)
                {
                    if (currentQtt < xe.SoLuongXe)
                        item.Quantity += quantity;
                    else
                    {
                        TempData["LoiSoLuong"] = "<script>alert('Trong kho đang tạm hết hàng, vui lòng chọn sản phẩm khác');</script>";
                        return RedirectToAction("Index");
                    }
                    //TODO
                }
            }
            Session[CartSession] = gioHang;
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Giam(int id, int quantity, int currentQtt)
        {

            var xe = context.Xe.FirstOrDefault(m => m.MaXe == id);
            var gioHang = Session[CartSession] as GioHang;
            foreach (var item in gioHang.DoTrongGioHangs)
            {
                if (item.Xe.MaXe == xe.MaXe)
                {
                    if (currentQtt > 1)
                        item.Quantity -= quantity;
                    else
                    {
                        TempData["LoiSoLuong"] = "<script>alert('Không thể đặt thuê với số lượng là 0');</script>";
                    }
                    //TODO
                }
            }
            Session[CartSession] = gioHang;
            return RedirectToAction("Index");


        }
        [HttpPost]
        public ActionResult Xoa(int id)
        {
            var gioHang = Session[CartSession] as GioHang;
            gioHang.DoTrongGioHangs.RemoveAll(x => x.Xe.MaXe == id);
            Session[CartSession] = gioHang;
            return RedirectToAction("Index");
        }
        public ActionResult Them(int id, int quantity)
        {

            var cart = Session[CartSession] as GioHang;
            var xe = context.Xe.FirstOrDefault(x => x.MaXe == id);
            if (cart != null)
            {
                var listItem = cart.DoTrongGioHangs as List<DoTrongGioHang>;
                if (listItem.Exists(m => m.Xe.MaXe == id))
                {
                    foreach (var item in listItem)
                    {
                        if (item.Xe.MaXe == xe.MaXe)
                        {
                            if (item.Quantity < xe.SoLuongXe)
                                item.Quantity += quantity;
                            else
                            {
                                TempData["LoiSoLuong"] = "<script>alert('Trong kho đang tạm hết hàng, vui lòng chọn sản phẩm khác');</script>";
                                return RedirectToAction("Index");
                            }
                            //TODO
                        }
                    }
                }
                else
                {
                    DoTrongGioHang cItem = new DoTrongGioHang { Xe = xe, Quantity = quantity };
                    listItem.Add(cItem);
                }
                cart.DoTrongGioHangs = listItem;
                Session[CartSession] = cart;
            }
            else
            {
                cart = new GioHang();
                List<DoTrongGioHang> listItem = new List<DoTrongGioHang>();
                DoTrongGioHang cItem = new DoTrongGioHang();
                cItem.Xe = xe;
                cItem.Quantity = quantity;
                listItem.Add(cItem);

                cart.DoTrongGioHangs = listItem;

                Session[CartSession] = cart;

            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ThanhToan(DateTime ngayThue, DateTime ngayTra)
        {
            decimal money = 0;
            ViewBag.ThongTinUser = Session[UserSession.USER_SESSION] as KhachHang;
            if (ViewBag.ThongTinUser == null)
            {
                TempData["ThongBaoDangNhap"] = "<script>alert('Mời Đăng Nhập Để thanh toán');</script>";
                return RedirectToAction("Index", "Login");
            }
            var cart = Session[CartSession] as GioHang;
            if (cart == null)
            {
                TempData["ThongBaoTrong"] = "<script>alert('Bạn Chưa Chọn Sản Phẩm Nên Không Thể Thanh Toán');</script>";
                return RedirectToAction("Index");
            }
            cart.NgayTra = ngayTra;
            cart.NgayThue = ngayThue;
            foreach (var item in cart.DoTrongGioHangs)
            {
                money += item.Xe.GiaChoThue * item.Quantity;
            }
            try
            {
                DateTime ngaymuon = Convert.ToDateTime(cart.NgayThue);
                DateTime ngaytra = Convert.ToDateTime(cart.NgayTra);
                TimeSpan time = ngaytra - ngaymuon;
                int tongSoNgay = time.Days + 1;

                ViewBag.Money = money * tongSoNgay;
            }
            catch { return RedirectToAction("Index"); }
            ViewBag.GioHang = cart;
            return View();
        }
        [HttpPost]
        public ActionResult DatHang()
        {
            decimal totalMoney = 0;
            try
            {
                KhachHang khachHang = Session[UserSession.USER_SESSION] as KhachHang;
                GioHang gioHang = Session[CartSession] as GioHang;
                List<ChiTietHoaDon> listChiTietHoaDons = new List<ChiTietHoaDon>();
                foreach (var item in gioHang.DoTrongGioHangs)
                {
                    ChiTietHoaDon chiTietHoaDon = new ChiTietHoaDon()
                    {
                        SoLuong = item.Quantity,
                        ThanhTien = item.Xe.GiaNhap * item.Quantity,
                        MaXe = item.Xe.MaXe
                    };
                    listChiTietHoaDons.Add(chiTietHoaDon);
                    totalMoney += chiTietHoaDon.ThanhTien;
                }
                HoaDon hoaDon = new HoaDon()
                {
                    MaKhachHang = khachHang.MaKhachHang,
                    NgayNhan = gioHang.NgayThue.Value,
                    NgayTra = gioHang.NgayTra.Value,
                    HinhThucThanhToan = Models.Enum.HinhThucThanhToan.ThanhToanTrucTiep,
                    TongThanhTien = totalMoney,
                    ChiTietHoaDon = listChiTietHoaDons
                };

                context.HoaDon.Add(hoaDon);
                context.SaveChanges();
                Session[CartSession] = null;
            }
            catch
            {
                return RedirectToAction("ThanhToan");
            }
            return RedirectToAction("Index", "Home");
        }
    }

}