using ECommerceLiteBLL.Repository;
using ECommerceLiteUI.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ECommerceLiteUI.Controllers
{
    public class HomeController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        public ActionResult Index()
        {
            //Ana kategorileriden 4 tanesini viewbag ile sayfaya gönderelim
            var categoryList = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId == null).Take(4).ToList();

            ViewBag.CategoryList = categoryList.OrderByDescending(x => x.Id);

            //Ürünler
            var productList = myProductRepo.AsQueryable().Where(x => x.IsDeleted == false && x.Quantity >= 1).Take(10).ToList();
            List<ProductViewModel> model = new List<ProductViewModel>();
            foreach (var item in productList)
            {
                //mapster : S
                //model.Add(item.Adapt<ProductViewModel>());
                var product = new ProductViewModel()
                {
                    Id = item.Id,
                    CategoryId = item.CategoryId,
                    ProductName = item.Description,
                    Quantity = item.Quantity,
                    Discount = item.Discount,
                    RegisterDate = item.RegisterDate,
                    Price = item.Price,
                    ProductCode = item.ProductCode
                    //isDeleted alanını viewmodelin içine eklemeyi unuttuk.Çünkü isDeleted alanını daha dün ekledik . Viewmodeli geçen hafta oluşturduk.
                };
                product.GetCategory();
                product.GetProductPictures();
                model.Add(product);
            }

            return View(model);
        }



        

        public ActionResult About()
        {
            return View();
        }
    }
}