using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}