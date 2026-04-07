using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SecureAPIsPractice.Models
{
    public class ApplicationUser : IdentityUser
    {

        [MaxLength(20)]
        public required string FirstName { get; set; }
        [MaxLength(20)]
        public required string LastName { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; }

    }
}
