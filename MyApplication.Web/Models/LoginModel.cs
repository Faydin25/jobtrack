﻿using Microsoft.AspNetCore.Mvc;

namespace MyApplication.Web.Models
{
    public class LoginModel
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
