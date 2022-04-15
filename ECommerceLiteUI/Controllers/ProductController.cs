using ECommerceLiteBLL.Repository;
using ECommerceLiteBLL.Settings;
using ECommerceLiteEntity.Models;
using ECommerceLiteUI.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;




namespace ECommerceLiteUI.Controllers
{
    public class ProductController : Controller
    {
        CategoryRepo myCategoryRepo = new CategoryRepo();
        ProductRepo myProductRepo = new ProductRepo();
        ProductPictureRepo myProductPictureRepo = new ProductPictureRepo();

        //Bu controller a Admin gibi yetkili kişiler erişecektir
        //Burada ürünlerin listelenmesi, ekleme, silme, güncelleme işlemleri yapılacakır.
        public ActionResult ProductList(string search = "")
        {
            //Alt Kategorileri repo aracılığıyla db'den çektik
            ViewBag.SubCategories = myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList();
            List<Product> allProducts = new List<Product>();//Boş bir liste ekliyorum 
            //var allProducts = myProductRepo.GetAll();
            if (string.IsNullOrEmpty(search))//eğer search in içi boi ise bütün productları getir
            {
                allProducts = myProductRepo.GetAll();
            }
            else
            {
                allProducts = myProductRepo.GetAll()
                    .Where(x => x.ProductName.ToLower().Contains(search.ToLower()) || x.Description.ToLower().Contains(search.ToLower())).ToList();
            }

            return View(allProducts);
            //return View(myProductRepo.GetAll());//üst satır gibi olur
        }

        [HttpGet]
        public ActionResult Create()
        {
            //Sayfayı çağırırken ürünün kategorisinin ne olduğunu seçmesi lazım
            //Bu nedenle sayfaya kategoriler gitmeli
            List<SelectListItem> subCategories = new List<SelectListItem>();
            //Linq
            //select * from Categories where BaseCategoriesId is not null

            myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList().ForEach(x => subCategories.Add(
                new SelectListItem()
                {
                    Text = x.CategoryName,
                    Value = x.Id.ToString()
                }));
            ViewBag.SubCategories = subCategories;
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            try
            {
                List<SelectListItem> subCategories = new List<SelectListItem>();
                myCategoryRepo.AsQueryable().Where(x => x.BaseCategoryId != null).ToList().ForEach(x => subCategories.Add(
                    new SelectListItem()
                    {
                        Text = x.CategoryName,
                        Value = x.Id.ToString()
                    }));
                ViewBag.SubCategories = subCategories;

                //if (!ModelState.IsValid)//Eğer modelstate isvalid değilse
                //{
                //    ModelState.AddModelError("", "Veri girişleri düzgün olmalıdır");
                //    return View(model);
                //}

                if (model.CategoryId <= 0 || model.CategoryId > myCategoryRepo.GetAll().Count())
                {
                    ModelState.AddModelError("", "Ürüne ait kategori seçilmelidir!");
                    return View(model);
                }

                // Burada kontrol lazım
                //Acaba girdiği ürün kodu bizim db de zaten var mı?

                if (myProductRepo.IsSameProductCode(model.ProductCode))
                {
                    ModelState.AddModelError("", "Dikkat! girdiğiniz ürün kodu sistemdeki bir başka ürüne aittir. Ürün kodları benzersiz olmalıdır.");
                    return View(model);
                }

                //Ürün tabloya kayıt olacak.
                //TO DO: Mapleme yapılacak.

                //Product product = new Product()
                //{
                //    ProductName = model.ProductName,
                //    Description = model.Description,
                //    ProductCode = model.ProductCode,
                //    CategoryId = model.CategoryId,
                //    Discount = model.Discount,
                //    Quantity = model.Quantity,
                //    RegisterDate = DateTime.Now,
                //    Price = model.Price
                //};
                //Mapleme yapıldı.
                //Mapster paketi indirildi.Mapster bir objedeki diğer bir objeye zehmetsizce aktarır.
                //Aktarım yapabilmesi için A objesiyle B objesinin içindeki propertylerin isimleri ve tipleri birebir aynı olmalıdır.
                //Bu projede Mater kullandık
                //Core projesinde daha profesyonel olan AutoMapper'ı kullanacağız.
                //Bir dto objesinin içindeki verileri alır asıl objenin içine aktarır.
                //Asıl objesinşn verilerini dto objesinin içindeki propertylere aktarır.

                Product product = model.Adapt<Product>();
                //Product product = model.Adapt<ProductViewModel, Product>();//üsttekinin 2. yolu

                int insertResult = myProductRepo.Insert(product);
                if (insertResult > 0)
                {
                    //Sıfırdan büyükse product tabloya eklendi
                    //Acaba bu producta resim seçmiş mi? resim seçtiyse o resimlerin yollarını kayıt et 
                    if (model.Files.Any())
                    {
                        ProductPicture productPicture = new ProductPicture();
                        productPicture.ProductId = product.Id;
                        productPicture.RegisterDate = DateTime.Now;
                        int counter = 1; // Bizim sistemde resim adedi 5 olarak belirlendiği için 
                        foreach (var item in model.Files)
                        {
                            if (counter == 5) break;
                            if (item != null && item.ContentType.Contains("İmage") && item.ContentLength < 0)
                            {
                                string filename = SiteSettings.StringCharacterConverter(model.ProductName).ToLower().Replace("-", "");
                                string extensionName = Path.GetExtension(item.FileName);
                                string directoryPath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}");
                                string guid = Guid.NewGuid().ToString().Replace("-", "");
                                string filePath = Server.MapPath($"~/ProductPictures/{filename}/{model.ProductCode}/")
                                    + filename + "-" + counter + "-" + guid + extensionName;
                                if (!Directory.Exists(directoryPath))
                                {
                                    Directory.CreateDirectory(directoryPath);
                                }
                                item.SaveAs(filePath);
                                //TODO: Buraya birisi çekidüzen vermeliiiiiiiiiiiiii
                                if (counter == 1)
                                {
                                    productPicture.ProductPicture1 = $"/ProductPicture/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }
                                if (counter == 2)
                                {
                                    productPicture.ProductPicture2 = $"/ProductPicture/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }
                                if (counter == 3)
                                {
                                    productPicture.ProductPicture3 = $"/ProductPicture/{filename}/{model.ProductCode}/" + filename + "-" + counter + "-" + guid + extensionName;
                                }
                            }
                            counter++;
                        }
                        //TO DO: Yukarıyı for'a dönüştürebilir miyiz?
                        //for (int i = 0; i < model.Files.Count; i++)
                        //{

                        //}

                        int productPictureInserResult = myProductPictureRepo.Insert(productPicture);
                        if (productPictureInserResult > 0)
                        {
                            return RedirectToAction("ProductList", "Product");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Ürün eklendi ama ürüne ait fotoğraf(lar) eklenirken beklenmedik bir hata oluştu! " +
                                "Ürününüzün fotoğraflarını daha sonra tekrar eklemeyi deneyebilirsiniz...");
                            return View(model);
                        }
                    }
                    else
                    {
                        return RedirectToAction("ProductList", "Product");
                    }

                }
                else
                {
                    ModelState.AddModelError("", "HATA: Ürün ekleme işleminde bir hata oluştu! Tekrar deneyiniz!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Beklenmedik bir hata oluştu");
                //ex loglanacak
                return View(model);
            }
        }
        
        public JsonResult GetProductDetails(int id) //mvc de sonu result ile biten geri dönüş tipleri vardır.
        {
            try
            {
                var product = myProductRepo.GetById(id);
                if (product!=null)
                {
                    //var data = product.Adapt<ProductViewModel>();
                    var data = new ProductViewModel()
                    {
                        Id = product.Id,
                        ProductName = product.ProductName,
                        Description = product.Description,
                        ProductCode = product.ProductCode,
                        CategoryId = product.CategoryId,
                        Discount = product.Discount,
                        Quantity = product.Quantity,
                        RegisterDate = product.RegisterDate,
                        Price = product.Price
                        
                    };
                    return Json(new {isSuccess=true,data },JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { isSuccess = false });

                }
            }
            catch (Exception ex)
            {
                //ex loglansn
                return Json(new { isSuccess=false});
                
            }
        }

        public ActionResult Edit(ProductViewModel model)
        {
            try
            {
                var product = myProductRepo.GetById(model.Id);
                if (product!=null)
                {
                    product.ProductName = model.ProductName;
                    product.Description = model.Description;
                    product.Discount = model.Discount;
                    product.Quantity = model.Quantity;
                    product.ProductCode = model.ProductCode;
                    product.Price = model.Price;
                    product.CategoryId = model.CategoryId;

                    int updateResult = myProductRepo.Update(); //DEğişimi görmek için int şeklinde gördük değeri
                    if (updateResult>0)
                    {
                        TempData["EditSuccess"] = "Bilgiler başarıyla güncellenmiştir!";
                        return RedirectToAction("ProductList", "Product");
                    }
                    else
                    {
                        TempData["EditFailed"] = "Beklenmedik bir hata olduğu için ürüne bilgileri sisteme aktarılmadı!";
                        return RedirectToAction("ProductList", "Product");
                    }
                    //Viewbag ile category gittiği için eklemeye gerek duymadık
                }
                else
                {
                    TempData["EditFailed"] = "Ürün bulunamadığı için ürün bilgileri güncellenemedi!";
                    return RedirectToAction("ProductList", "Product");
                }
            }
            catch (Exception ex)
            {

                //ex loglanacak
                TempData["EditFailed"] = "Beklenmedik bir hata nedeniyle ürün bilgileri güncellenemedi!";
                return RedirectToAction("ProductList", "Product");
            }
        }

    }
}