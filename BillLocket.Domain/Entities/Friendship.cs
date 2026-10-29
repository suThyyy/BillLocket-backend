using BillLocket.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Domain.Entities
{
    public sealed class Friendship
    {
        private Friendship() { }

        public Guid Id { get; private set; }

        public string RequesterId { get; private set; } = string.Empty;
        public string AddresseeId { get; private set; } = string.Empty;

        public string UserLowId { get; private set; } = string.Empty;
        public string UserHighId { get; private set; } = string.Empty;

        public FriendshipStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? RespondedAt { get; private set; }
        public DateTimeOffset UpdatedAt { get; private set; }

        public byte[] RowVersion { get; private set; } = [];

        public static Friendship Create(
            string requesterId,
            string addresseeId,
            DateTimeOffset now)
        {
            ValidateUsers(requesterId, addresseeId);

            var (lowId, highId) =
                NormalizePair(requesterId, addresseeId);

            return new Friendship
            {
                Id = Guid.NewGuid(),
                RequesterId = requesterId,
                AddresseeId = addresseeId,
                UserLowId = lowId,
                UserHighId = highId,
                Status = FriendshipStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void Accept(string actorId, DateTimeOffset now)
        {
            EnsureAddressee(actorId);
            EnsurePending();

            Status = FriendshipStatus.Accepted;
            RespondedAt = now;
            UpdatedAt = now;
        }

        public void Reject(string actorId, DateTimeOffset now)
        {
            EnsureAddressee(actorId);
            EnsurePending();

            Status = FriendshipStatus.Rejected;
            RespondedAt = now;
            UpdatedAt = now;
        }

        public void Reopen(
            string requesterId,
            string addresseeId,
            DateTimeOffset now)
        {
            ValidateUsers(requesterId, addresseeId);

            if (Status != FriendshipStatus.Rejected)
                throw new InvalidOperationException(
                    "Only rejected friendships can be reopened.");

            if (RespondedAt is null ||
                now < RespondedAt.Value.AddDays(7))
                throw new InvalidOperationException(
                    "Friend request is not yet allowed.");

            var (lowId, highId) =
                NormalizePair(requesterId, addresseeId);

            if (UserLowId != lowId || UserHighId != highId)
                throw new InvalidOperationException(
                    "Cannot reopen a friendship for another pair.");

            RequesterId = requesterId;
            AddresseeId = addresseeId;
            Status = FriendshipStatus.Pending;
            CreatedAt = now;
            RespondedAt = null;
            UpdatedAt = now;
        }

        public void EnsureCanCancel(string actorId)
        {
            if (RequesterId != actorId)
                throw new InvalidOperationException(
                    "Only the requester can cancel.");

            EnsurePending();
        }

        public void EnsureCanUnfriend(string actorId)
        {
            if (Status != FriendshipStatus.Accepted)
                throw new InvalidOperationException(
                    "Only accepted friendships can be removed.");

            if (RequesterId != actorId && AddresseeId != actorId)
                throw new InvalidOperationException(
                    "User is not part of this friendship.");
        }

        private void EnsureAddressee(string actorId)
        {
            if (AddresseeId != actorId)
                throw new InvalidOperationException(
                    "Only the addressee can respond.");
        }

        private void EnsurePending()
        {
            if (Status != FriendshipStatus.Pending)
                throw new InvalidOperationException(
                    "Friend request is not pending.");
        }

        private static void ValidateUsers(
            string requesterId,
            string addresseeId)
        {
            if (string.IsNullOrWhiteSpace(requesterId) ||
                string.IsNullOrWhiteSpace(addresseeId))
                throw new ArgumentException(
                    "User IDs are required.");

            if (requesterId == addresseeId)
                throw new InvalidOperationException(
                    "Cannot send a friend request to yourself.");
        }

        private static (string LowId, string HighId) NormalizePair(
            string firstId,
            string secondId)
        {
            return string.CompareOrdinal(firstId, secondId) < 0
                ? (firstId, secondId)
                : (secondId, firstId);
        }
    }
}
