﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Categories")]
    public class Category : Base<int>
    {
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 100 karakter aralığında olmalıdır.")]
        [Display(Name = "Kategori Adı")]
        public string CategoryName { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 500 karakter aralığında olmalıdır.")]
        [Display(Name = "Kategori Açıklaması")]
        public string CategoryDescription { get; set; }

        public int? BaseCategoryId { get; set; } //int normalde asla null değer alamaz.int'in yanında ? yazarsak Nullable bir int oluşturmuş oluruz.
        //public Nullable<int> BaseCategoryId eskiden böyle yazılabilirdi.Şimdi soru işareti yazarak aynı işlemi yapmış oluyoruz

        [ForeignKey("BaseCategoryId")]
        public virtual Category BaseCategory { get; set; }
        public virtual List<Category> SubCategoryList { get; set; }

        //Her ürünün bir kategorisi olur cümlesinden yola çıkarak Productta tanımlanan ilişkiyi burada karşıyalım
        //Bire sonsuz ilişki nedeniyle yani bir kategorinin bir den çok ürünü olabilir mantığını karşılamak amacıyla buradaki
        //virtual property List tipindedir.
        public virtual List<Product> ProductList { get; set; }


    }
}
