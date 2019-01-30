using Microsoft.WindowsAzure.Storage.Table;
using System.ComponentModel.DataAnnotations;

namespace Common
{
    public enum Sex
    {
        NotSpecified,
        Male,
        Female
    }

    public class User
    {
        [Required, MaxLength(40), MinLength(3)]
        public string Name { get; set; }

        [Required, MaxLength(40), MinLength(15)]
        public string Email { get; set; }

        public int Age { get; set; }

        public Sex Sex { get; set; }
    }

    public class UserEntity : TableEntity
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public int Age { get; set; }

        public Sex Sex { get; set; }
    }
}
