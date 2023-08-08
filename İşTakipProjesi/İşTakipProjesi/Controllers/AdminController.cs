using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using İşTakipProjesi.Models.Baglantisql;
using OfficeOpenXml;


namespace İşTakipProjesi.Controllers
{
    public class DashboardViewModel
    {
        public List<GrafikVeri> GrafikVerileri { get; set; }
        public int ToplamIsSayisi { get; set; }
        public int DevamEden { get; set; }
        public int Tamamlanan { get; set; }
        public int İptal { get; set; }
    }
    public class GrafikVeri
    {
        public string Label { get; set; }
        public int Value { get; set; }
    }
    public class AdminController : Controller
    {
        // GET: Admin
        İşTakipDbEntities db = new İşTakipDbEntities();
        public ActionResult AdminDashboard(string personelAdi = "")
        {
            var veriler = db.İsTakip.ToList();

            var model = new DashboardViewModel();
            model.GrafikVerileri = new List<GrafikVeri>();

            if (!string.IsNullOrEmpty(personelAdi))
            {
                var tamamlandiSayisi = veriler.Count(x => x.Durum == "Tamamlandı" && x.SorumluPersonel == personelAdi);
                var devamEdiyorSayisi = veriler.Count(x => x.Durum == "Devam Ediyor" && x.SorumluPersonel == personelAdi);
                var iptalSayisi = veriler.Count(x => x.Durum == "İptal" && x.SorumluPersonel == personelAdi);

                model.GrafikVerileri.Add(new GrafikVeri { Label = "Tamamlandı", Value = tamamlandiSayisi });
                model.GrafikVerileri.Add(new GrafikVeri { Label = "Devam Ediyor", Value = devamEdiyorSayisi });
                model.GrafikVerileri.Add(new GrafikVeri { Label = "İptal", Value = iptalSayisi });

                model.ToplamIsSayisi = veriler.Count(x => x.SorumluPersonel == personelAdi);
                model.DevamEden = devamEdiyorSayisi;
                model.Tamamlanan = tamamlandiSayisi;
                model.İptal = iptalSayisi;
            }
            else
            {
                model.ToplamIsSayisi = veriler.Count;
                model.DevamEden = veriler.Count(x => x.Durum == "Devam Ediyor");
                model.Tamamlanan = veriler.Count(x => x.Durum == "Tamamlandı");
                model.İptal = veriler.Count(x => x.Durum == "İptal");

                model.GrafikVerileri.Add(new GrafikVeri { Label = "Tamamlandı", Value = model.Tamamlanan });
                model.GrafikVerileri.Add(new GrafikVeri { Label = "Devam Ediyor", Value = model.DevamEden });
                model.GrafikVerileri.Add(new GrafikVeri { Label = "İptal", Value = model.İptal });
            }

            ViewBag.PersonelListesi = db.Personel.ToList();
            return View(model);
        }
        #region Personel İşlemleri
        public ActionResult PersonelListesi()
        {
            var veriler = db.Personel.ToList();
            return View(veriler);
        }
        [HttpGet]
        public ActionResult PersonelEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PersonelEkle(Personel y)
        {
            if (ModelState.IsValid)
            {
                y.AktifMi = "aktif";
                db.Personel.Add(y);
                db.SaveChanges();
                Response.Write("<script lang='JavaScript'>alert('Personel Eklenmiştir.');</script>");
                return RedirectToAction("PersonelListesi", "Admin");
            }
            return View(y);
        }
        public ActionResult PersonelSıl(int id)
        {
            var personelid = db.Personel.Find(id);
            personelid.AktifMi = "pasif";
            db.SaveChanges();
            return RedirectToAction("PersonelListesi");
        }
        public ActionResult PersonelBilgiGetir(int id)
        {
            var doldur = db.Personel.Find(id);
            return View("PersonelBilgiGetir", doldur);

        }
        public ActionResult PersonelGüncelle(Personel p1)
        {
            var doldur = db.Personel.Find(p1.ID);
            doldur.AdSoyad = p1.AdSoyad;
            doldur.Eposta = p1.Eposta;
            doldur.KullaniciAdi = p1.KullaniciAdi;
            doldur.Sifre = p1.Sifre;
            doldur.AktifMi = p1.AktifMi;
            db.SaveChanges();
            return RedirectToAction("PersonelListesi");
        }
        #endregion

        #region İş İşlemleri
        public ActionResult TümIsListesi()
        {
            var veriler = db.İsTakip.ToList();
            return View(veriler);
        }
        public ActionResult ExceleAktar()
        {
            var veriler = db.İsTakip.ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("İş Listesi");

                worksheet.Cells[1, 1].Value = "İŞ ID";
                worksheet.Cells[1, 3].Value = "SORUMLU PERSONEL";
                worksheet.Cells[1, 4].Value = "TALEP EDEN";
                worksheet.Cells[1, 5].Value = "KONU";
                worksheet.Cells[1, 6].Value = "DEPARTMAN";
                worksheet.Cells[1, 7].Value = "MODÜL";
                worksheet.Cells[1, 8].Value = "TARİH";
                worksheet.Cells[1, 9].Value = "DURUM";

                for (int row = 2; row <= veriler.Count + 1; row++)
                {
                    var rowData = veriler[row - 2];
                    worksheet.Cells[row, 1].Value = rowData.ID;
                    worksheet.Cells[row, 3].Value = rowData.SorumluPersonel;
                    worksheet.Cells[row, 4].Value = rowData.TalepEden;
                    worksheet.Cells[row, 5].Value = rowData.Konu;
                    worksheet.Cells[row, 6].Value = rowData.Departman;
                    worksheet.Cells[row, 7].Value = rowData.Modul;
                    worksheet.Cells[row, 8].Value = rowData.Tarih;
                    worksheet.Cells[row, 9].Value = rowData.Durum;
                }

                worksheet.Cells.AutoFitColumns();

                var excelBytes = package.GetAsByteArray();

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "IsListesi.xlsx");
            }
        }

        public ActionResult DevamEdenIsListesi()
        {
            var veriler = db.İsTakip.Where(x => x.Durum == "Devam Ediyor").ToList();
            return View(veriler);
        }
        public ActionResult TamamlananIsListesi()
        {
            var veriler = db.İsTakip.Where(x => x.Durum == "Tamamlandı").ToList();
            return View(veriler);
        }
        public ActionResult IptalEdilenIsListesi()
        {
            var veriler = db.İsTakip.Where(x => x.Durum == "İptal").ToList();
            return View(veriler);
        }
        [HttpGet]
        public ActionResult YeniIsKaydı()
        {
            ViewBag.PersonelListesi = db.Personel.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult YeniIsKaydı(İsTakip y, string[] Modul, HttpPostedFileBase Dosya)
        {
            if (Dosya != null && Dosya.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Dosya.FileName);
                var filePath = Server.MapPath("~/Uploads/") + fileName;
                Dosya.SaveAs(filePath);
                y.Dosya = fileName;
            }
            else
            {
                y.Dosya = "";
            }

            if (ModelState.IsValid)
            {
                y.Tarih = DateTime.Now.ToString();

                if (Modul != null)
                {
                    y.Modul = "";
                    foreach (var modul in Modul)
                    {
                        y.Modul += modul + "  ";
                    }
                }

                db.İsTakip.Add(y);
                db.SaveChanges();
                ViewBag.FilePath = "/Uploads/" + y.Dosya;
                return RedirectToAction("TümIsListesi", "Admin");
            }

            ViewBag.PersonelListesi = db.Personel.ToList();
            return View(y);
        }


        #endregion
        public ActionResult MesajListesi()
        {
            var admin = Session["adsoyad"].ToString();
            var veriler = db.İletisim.Where(x => x.GönderilecekKisi == admin).ToList();
            return View(veriler);
        }
        [HttpGet]
        public ActionResult MesajCevapla(int id)
        {
            var originalMessage = db.İletisim.Find(id);
            var model = new İletisim
            {
                ID = 0, // Set ID to 0 for a new message
                GönderenKisi = Session["adsoyad"].ToString(),
                GönderilecekKisi = originalMessage.GönderenKisi,
                Konu = "Cevap: " + originalMessage.Konu,
                Mesaj = string.Empty
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult MesajCevapla(İletisim y)
        {
            if (ModelState.IsValid)
            {
                db.İletisim.Add(y);
                db.SaveChanges();
                ViewBag.Message = "Mesaj Başarıyla Gönderilmiştir.";
                return RedirectToAction("MesajListesi");
            }

            return View(y);
        }
        #region Ortak Alan İşlemleri
        public ActionResult OrtakAlanListele()
        {
            var veriler = db.OrtakAlan.ToList();
            return View(veriler);
        }
        [HttpGet]
        public ActionResult OrtakAlanEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult OrtakAlanEkle(OrtakAlan y, HttpPostedFileBase Dosya)
        {
            if (Dosya != null && Dosya.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Dosya.FileName);
                var filePath = Server.MapPath("~/Uploads/") + fileName;
                Dosya.SaveAs(filePath);
                y.Dosya = fileName;
                ViewBag.FilePath = "/Uploads/" + fileName;
                db.OrtakAlan.Add(y);
                db.SaveChanges();
            }

            return RedirectToAction("OrtakAlanListele", "Admin");
        }
        #endregion
    }
}
