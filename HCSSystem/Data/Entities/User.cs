﻿namespace HCSSystem.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}