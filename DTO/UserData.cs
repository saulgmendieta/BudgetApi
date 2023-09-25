using System.ComponentModel.DataAnnotations;

namespace BasicApi.DTO
{
    public class UserData
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }

    }
}
