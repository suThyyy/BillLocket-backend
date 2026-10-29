using BillLocket.Domain.Entities;
using BillLocket.Domain.Enums;

namespace BillLocket.Domain.Tests
{
    public class FriendshipTests
    {
        [Fact]
        public void Create_WithValidUsers_ShouldCreatePendingFriendship()
        {
            // Arrange
            var requesterId = Guid.NewGuid().ToString("D");
            var addresseeId = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            // Act
            var friendship = Friendship.Create(
                requesterId,
                addresseeId,
                now);

            // Assert
            Assert.NotEqual(Guid.Empty, friendship.Id);
            Assert.Equal(requesterId, friendship.RequesterId);
            Assert.Equal(addresseeId, friendship.AddresseeId);
            Assert.Equal(FriendshipStatus.Pending, friendship.Status);
            Assert.Equal(now, friendship.CreatedAt);
            Assert.Equal(now, friendship.UpdatedAt);
            Assert.Null(friendship.RespondedAt);
        }

        [Fact]
        public void Create_WhenRequesterEqualsAddressee_ShouldThrow()
        {
            // Arrange
            var userId = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                Friendship.Create(userId, userId, now));
        }
        [Theory]
        [InlineData("abc", "xyz")]
        [InlineData("", "xyz")]
        [InlineData("invalid-guid", "invalid-guid-2")]
        public void Create_WithInvalidUserIds_ShouldThrow(
    string requesterId,
    string addresseeId)
        {
            var now = DateTimeOffset.UtcNow;

            Assert.Throws<ArgumentException>(() =>
                Friendship.Create(requesterId, addresseeId, now));
        }
        [Fact]
        public void Create_WhenUsersAreReversed_ShouldNormalizeSamePair()
        {
            // Arrange
            var userA = Guid.NewGuid().ToString("D");
            var userB = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            // Act
            var friendshipAB = Friendship.Create(userA, userB, now);
            var friendshipBA = Friendship.Create(userB, userA, now);

            // Assert
            Assert.Equal(friendshipAB.UserLowId, friendshipBA.UserLowId);
            Assert.Equal(friendshipAB.UserHighId, friendshipBA.UserHighId);

            Assert.NotEqual(
                friendshipAB.RequesterId,
                friendshipBA.RequesterId);
        }
        [Fact]
        public void Accept_ByAddressee_ShouldChangeStatusToAccepted()
        {
            // Arrange
            var requesterId = Guid.NewGuid().ToString("D");
            var addresseeId = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            var friendship = Friendship.Create(
                requesterId, addresseeId, now);

            // Act
            friendship.Accept(addresseeId, now.AddMinutes(5));

            // Assert
            Assert.Equal(FriendshipStatus.Accepted, friendship.Status);
            Assert.Equal(now.AddMinutes(5), friendship.RespondedAt);
            Assert.Equal(now.AddMinutes(5), friendship.UpdatedAt);
        }
        [Fact]
        public void Accept_ByRequester_ShouldThrow()
        {
            var requesterId = Guid.NewGuid().ToString("D");
            var addresseeId = Guid.NewGuid().ToString("D");

            var friendship = Friendship.Create(
                requesterId, addresseeId, DateTimeOffset.UtcNow);

            Assert.Throws<InvalidOperationException>(() =>
                friendship.Accept(requesterId, DateTimeOffset.UtcNow));

            Assert.Equal(FriendshipStatus.Pending, friendship.Status);
        }
        [Fact]
        public void Accept_WhenAlreadyAccepted_ShouldThrow()
        {
            var requesterId = Guid.NewGuid().ToString("D");
            var addresseeId = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            var friendship = Friendship.Create(
                requesterId, addresseeId, now);

            friendship.Accept(addresseeId, now.AddMinutes(1));

            Assert.Throws<InvalidOperationException>(() =>
                friendship.Accept(addresseeId, now.AddMinutes(2)));

            Assert.Equal(FriendshipStatus.Accepted, friendship.Status);
            Assert.Equal(now.AddMinutes(1), friendship.RespondedAt);
        }
        [Fact]
        public void Reject_ByAddressee_ShouldChangeStatusToRejected()
        {
            var requesterId = Guid.NewGuid().ToString("D");
            var addresseeId = Guid.NewGuid().ToString("D");
            var now = DateTimeOffset.UtcNow;

            var friendship = Friendship.Create(
                requesterId, addresseeId, now);

            friendship.Reject(addresseeId, now.AddMinutes(5));

            Assert.Equal(FriendshipStatus.Rejected, friendship.Status);
            Assert.Equal(now.AddMinutes(5), friendship.RespondedAt);
            Assert.Equal(now.AddMinutes(5), friendship.UpdatedAt);
        }

    }
}
