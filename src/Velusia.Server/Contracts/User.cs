using System;

namespace Velusia.Server.Contracts
{
    public record User
    {
        public string Id { get; set; }

        public string Name { get; init; }

        public string DisplayName { get; init; }

        public string EmailAddress { get; init; }

        public string PhoneNumber { get; init; }

        public bool Enabled { get; init; }

        public bool LockedOut { get; init; }

        public DateTime Changed { get; init; }

        public DateTime? PasswordChanged { get; init; }

        public string[] Groups { get; init; }
    }
}
