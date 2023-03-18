
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace DiaryUI
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = "";
        public byte[] Password { get; set; } = new byte[0];

        public override string Slug => "User";

        public override string Describe()
        {
            return $"User:{Username}";
        }
    }
}
