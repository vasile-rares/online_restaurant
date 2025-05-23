using System;
using System.IO;
using System.Text.Json;

namespace OnlineRestaurant.Services
{
    public class UserCredentialsService
    {
        private readonly string _credentialsFile;

        public UserCredentialsService()
        {
            // Create the credentials file in the user's application data directory
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OnlineRestaurant");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _credentialsFile = Path.Combine(appDataPath, "user_credentials.json");
        }

        public void SaveCredentials(string email, string password, bool rememberMe)
        {
            if (!rememberMe)
            {
                if (File.Exists(_credentialsFile))
                {
                    File.Delete(_credentialsFile);
                }
                return;
            }

            var credentials = new UserCredentials
            {
                Email = email,
                Password = password
            };

            string json = JsonSerializer.Serialize(credentials);
            File.WriteAllText(_credentialsFile, json);
        }

        public UserCredentials LoadCredentials()
        {
            if (!File.Exists(_credentialsFile))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(_credentialsFile);
                var credentials = JsonSerializer.Deserialize<UserCredentials>(json);
                return credentials;
            }
            catch
            {
                File.Delete(_credentialsFile);
                return null;
            }
        }

        public void ClearCredentials()
        {
            if (File.Exists(_credentialsFile))
            {
                File.Delete(_credentialsFile);
            }
        }

        public class UserCredentials
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}