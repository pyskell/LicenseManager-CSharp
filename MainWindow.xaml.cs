// MIT Licensed. Copyright (c) 2017
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Portable.Licensing;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace LicenseManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _privateKey = null;
        private static readonly HttpClient _client = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();

            TrialLicenseDate.SelectedDate = DateTime.Now.AddDays(60);

            CreateKeyPair.Click += CreateKeyPair_Click;
            CreateLicense.Click += CreateLicense_Click;
            OpenPrivateKey.Click += OpenPrivateKey_Click;

            InstallLimit.PreviewTextInput += InstallLimit_PreviewTextInput;
            UnlimitedInstallsCheckBox.Click += UnlimitedInstallsCheckBox_Click;
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            MessageBox.Show(
                $"An exception occurred: {e.Exception.Message}. Additional information: {e.Exception.InnerException?.Message}");
        }

        private void UnlimitedInstallsCheckBox_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            InstallLimit.Text = "";
        }

        private void InstallLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsInteger(e.Text);
        }

        private bool IsInteger(string text)
        {
            int x;
            return Int32.TryParse(text, out x);
        }

        private void OpenPrivateKey_Click(object sender, RoutedEventArgs e)
        {
            FileInfo privateKeyFileInfo = OpenDialog("Private Key (.private_key)| *.private_key",
                "Open PRIVATE Key");

            _privateKey = File.ReadAllText(privateKeyFileInfo.FullName);
        }

        private void CreateLicense_Click(object sender, RoutedEventArgs e)
        {
            if (_privateKey == null) { return; }

            if (TrialLicenseDate.SelectedDate == null) { return; }

            if (String.IsNullOrWhiteSpace(LicenseeName.Text) || String.IsNullOrWhiteSpace(LicenseeEmail.Text))
            {
                return;
            }

            License license = License.New()
                .WithUniqueIdentifier(Guid.NewGuid())
                .As(LicenseType.Standard)
                .ExpiresAt(TrialLicenseDate.SelectedDate.Value)
                .WithMaximumUtilization(1)
                .LicensedTo(LicenseeName.Text, LicenseeEmail.Text)
                .CreateAndSignWithPrivateKey(_privateKey, CreateLicensePrivateKeyPassword.Password);

            FileInfo licenseFileInfo = SaveDialog("License File (.lic) | *.lic", "Save License");

            // Write UTF without the Byte Order Mark because it messes things up
            File.WriteAllText(licenseFileInfo.FullName,
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
{license}",
                new UTF8Encoding(false));

            if (!String.IsNullOrWhiteSpace(LicenseServerUrl.Text))
            {
                AddLicenseToServer(license);
            }
        }

        // For telling the server what license we want to add
        public class InsertResponse
        {
            [JsonProperty("Signature")]
            public string Signature { get; set; }

            [JsonProperty("InstallLimit")]
            public int InstallLimit { get; set; }

            [JsonProperty("UnlimitedInstalls")]
            public bool UnlimitedInstalls { get; set; }
        }

        private void AddLicenseToServer(License license)
        {
            // Auth
            string url = LicenseServerUrl.Text + "/insert";
            string username = LicenseServerUsername.Text;
            string password = LicenseServerPassword.Password;

            if (String.IsNullOrWhiteSpace(url) || String.IsNullOrWhiteSpace(username) ||
                String.IsNullOrWhiteSpace(password))
            {
                throw new DataException("Url, Username, and Password cannot be blank");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

            // Install limits
            int installLimit = 0;
            Int32.TryParse(InstallLimit.Text, out installLimit);
            bool unlimitedInstalls = UnlimitedInstallsCheckBox.IsChecked ?? false;

            if (installLimit == 0 && unlimitedInstalls == false)
            {
                throw new DataException(
                    "No install limit set (Install Limit is 0, and Unlimited Installs is false). Please set one of these.");
            }

            InsertResponse insertResponse = new InsertResponse
            {
                Signature = license.Signature,
                InstallLimit = installLimit,
                UnlimitedInstalls = unlimitedInstalls
            };

            string insertResponseString = JsonConvert.SerializeObject(insertResponse);

            StringContent stringContent = new StringContent(insertResponseString, Encoding.UTF8, "application/json");

            // Display response to the user so they know if it worked.
                _client.PostAsync(url, stringContent)
                    .ContinueWith(finishedTask =>
                        MessageBox.Show(finishedTask.Result.Content.ReadAsStringAsync().Result));
        }

        private void CreateKeyPair_Click(object sender, RoutedEventArgs e)
        {
            if (CreateKeyPairPassword.Password.Length == 0) { return; }
            if (CreateKeyPairPassword.Password != CreateKeyPairPasswordConfirm.Password) { return; }

            var keyGenerator = Portable.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();

            string publicKey = keyPair.ToPublicKeyString();
            string privateKey = keyPair.ToEncryptedPrivateKeyString(CreateKeyPairPassword.Password);

            FileInfo publicKeyFileInfo = SaveDialog("Public Key (.public_key)| *.public_key",
                "Save Public Key (Plain-Text)");
            FileInfo privateKeyFileInfo = SaveDialog("Private Key (.private_key)| *.private_key",
                "Save PRIVATE Key (ENCRYTPED)");

            File.WriteAllText(publicKeyFileInfo.FullName, publicKey);
            File.WriteAllText(privateKeyFileInfo.FullName, privateKey);

            CreateKeyPairPassword.Clear();
            CreateKeyPairPasswordConfirm.Clear();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        public static FileInfo OpenDialog(string fileFilter = null, string title = null)
        {
            string filename = null;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = fileFilter,
                Title = title
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = openFileDialog.FileName;
            }

            return filename != null ? new FileInfo(filename) : null;
        }

        public static FileInfo SaveDialog(string fileFilter = null, string title = null)
        {
            string filename = null;
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = fileFilter,
                Title = title
            };

            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filename = saveFileDialog.FileName;
            }

            return filename != null ? new FileInfo(filename) : null;
        }
    }
}