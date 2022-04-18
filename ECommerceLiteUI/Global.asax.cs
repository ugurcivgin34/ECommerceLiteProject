﻿using ECommerceLiteBLL.Account;
using ECommerceLiteBLL.Repository;
using ECommerceLiteEntity.Enums;
using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ECommerceLiteUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            #region CreateRoles_RolleriOlustur
            //NOT: Application_Start :
            //Uygulama ilk kez çalıştırıldığında bir defaya mahsus olmak üzere çalışır.
            //Bu nedenle ben uyg. ilk kez çalıştığında DB'de Roller ekli mi diye bakmak istiyorum.
            //Ekli değilse rolleri Enum'dan çağırıp ekleyelim
            //Ekli ise bişey yapmaya gerek kalmıyor.

            //adım 1: Rollere bakacağım şey --> Role Manager
            var myRoleManager = MembershipTools.NewRoleManager();
            //adım 2: Rollerin isimlerini almak (ipucu --> Enum)
            var allRoles = Enum.GetNames(typeof(Roles));
            //adım 3: Bize gelen diziyi tek tek tek döneceğiz (döngü)

            foreach (var item in allRoles)
            {
                //adım 4: Acaba bu rol DB'de ekli mi? 
                if (!myRoleManager.RoleExists(item)) // Eğer bu role ekli değilse?
                {
                    //Adım 5: Rolü ekle!
                    //1.yol
                    ApplicationRole role = new ApplicationRole()
                    {
                        Name = item,
                        IsDeleted = false
                    };
                    myRoleManager.Create(role);
                    //2.yol
                    //myRoleManager.Create(new ApplicationRole()
                    //{
                    //    Name = item
                    //});


                }
            }
            #endregion



            #region CreateDefaultAdmin_SistemAdminiOlustur

            //NOT: Proje ilk ayağa kalkığında arka planda default admin kullanıcısı ekeleylim
            //NOT: Kendi isminizle admin olarak kayıt olmanız için Admin register sayfası zaman kısıtlılığından yapamadık. Geniş bir zamanda eklenebilir.
            var myUserManager = MembershipTools.NewUserManager();
            var allUsers = myUserManager.Users;
            AdminRepo myAdminRepo = new AdminRepo();
            if (myAdminRepo.GetAll().Count == 0) // Hiç admin yoksa ekleyelim 
            {
                ApplicationUser adminUser = new ApplicationUser()
                {
                    Name = "303",
                    Surname = "Admin",
                    RegisterDate = DateTime.Now,
                    Email = "nayazilim303@gmail.com",
                    UserName = "nayazilim303@gmail.com",
                    IsDeleted = false,
                    EmailConfirmed = true
                };

                var createResult = myUserManager.Create(adminUser, "admin12345");
                if (createResult.Succeeded)
                {
                    myUserManager.AddToRole(adminUser.Id, Roles.Admin.ToString());
                    Admin admin = new Admin()
                    {
                        UserId = adminUser.Id,
                        TCNumber = "00000000000",
                        IsDeleted = false,
                        LastActiveTime = DateTime.Now
                    };
                    myAdminRepo.Insert(admin);
                }
            }

            #endregion








        }
    }
}

