﻿using ECommerceLiteBLL.Account;
using ECommerceLiteBLL.Repository;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.Enums;
using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using ECommerceLiteEntity.ViewModels;
using ECommerceLiteUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class AccountController : BaseController
    {
        //Global alan
        //Not: Bir sonraki projede repoları UI'ın içinde NEW'lemeyeceğiz!
        //Çünkü bu bağımlılık oluşturur! Bir sonraki projede bağımlılıkları tersine çevirme işlemi olarak bilinen Dependecny Injection işlemleri
        //yapacağız.

        CustomerRepo myCustomerRepo = new CustomerRepo();
        PassiveUserRepo myPassiveUserRepo = new PassiveUserRepo();
        UserManager<ApplicationUser> myUserManager = MembershipTools.NewUserManager();
        UserStore<ApplicationUser> myUserStore = MembershipTools.NewUserStore();
        RoleManager<ApplicationRole> myRoleManager = MembershipTools.NewRoleManager();

        [HttpGet]
        public ActionResult Register()
        {
            //KAyıt ol sayfası
            return View();
        }
        /// <summary>
        /// Kullanıcı kayıt işlemini gerçekleştiren metod.
        /// </summary>
        /// <param name="model">Oluşturduğumuz model classının propertlerini kullandık</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken] //Güvenliği sağlama kamacıyla bot hesaplardan bu metoda erişlemzsin diye kullandık

        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) // Model validasyonları sağladı mı
                {
                    return View(model);
                }
                var checkUserTC = myUserStore.Context.Set<Customer>()
                    .FirstOrDefault(x => x.TCNumber == model.TCNumber)?.TCNumber;
                if (checkUserTC != null) // Buldu 
                {
                    ModelState.AddModelError("", "Bu TC numarası ile daha önceden sisteme kayıt yapılmıştır!");
                    return View(model);
                }
                //To Do : Soru işaretli silinip debuglanacak
                var checkUserEmail = myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.Email == model.Email)?.Email;
                if (checkUserEmail != null)
                {
                    ModelState.AddModelError("", "Bu email ile daha önceden sisteme kayıt yapılmıştır!");
                    return View(model);
                }
                //aktivasyon kodu üretelim
                var activationCode = Guid.NewGuid().ToString().Replace("-", "");

                //Artık sisteme kayıt olabilir...
                var newUser = new ApplicationUser()
                {
                    Name = model.Name,
                    Surname = model.Surname,
                    Email = model.Email,
                    UserName = model.TCNumber,
                    ActivationCode = activationCode
                };



                //Artık ekleyelim
                var createResult = myUserManager.CreateAsync(newUser, model.Password);

                //To Do: createResult.Isfault ne acaba ? debuglarken bakalım

                if (createResult.Result.Succeeded)
                {
                    //göre başarıyla tamamlandıysa kişi aspnetusers tablosunna eklenmiştir
                    //Yeni kayıt olduğu için bu kişiye pasif rol verilecektir
                    //Kişi emailiine gelen aktivasyon koduna tıklarsaa pasifledikten çıkıp  customer olabilir.

                    await myUserManager.AddToRoleAsync(newUser.Id, Roles.Passive.ToString());
                    PassiveUser myPassiveUser = new PassiveUser()
                    {
                        UserId = newUser.Id,
                        TCNumber = model.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };

                    //myPassiveUsrRepo.Insert(myPassiveUser);
                    await myPassiveUserRepo.InsertAsync(myPassiveUser);

                    //email gönderilecek
                    // site adresini alıyoruz.
                    var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter
                        + Request.Url.Host +
                        (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                    await SiteSettings.SendMail(new MailModel()
                    {
                        To = newUser.Email,
                        Subject = "ECommerceLite Site Aktivasyon Emaili",
                        Message = $"Merhaba {newUser.Name} {newUser.Surname}," +
                        $"<br/>Hesabınızı aktifleştirmek için <b>" +
                        $"<a href='{siteURL}/Account/Activation?" +
                        $"code={activationCode}'>Aktivasyon Linkine</a></b> tıklayınız..."
                    });
                    // işlemler bitti...
                    return RedirectToAction("Login", "Account", new { email = $"{newUser.Email}" });


                }
                else
                {
                    ModelState.AddModelError("", "Kayıt işleminde beklenmedik bir hata oluştu");
                    return View(model);
                }

            }
            catch (Exception ex)
            {
                //To Do:Loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştur! Tekrar deneyiniz");
                return View(model);

            }
        }


        [HttpGet]
        public async Task<ActionResult> Activation(string code)
        {
            try
            {
                //select * from AspNetUsers where Activationcode='şlasdknawdşasdnwak'
                var user =
                    myUserStore.Context.Set<ApplicationUser>()
                    .FirstOrDefault(x => x.ActivationCode == code);

                if (user == null)
                {
                    ViewBag.ActivationResult = "Aktivasyon işlemi başarısız! Sistem yöneticisinden yeniden email isteyiniz..";
                    return View();
                }
                //user bulundu

                if (user.EmailConfirmed) //zaten aktifleşmiş mi?
                {
                    ViewBag.ActivationResult = "Aktivasyon işleminiz zaten gerçekleşmiştir! Giriş yaparak sistemi kullanabilirsiniz...";
                    return View();
                }
                user.EmailConfirmed = true;
                await myUserStore.UpdateAsync(user);
                await myUserStore.Context.SaveChangesAsync();
                //Bu kişi artık aktif!

                PassiveUser passiveUser = myPassiveUserRepo
                    .AsQueryable().FirstOrDefault(x => x.UserId == user.Id);
                if (passiveUser !=null)
                {
                    //TODO: PassiveUser tablosuna TargetRole ekleme işlemini daha sonra yapalım.Kafalarındaki soru işareti gittikten sonra...
                    passiveUser.IsDeleted = true;
                    myPassiveUserRepo.Update();
                    Customer customer = new Customer()
                    {
                        UserId = user.Id,
                        TCNumber = passiveUser.TCNumber,
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };

                    await myCustomerRepo.InsertAsync(customer); //Asenkron mekanizmayı hazırla

                    //Aspnetuserrole tablosuna bu kişinin artık customer mertebesine uaştığını bildirelim.
                    myUserManager.RemoveFromRole(user.Id, Roles.Passive.ToString()); //id sini verdiğimiz passive rolü sil
                    myUserManager.AddToRole(user.Id, Roles.Customer.ToString());
                    //işlem bitti başarılı olduğuna dair mesajı gönderelim

                    ViewBag.ActivationResult = $"Merhaba Sayın {user.Name} {user.Surname},aktileştirme işleminiz başarılıdır! Giriş yapıp sistemi kullanabilirsiniz";
                    return View();
                   
                }

            }
            catch (Exception ex)
            {
                //ToDo: Loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştur!");
                return View();

            }

        }



    }
}