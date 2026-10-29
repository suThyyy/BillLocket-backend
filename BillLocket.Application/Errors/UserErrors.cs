using BillLocket.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Application.Errors
{
    public static class UserErrors
    {
        public static readonly AppError NotFound =
            new("USER_NOT_FOUND", "User not found.");

        public static readonly AppError UsernameTaken =
            new("USER_USERNAME_TAKEN", "Username is already taken.");

        public static readonly AppError UsernameUnchanged =
            new("USER_USERNAME_UNCHANGED", "New username matches current username.");

        public static readonly AppError UsernameCooldown =
            new("USER_USERNAME_COOLDOWN", "Username can only be changed once every 30 days.");

        public static readonly AppError InvalidUsername =
            new("USER_INVALID_USERNAME", "Invalid username.");

        public static readonly AppError ConcurrencyConflict = new(
    "USER_CONCURRENCY_CONFLICT",
    "User information was modified by another request. Please try again.");
    }
}
