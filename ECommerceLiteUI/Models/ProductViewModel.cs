using ECommerceLiteBLL.Repository;
using ECommerceLiteEntity.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class ProductViewModel
    {
        
        CategoryRepo mycategoryRepo = new CategoryRepo();
        ProductPictureRepo myproductPictureRepo = new ProductPictureRepo();
        public int Id { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(maximumLength: 100, MinimumLength = 2, ErrorMessage = "Ürün Adı 2 ile 100 karakter aralığında olmalıdır")]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }
        [Required]
        [StringLength(maximumLength: 500, ErrorMessage = "Ürün Açıklaması en fazla 500 karakter olmalıdır")]
        [Display(Name = "Ürün Açıklaması")]
        public string Desctription { get; set; }
        [Required]
        [StringLength(maximumLength: 8, MinimumLength = 8, ErrorMessage = "Ürün kodu en fazla 8 karakter olmalıdır")]
        [Display(Name = "Ürün Kodu")]
        [Index(IsUnique = true)] //Benzersiz tekrarsız olmasını sağlar
        public string ProductCode { get; set; }
        [Required]
        [DataType(DataType.Currency)] //para birimi
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public double Discount { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public List<ProductPicture> ProductPictureList { get; set; } = new List<ProductPicture>();

        //Ürün eklenirken ürüne ait resimlere seçilebilir
        //Seçilebilen resimleri hafıza tutacak property
        public List<HttpPostedFileBase> Files { get; set; } = new List<HttpPostedFileBase>();

        public void GetProductPictures()
        {
            if (Id>0)
            {
                ProductPictureList = myproductPictureRepo.AsQueryable().Where(x => x.ProductId == Id).ToList();
            }
        }



    }
}