using ECommerceLiteBLL.Account;
using ECommerceLiteEntity.Enums;
using ECommerceLiteEntity.IdentityModels;
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

            //NOT : Application_Start :
            //Uygulama ilk kez çalıştığında bir defaya mahsus olmak üzere çalışır.
            //Bu nedenle ben uyg. İlk kez çalıştığında db'de Roller ekli mi diye bakmak istiyorum.
            //Ekli değilse rolleri Enum'dan çağırıp ekleyelim
            //Ekli ise bir şey yapmaya gerek kalmıyor.

            //Adım 1: Rollere bakacağım --> Role Manager
            var myRoleManager = MembershipTools.NewRoleManager();
            //Adım 2: Rollerin isimlerini almak (ipucu --> Enum)
            var allRoles = Enum.GetNames(typeof(Roles));//İ.eride kaç enum var ise string dizi olarak getirecek bana
            //Adım 3: Bize gelen diziyi tek tek tek döneceğiz (döngü)
            foreach (var item in allRoles)
            {
                //Adım 4: Acaba bu rol DB'de ekli mi?
                if (!myRoleManager.RoleExists(item))//eğer rol yoksa ekli değilse
                {
                    //Adım 5: Rolü ekle!
                    //------------1. YOL-------------
                    //ApplicationRole role = new ApplicationRole()
                    //{
                    //    Name = item
                    //};
                    //myRoleManager.Create(role);
                    //------------2. YOL-------------
                    myRoleManager.Create(new ApplicationRole()
                    {
                        Name = item,
                        IsDeleted=false

                    });
                }
            }

        }
        protected void Application_Error()
        {
            //NOT : İhtiyacım olursa internetten Global.asax'ın metotlarına bakıp kullanabilirim...

            //Örneğin: Application_Error : Uygulama içinde istenmeyen bir hata meydana geldiğinde çalışır.
            //Bu metodu yazarsak o hatayı loglayıpsorunu çözebiliriz. 

            Exception ex = Server.GetLastError();

            //ex loglanacak

        }
    }
}

