using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using İşTakipProjesi.Models.Baglantisql;

namespace İşTakipProjesi.Controllers
{
    public class GirisController : Controller
    {
        İşTakipDbEntities db = new İşTakipDbEntities();
        [HttpGet]
        public ActionResult AdminGiris()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AdminGiris(FormCollection form)
        {
            string kullaniciadi = form["KullaniciAdi"];
            string sifre= form["Sifre"];

            var veri = db.AdminGiris.FirstOrDefault(m => m.KullaniciAdi == kullaniciadi && m.Sifre== sifre);
            if (veri != null)
            {
                Session["adsoyad"] = veri.AdiSoyadi.ToString();
                Session["kullanıcıadı"] = veri.KullaniciAdi.ToString();
                return RedirectToAction("PersonelListesi", "Admin");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre. Lütfen tekrar deneyin.");
                return View();
            }
        }

        [HttpGet]
        public ActionResult PersonelGiris()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PersonelGiris(FormCollection form)
        {
            string kullaniciadi = form["KullaniciAdi"];
            string sifre = form["Sifre"];

            var veri = db.Personel.FirstOrDefault(m => m.KullaniciAdi == kullaniciadi && m.Sifre == sifre);
            var aktifMi = db.Personel.FirstOrDefault(m => m.KullaniciAdi== kullaniciadi && m.Sifre == sifre && m.AktifMi=="aktif");
            if (veri != null)
            {
                if (aktifMi!=null)
                {
                    FormsAuthentication.SetAuthCookie(veri.Eposta, false);
                    Session["email"] = veri.Eposta.ToString();
                    Session["kullanıcıadı"] = veri.KullaniciAdi.ToString();
                    Session["adsoyad"] = veri.AdSoyad.ToString();
                    return RedirectToAction("PersonelIslerim", "Personel");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Üyeliğiniz Aktif Değildir !");
                    return View();
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre. Lütfen tekrar deneyin.");
                return View();
            }
        }
    }
}