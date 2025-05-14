using HCSSystem.Data;
using HCSSystem.Helpers;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace HCSSystem.ViewModels;

public class ProfileFlipperViewModel : INotifyPropertyChanged
{
    private string _photoPath;
    private string _newPassword;
    private string _confirmPassword;

    public string FullName { get; set; }
    public string PhotoPath
    {
        get => _photoPath;
        set { _photoPath = value; OnPropertyChanged(); }
    }

    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set { _confirmPassword = value; OnPropertyChanged(); }
    }

    public ICommand SelectPhotoCommand { get; }
    public ICommand SaveCommand { get; }

    public ProfileFlipperViewModel()
    {
        var user = App.CurrentUser;
        if (user != null)
        {
            using var db = new HcsDbContext();

            var client = db.Clients.FirstOrDefault(c => c.Id == user.Id);
            var employee = db.Employees.FirstOrDefault(e => e.Id == user.Id);

            FullName = client != null
                ? $"{client.LastName} {client.FirstName}"
                : employee != null
                    ? $"{employee.LastName} {employee.FirstName}"
                    : user.Username;

            var photoFile = client?.PhotoFileName ?? employee?.PhotoFileName;

            if (!string.IsNullOrEmpty(photoFile))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Users", photoFile);
                if (File.Exists(path))
                    PhotoPath = path;
            }
        }

        SelectPhotoCommand = new RelayCommand(_ => SelectPhoto());
        SaveCommand = new RelayCommand(_ => SaveChanges());
    }

    private void SelectPhoto()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png",
            Title = "Выберите новое фото"
        };

        if (dlg.ShowDialog() == true)
        {
            var ext = Path.GetExtension(dlg.FileName);
            var fileName = Guid.NewGuid() + ext;
            var destPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Images", "Users", fileName);
            File.Copy(dlg.FileName, destPath, true);
            PhotoPath = destPath;

            using var db = new HcsDbContext();
            var client = db.Clients.FirstOrDefault(c => c.Id == App.CurrentUser.Id);
            var employee = db.Employees.FirstOrDefault(e => e.Id == App.CurrentUser.Id);

            if (client != null)
                client.PhotoFileName = fileName;
            else if (employee != null)
                employee.PhotoFileName = fileName;

            db.SaveChanges();
        }
    }

    private void SaveChanges()
    {
        if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            MessageBox.Show("Введите и подтвердите пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = new HcsDbContext();
        var user = db.Users.FirstOrDefault(u => u.Id == App.CurrentUser.Id);

        if (user != null)
        {
            user.PasswordHash = NewPassword; // предположим, пока без хеша
            db.SaveChanges();
            MessageBox.Show("Пароль обновлён.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
