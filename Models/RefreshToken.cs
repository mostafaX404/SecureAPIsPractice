using Microsoft.EntityFrameworkCore;

namespace SecureAPIsPractice.Models
{
    [Owned]
    public class RefreshToken
    {
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public bool Isexpired => DateTime.UtcNow > ExpiresOn;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => RevokedOn == null && !Isexpired;  

    }
}
