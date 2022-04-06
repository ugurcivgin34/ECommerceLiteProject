using ECommerceLiteBLL.Account;
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
                if (passiveUser != null)
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

                //NOT:Müsait olduğunuzda bir beyin fırtınası yapabilirsiniz.
                //Kendinize şu soruyu sorun! PassiveUser null gelirse nasıl bir yol izlenilebilir.??
                //PassiveUser null gelmesi çok büyük bir problem mi?
                //Customerda bu kişi kayıtlı mı? Customerda bir problem yok.CUstomer kayıtlı değilse PROBLEM VAR!
                //Buraya yazılması gereken mini mimmacık kodları şmdilik size bırakma gibi yapıyorum sonra birlikte tekrar bakacağız


                return View();
            }
            catch (Exception ex)
            {
                //ToDo: Loglama yapılacak
                ModelState.AddModelError("", "Beklenmedik bir hata oluştur!");
                return View();

            }

        }

        [HttpGet]
        [Authorize] //Kişinin login olması şart.Şart olmasını getirmek istiyorsak bu anatosyonu kulanmamız gerek.Yetkili olacak mı bu sayfaya girecek olan kişi
        public ActionResult UserProfile()
        {
            //login olmuş kişinin id bilgisini alalım
            var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());//İçeriye giren kişinin bilgilerini tutuyor.Session 
            if (user != null)
            {
                //kişiyi bulacağız ve mevcutr bilgilerini ProfileVİewModele atayıp sayfaya göndereceğiz.

                ProfileViewModel model = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    TCNumber = user.UserName
                };
                return View(model);
            }
            //User null ise( temkinli davrandık...)
            ModelState.AddModelError("", "Beklenmedik bir sorun oluşmuş olabilir mi? Giriş yapıp,tekrar deneyiniz. Sizinle tekrar buluşalım!");
            return View();


        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserProfile(ProfileViewModel model)
        {
            try
            {
                //Sisteme kayıt olmuş ve login ile giriş yapmış kişi Hesabıma tıkladı,bilgilerini gördü.Bilgilerinde değişiklik yaptı.
                //Biz burada kontrol edeceğiz.Yapılan değişiklikleri tespit edip DB'mizi güncelleyeceğiz.
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
                if (user == null)
                {
                    ModelState.AddModelError("", "Mevcut kullanıcı bilgilerinize ulaşılamadığı için işlem yapamıyoruz");
                    return View(model);
                }

                //Bir user herhangi bir bilgisini değiştirecekse PAROLASINI girmek zorunda
                //Bu nedenle model ile gelen parola DB'deki parola ile eşleşiyor mu diye bakmak lazım...
                if (myUserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError("", "Mevcut şirenizi yanlış girdiğiniz için bilgilerinizi güncellemeyedik! Lütfen tekrar deneyinz");
                    return View(model);
                }
                //Başarılıysa yani parolayı doğru yazdı!
                //Bilgilerini güncelleyeceğiz

                user.Name = model.Name;
                user.Surname = model.Surname;
                await myUserManager.UpdateAsync(user);
                ViewBag.Result = "Bilgileriniz güncellendi";
                var updateModel = new ProfileViewModel()
                {
                    Name = user.Name,
                    Surname = user.Surname,
                    TCNumber = user.UserName,
                    Email = user.Email
                };
                return View(updateModel);

            }
            catch (Exception ex)
            {

                //ex loglanacak
                ModelState.AddModelError("", "Beklenmedik hata oluştur! Tekrar deneyeiniz");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdatePassword()
        {
            var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());
            if (user != null)
            {
                ProfileViewModel model = new ProfileViewModel()
                {
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                    TCNumber = user.UserName
                };
                return View(model);
            }
            ModelState.AddModelError("", "Sistem giriş yapmanız gerekmektedir");
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePassword(ProfileViewModel model)
        {
            try
            {//Mevcuttan login olmuş kişinin ID'sini veriyor.O id ile manager kişiyi db'den bulup getiriyor.
                var user = myUserManager.FindById(HttpContext.User.Identity.GetUserId());

                //Ya şifreler aynısıydı??
                if (myUserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, model.NewPassword) == PasswordVerificationResult.Success)
                {
                    //Bu kişi mevcut şifresinin aynısını yeni şifre olarak yutturmaya çalışıyor
                    ModelState.AddModelError("", "Yeni şifreniz mevcut şifrenizle aynı olmasın madem değiştirmek istedin!!");
                    return View(model);
                }

                //Yeni şifre ile şifre tekrarı uyuşuyor mu?
                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Şifreler uyuşmuyor");
                    return View(model);
                }


                //Acaba mevcut şifresini doğru yazdı mı ?
                var checkCurrent = myUserManager.Find(user.UserName, model.Password);
                if (checkCurrent == null)
                {
                    //Mevcut şifresiniz yanlış yazmış!
                    ModelState.AddModelError("", "Mevcut şifrenizi yanlış girdiğiniz için yeni şifre oluşturme işleminiz başarısız oldu! Tekrar deneyiniz");
                    return View(model);
                }
                //Artık şifresini değiştirebilir.
                await myUserStore.SetPasswordHashAsync(user,myUserManager.PasswordHasher.HashPassword(model.NewPassword));

                await myUserManager.UpdateAsync(user);
                //Şifre değiştirdikten sonra sistemden atalım!
                TempData["PasswordUpdated"] = "Parolanız değiştirildi";
                HttpContext.GetOwinContext().Authentication.SignOut();
                return RedirectToAction("Login", "Account", new { email = user.Email });
            }
            catch (Exception ex)
            {
                //ex loglanacak
                ModelState.AddModelError("", "Beklenmedik bir hata oldu! Tekrar deneyiniz");
                return View(model);

            }
        }

        [HttpGet]
        public ActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RecoverPassword(ProfileViewModel model)
        {
            try
            {
                //Şifresini unutmuş.
                //1.yöntem
                //var user = myUserStore.Context.Set<ApplicationUser>().FirstOrDefault(x => x.Email == model.Email);

                //2.yöntem
                var user = myUserManager.FindByEmail(model.Email);
                if (user==null)
                {
                    ViewBag.RecorverPassword = "Sistemde kayıtlı böyle bir kullanıcı olmadığı için size yeni şifre gönderemiyoruz! Lütfen önce kayıt olun";
                    return View(model);
                }
                //Random şifre oluştur!
                var randomPassword = CreateRandomNewPassword();
                await myUserStore.SetPasswordHashAsync(user, myUserManager.PasswordHasher.HashPassword(randomPassword));
                await myUserStore.UpdateAsync(user);
                //email gönderilecek
                // site adresini alıyoruz.
                var siteURL = Request.Url.Scheme + Uri.SchemeDelimiter
                    + Request.Url.Host +
                    (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
                await SiteSettings.SendMail(new MailModel()
                {
                    To = user.Email,
                    Subject = "ECommerceLite - Şifre Yenilendi!",
                    Message = $"Merhaba {user.Name} {user.Surname}," +
                    $"<br/>Yeni şifreniz:<b>{randomPassword}</b>Sisteme Giriş Yapmak İçin <b>" +
                    $"<a href='{siteURL}/Account/Login?" +
                    $"email={user.Email}'>BURAYA</a></b> tıklayınız..."
                });
                // işlemler bitti...
                ViewBag.RecoverPassword = "Email adresinize şifre gönderilmiştir.";
                return View();

            }
            catch (Exception ex)
            {
                //Todo ex loglanacak
                ViewBag.RecoverPasswordResult = "Sistemsel bir hata oluştu! Tekrar deneyiniz!";
                return View(model);
            }
        }

    }
}