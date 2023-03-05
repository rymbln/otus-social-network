using System.ComponentModel.DataAnnotations;

namespace OtusSocialNetwork.DataClasses.Requests
{
    public class RegisterReq
    {
        [Required]
        public string First_name { get; set; }

        [Required]
        public string Second_name { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string Sex { get; set; }

        [Required]
        public string Biography { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Password { get; set; }

    }
}
