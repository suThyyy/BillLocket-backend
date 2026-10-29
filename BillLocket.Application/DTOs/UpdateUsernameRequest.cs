using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BillLocket.Application.DTOs
{
    public sealed class UpdateUsernameRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        [RegularExpression(
            @"^[a-z0-9_]+$",
            ErrorMessage = "Username must contain only lowercase letters, numbers, and underscores.")]
        public string Username { get; init; } = string.Empty;
    }
}
