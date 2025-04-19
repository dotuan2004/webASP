using LTW.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LTW.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class SanPhamsController : Controller
    {
        MyDataDataContext data = new MyDataDataContext();
        // GET: SanPham

        public List<SanPham> SearchByName(string searchString)
        {
            var all_sp = (from ss in data.SanPhams
                          where ss.SoLuongTon > 0 && ss.TenSP.Contains(searchString)
                          orderby ss.MaSP descending
                          select ss).ToList();
            return all_sp;
        }
        public ActionResult ListSanPham(string searchString)
        {
            ViewBag.Keyword = searchString;

            if (searchString != null)
            {
                var searchResult = SearchByName(searchString).OrderByDescending(s => s.MaSP);
                return View(searchResult);
            }

            var all_sanpham = data.SanPhams.OrderByDescending(s => s.MaSP);
            return View(all_sanpham);
        }

        public ActionResult Create()
        {
            // Lấy danh sách loại sản phẩm
            var loaiList = data.Loais.ToList();
            ViewBag.MaLoai = new SelectList(loaiList, "MaLoai", "TenLoai");

            // Lấy danh sách nhà cung cấp 
            var nccList = data.NCCs.ToList();
            ViewBag.MaNCC = new SelectList(nccList, "MaNCC", "TenNCC");

            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection collection, SanPham sp, HttpPostedFileBase Hinh)
        {
            var E_TenSP = collection["TenSP"];
            var E_Hinh = ProcessUpload(Hinh);
            var E_GiaVon = Convert.ToInt32(collection["GiaVon"]);
            var E_GiaSP = Convert.ToInt32(collection["GiaSP"]);
            var E_SoLuongTon = Convert.ToInt32(collection["SoLuongTon"]);
            var E_MoTa = collection["MoTa"];
            var E_MaLoai = Convert.ToInt32(collection["MaLoai"]);
            var E_NCC = Convert.ToInt32(collection["MaNCC"]);

            if (string.IsNullOrEmpty(E_TenSP))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                sp.TenSP = E_TenSP.ToString();
                sp.Hinh = E_Hinh.ToString();
                sp.GiaVon = E_GiaVon;
                sp.GiaSP = E_GiaSP;
                sp.SoLuongTon = E_SoLuongTon;
                sp.MoTa = E_MoTa.ToString();
                sp.MaLoai = E_MaLoai;
                sp.MaNCC = E_NCC;

                // Xử lý upload hình
                if (Hinh != null)
                {
                    string path = Path.Combine(Server.MapPath("~/Content/images"), Path.GetFileName(Hinh.FileName));
                    Hinh.SaveAs(path);
                    sp.Hinh = "/Content/images/" + Hinh.FileName;
                }

                data.SanPhams.InsertOnSubmit(sp);
                data.SubmitChanges();
                return RedirectToAction("ListSanPham");
            }
            return this.Create();
        }

        public ActionResult Detail(int id)
        {
            var D_sach = data.SanPhams.Where(m => m.MaSP == id).First();
            return View(D_sach);
        }

        public ActionResult Edit(int id)
        {
            var E_sp = data.SanPhams.First(m => m.MaSP == id);
            return View(E_sp);
        }
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {

            var E_SP = data.SanPhams.First(m => m.MaSP == id);
            var E_TenSP = collection["TenSP"];
            //var E_Hinh = "/Content/images/"+collection["fileUpload"];
            var E_Hinh = collection["Hinh"];
            var E_GiaSP = Convert.ToInt32(collection["GiaSP"]);
            var E_SoLuongTon = Convert.ToInt32(collection["SoLuongTon"]);
            var E_MoTa = collection["MoTa"];
            var E_MaLoai = Convert.ToInt32(collection["MaLoai"]);
            E_SP.MaSP = id;
            if (string.IsNullOrEmpty(E_TenSP))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                E_SP.TenSP = E_TenSP;
                E_SP.Hinh = E_Hinh;
                E_SP.GiaSP = E_GiaSP;
                E_SP.SoLuongTon = E_SoLuongTon;
                E_SP.MoTa = E_MoTa;
                E_SP.MaLoai = E_MaLoai;
                UpdateModel(E_SP);
                data.SubmitChanges();
                return RedirectToAction("ListSanPham");

            }
            return this.Edit(id);

        }

        public string ProcessUpload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return "";
            }
            file.SaveAs(Server.MapPath("~/Content/images/" + file.FileName));
            return "/Content/images/" + file.FileName;
        }


        public ActionResult Delete(int id)
        {
            var D_sp = data.SanPhams.First(m => m.MaSP == id);
            return View(D_sp);
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var D_sp = data.SanPhams.Where(m => m.MaSP == id).FirstOrDefault();
            data.SanPhams.DeleteOnSubmit(D_sp);
            data.SubmitChanges();
            return RedirectToAction("ListSanPham");
        }

    }
}