using System.IO;

namespace HCSSystem.ViewModels.Models;

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; }        
    public string FullName { get; set; }         
    public string Email { get; set; }
    public string Phone { get; set; }
    public DateTime BirthDate { get; set; }
    public string? PhotoFileName { get; set; }
    public string RoleName { get; set; }
    public bool IsActive { get; set; }
    public string PhotoPath =>
        string.IsNullOrEmpty(PhotoFileName)
            ? Path.Combine(Environment.CurrentDirectory, "Resources", "Images", "Users", "no_avatar.jpg")
                .Replace('\\', '/')
            : Path.Combine(Environment.CurrentDirectory, "Resources", "Images", "Users", PhotoFileName)
                .Replace('\\', '/');
}