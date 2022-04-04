using ECommerceLiteEntity.IdentityModels;
using ECommerceLiteEntity.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteDAL
{
    public class MyContext:IdentityDbContext<ApplicationUser> //microsoftun idendity tablolarını istyoruz ,IdentityDbCOntext şeklinde yaptık.Extra olarak ApplicationUser ın iindeki propertleri de almak için genişletmiş olduk.Aldık yani
    {
        //Bİr classı inşaa eden metod
        public MyContext() : base("MyCon")  //22.satırdaki kodu kullanıyor, miras aldığı class ın yani
        {

        }


        //Tabloları oluşturalım
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<PassiveUser> PassiveUsers { get; set; }
        public virtual DbSet<Category>  Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<ProductPicture> ProductPictures { get; set; }
    }
}
