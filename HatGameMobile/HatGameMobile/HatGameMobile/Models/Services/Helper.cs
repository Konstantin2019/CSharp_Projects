using System;
using System.Linq;

namespace HatGameMobile.Models.Services
{
    public static class Helper
    {
        public static string GeneratePassword(int length)
        {
            var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(chars.OrderBy(o => Guid.NewGuid()).Take(length).ToArray());
        }
    }
}
