using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using İşTakipProjesi.Models.Baglantisql;

namespace İşTakipProjesi.Controllers
{
    public class PersonelController : Controller
    {
        // GET: Personel
        İşTakipDbEntities db = new İşTakipDbEntities();
        public ActionResult PersonelDashboard()
        {
            var personelAdi = Session["adsoyad"].ToString();
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
            return View(model);
        }
        public ActionResult PersonelIslerim()
        {
            var sorumluPersonel = Session["adsoyad"].ToString();
            var veriler = db.İsTakip.Where(x => x.SorumluPersonel == sorumluPersonel).ToList();

            if (veriler.Count == 0)
            {
                ViewBag.Message = "İş bulunamadı.";
            }

            return View(veriler);
        }
        public ActionResult DevamEdenIsListesi()
        {
            var sorumluPersonel = Session["adsoyad"].ToString();
            var veriler = db.İsTakip.Where(x => x.Durum == "Devam Ediyor" && x.SorumluPersonel == sorumluPersonel).ToList();

            if (veriler.Count == 0)
            {
                ViewBag.Message = "Devam eden iş bulunamadı.";
            }

            return View(veriler);
        }

        public ActionResult TamamlananIsListesi()
        {
            var sorumluPersonel = Session["adsoyad"].ToString();
            var veriler = db.İsTakip.Where(x => x.Durum == "Tamamlandı" && x.SorumluPersonel == sorumluPersonel).ToList();

            if (veriler.Count == 0)
            {
                ViewBag.Message = "Tamamlanan iş bulunamadı.";
            }

            return View(veriler);
        }

        public ActionResult IptalEdilenIsListesi()
        {
            var sorumluPersonel = Session["adsoyad"].ToString();
            var veriler = db.İsTakip.Where(x => x.Durum == "İptal" && x.SorumluPersonel == sorumluPersonel).ToList();

            if (veriler.Count == 0)
            {
                ViewBag.Message = "İptal edilen iş bulunamadı.";
            }

            return View(veriler);
        }

        [HttpGet]
        public ActionResult YeniIsKaydı()
        {
            ViewBag.TalepEdenListesi = new SelectList(db.AdminGiris, "AdiSoyadi", "AdiSoyadi");
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
                return RedirectToAction("PersonelIslerim", "Personel");
            }

            ViewBag.PersonelListesi = db.Personel.ToList();
            return View(y);
        }

        public ActionResult İşİptali(int id)
        {
            var x = db.İsTakip.Find(id);
            x.Durum = "İptal";
            db.SaveChanges();
            return RedirectToAction("Personelİşlerim");
        }
        public ActionResult IsBilgiGetir(int id)
        {
            var doldur = db.İsTakip.Find(id);
            return View("IsBilgiGetir", doldur);

        }
        [HttpPost]
        public ActionResult İşKaydıGüncelle(İsTakip p1)
        {
            var doldur = db.İsTakip.Find(p1.ID);
            doldur.SorumluPersonel = p1.SorumluPersonel;
            doldur.TalepEden = p1.TalepEden;
            doldur.Konu = p1.Konu;
            doldur.Departman = p1.Departman;
            doldur.Modul = p1.Modul;
            doldur.Tarih = p1.Tarih;
            doldur.Durum = p1.Durum;
            db.SaveChanges();
            return RedirectToAction("PersonelIslerim");
        }
        public ActionResult MesajListesi()
        {
            var personel = Session["adsoyad"].ToString();
            var veriler = db.İletisim.Where(x => x.GönderilecekKisi == personel).ToList();
            return View(veriler);
        }
        [HttpGet]
        public ActionResult MesajGönder()
        {
            ViewBag.GönderilecekKişi = db.AdminGiris.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult MesajGönder(İletisim y)
        {
            if (ModelState.IsValid)
            {
                db.İletisim.Add(y);
                db.SaveChanges();
                ViewBag.Message = "Mesaj Başarıyla Gönderilmiştir.";
            }
            ViewBag.GönderilecekKişi = db.AdminGiris.ToList();
            return View(y);
        }
        public ActionResult ProfilBilgiGetir()
        {
            // Oturumdan kullaniciadi değerini al
            string adSoyad = Session["adsoyad"] as string;

            if (!string.IsNullOrEmpty(adSoyad))
            {
                // Kullanıcı adına göre profil bilgilerini bul
                var profilBilgisi = db.Personel.FirstOrDefault(p => p.AdSoyad == adSoyad);

                if (profilBilgisi != null)
                {
                    return View(profilBilgisi);
                }
            }
            return RedirectToAction("ProfilBilgiGetir", "Personel");
        }
        public ActionResult ProfilGüncelle(Personel p1)
        {
            var doldur = db.Personel.Find(p1.ID);
            doldur.AdSoyad = p1.AdSoyad;
            doldur.Eposta = p1.Eposta;
            doldur.KullaniciAdi = p1.KullaniciAdi;
            doldur.Sifre = p1.Sifre;
            db.SaveChanges();
            ViewBag.MessageGüncelle = "Profil Bilgileri Başarıyla Güncellenmiştir.";
            return RedirectToAction("ProfilBilgiGetir");
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

            return RedirectToAction("OrtakAlanListele", "Personel");
        }
        #endregion
    }
}