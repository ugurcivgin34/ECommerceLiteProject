﻿using ECommerceLiteEntity.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.IdentityModels
{
    public class ApplicationUser:IdentityUser
    {
        //IdentityUser'dan kalıtım alındı.Identity USer Microsoftu'un identity şemasına ait bir classtır.
        //IdentityUser classı ile bize sunulan AspNetUsers tablosundaki kolonları genişletmek için kalıtım aldık.
        //Aşağıya ihtiyacımız olan kolonları ekledik

        [Required]
        [Display(Name="Ad")]
        [StringLength(maximumLength:30,MinimumLength =2,ErrorMessage ="İsminizin uzunluğu 2 ile 30 karakter aralığında olmalıdır.")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(maximumLength: 30, MinimumLength = 2, ErrorMessage = "Soyadınızın uzunluğu 2 ile 30 karakter aralığında olmalıdır.")]
        public string Surname { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        //ToDo:Guid'in kaç haneli olduğuna bakıp buraya string string length ile attribute tanımlanacaktır.
        public string ActivationCode { get; set; }


        //İsteyen birthDate gibi bir alan ekleyebilir
        public virtual List<Admin> AdminList { get; set; }
        public virtual List<Customer> CustomerList { get; set; }
        public virtual List<PassiveUser> PassiveUserList { get; set; }



    }
}
