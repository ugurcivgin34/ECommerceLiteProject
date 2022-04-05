using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class RegisterViewModel
    {
        //Kayıt Modeli içinde siteye kayıt olmak isteyen kişilerden hangi bilgileri alacağımızı belirleyeceğiz
        //TcKimlik,isim soy isim email varsa (eğer yazdıysak telefon,cinsiyet vb) alanlarını tanınlayalım

        //Not: Data Annotation'ları kullanarak validation kurallarını belirlediğimiz için kapsüllemeye gerek kalmadı.

        [Required]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "Tc kimlik numarası 11 haneli olmalıdır")]
        [Display(Name = "Tc Kimlik")]
        public string TCNumber { get; set; }

        [Required]
        [Display(Name = "Ad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "İsminizin uzunluğu 2 ile 30 karakter aralığında olmalıdır.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Soyadınızın uzunluğu 2 ile 30 karakter aralığında olmalıdır.")]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^[a-zA-Z]\w{4,14}$", ErrorMessage = @"	
            Şifrenin ilk karakteri bir harf olmalı, en az 5 en fazla 15 karakter içermeli ve harflerden başka karakter içermemelidir,
            sayılar ve alt çizgi kullanılabilir")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Şifre Tekrarı")]
        [DataType(DataType.Password)]
        [Compare("Password",ErrorMessage ="Şifreler uyuşmuyor.Tekrar deneyiniz")]
        public string ConfirmPassword { get; set; }

    }
}