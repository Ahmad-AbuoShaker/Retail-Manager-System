using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.User
{
    public class UserDto
    {
        public Guid Id {  get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } 
        public DateTime CreatedDate { get; set; }

    }
}
