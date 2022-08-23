namespace Microsoft.ALTA
{
    using System;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;

    public class CertificateConfig
    {
        public static async void RegisterCerts()
        {
            string certs = ConfigurationManager.AppSettings["MSTEST_CERTIFICATES"];
            string[] parsed = certs.Split(';');
            foreach (string key in parsed)
            {
                string[] kvCert = key.Split(',');
                string url = kvCert[0].Substring(kvCert[0].IndexOf('=') + 1);
                string name = kvCert[1].Substring(kvCert[1].IndexOf('=') + 1);

                var client = new SecretClient(new Uri(url), new DefaultAzureCredential());

                var secret = await client.GetSecretAsync(name);

                byte[] data = Convert.FromBase64String(secret.Value.Value);
                string password = null;

                X509Certificate2 certificate = new X509Certificate2(data, password, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

                var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                store.Open(OpenFlags.ReadWrite);
                store.Add(certificate);
            }
        }
    }
}