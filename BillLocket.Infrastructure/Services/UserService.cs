using BillLocket.Application.Common;
using BillLocket.Application.DTOs;
using BillLocket.Application.Errors;
using BillLocket.Application.Interfaces;
using BillLocket.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BillLocket.Infrastructure.Services
{
    public sealed class UserService(
      UserManager<ApplicationUser> userManager) : IUserService
    {
        public async Task<Result<UpdateUsernameResponse>> UpdateUsernameAsync(
            string userId,
            UpdateUsernameRequest request,
            CancellationToken cancellationToken = default)
        {
            // TODO: Implement
            cancellationToken.ThrowIfCancellationRequested();

            var username = request.Username;

            if (!Regex.IsMatch(username, @"^[a-z0-9_]{3,20}$"))
            {
                return Result<UpdateUsernameResponse>.Failure(
                    UserErrors.InvalidUsername);
            }

            var user = await userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Result<UpdateUsernameResponse>.Failure(
                    UserErrors.NotFound);
            }

            if (string.Equals(
                user.UserName,
                username,
                StringComparison.OrdinalIgnoreCase))
            {
                return Result<UpdateUsernameResponse>.Failure(
                    UserErrors.UsernameUnchanged);
            }

            var now = DateTimeOffset.UtcNow;

            // Kiểm tra người dùng đã từng đổi Username chưa.
            // `is { } lastChangedAt` trả về true nếu UsernameChangedAt khác null,
            // đồng thời lấy giá trị đó gán vào biến lastChangedAt.
            // - Khác null: vào if để kiểm tra đã đủ 30 ngày chưa.
            // - Bằng null: bỏ qua if, cho phép đổi Username lần đầu (Policy B).
            // Nếu chưa đủ 30 ngày thì return Failure; ngược lại tiếp tục xử lý bên dưới.
            if (user.UsernameChangedAt is { } lastChangedAt)
            {
                var nextChangeAllowedAt = lastChangedAt.AddDays(30);

                if (now < nextChangeAllowedAt)
                {
                    var error = UserErrors.UsernameCooldown with
                    {
                        Details = new Dictionary<string, object?>
                        {
                            ["nextChangeAllowedAt"] = nextChangeAllowedAt
                        }
                    };

                    return Result<UpdateUsernameResponse>.Failure(error);
                }
            }

            // TODO: Kiểm tra trùng Username và lưu thay đổi.
            // Tạm thời chưa triển khai phần này.
            var existingUser = await userManager.FindByNameAsync(username);

            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return Result<UpdateUsernameResponse>.Failure(
                    UserErrors.UsernameTaken);
            }
            user.UserName = username;
            user.UsernameChangedAt = now;

            var identityResult = await userManager.UpdateAsync(user);
            if (!identityResult.Succeeded)
            {
                if (identityResult.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    return Result<UpdateUsernameResponse>.Failure(
                        UserErrors.UsernameTaken);
                }

                if (identityResult.Errors.Any(e => e.Code == "ConcurrencyFailure"))
                {
                    return Result<UpdateUsernameResponse>.Failure(
                        UserErrors.ConcurrencyConflict);
                }

                throw new InvalidOperationException(
                    "Failed to update username.");
            }
            return Result<UpdateUsernameResponse>.Success(
    new UpdateUsernameResponse(
        username,
        now.AddDays(30)));
        }

    }
}
