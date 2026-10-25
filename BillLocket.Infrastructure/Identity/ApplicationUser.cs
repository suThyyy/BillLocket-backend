using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }
            = DateTimeOffset.UtcNow;
    }
}
