using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ECommerceLiteUI.Models
{
    public class ProfileViewModel:RegisterViewModel
    {
        //Ad,Soyad,Email,Username,Şifre ve Şifre Tekrarı,Yeni Şifre 

        [Required]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^[a-zA-Z]\w{4,14}$", ErrorMessage = @"	
            Şifrenin ilk karakteri bir harf olmalı, en az 5 en fazla 15 karakter içermeli ve harflerden başka karakter içermemelidir,
            sayılar ve alt çizgi kullanılabilir")]
        public string NewPassword { get; set; }


    }
}