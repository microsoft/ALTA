using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ALTA
{
    public class CertificateConfig
    {

        public static async void RegisterCerts()
        {
            const string secretName = "alta-test-certificate-protected";
            var kvUri = "https://test-internship-kv.vault.azure.net";


            var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

            var secret = await client.GetSecretAsync(secretName);

            byte[] data = Convert.FromBase64String(secret.Value.Value);
            string password = null;

            X509Certificate2 certificate = new X509Certificate2(data, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            store.Add(certificate);
        }
    }
}