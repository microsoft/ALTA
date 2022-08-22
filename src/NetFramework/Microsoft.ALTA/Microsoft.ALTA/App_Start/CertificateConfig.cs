using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.ALTA
{
    public class CertificateConfig
    {

        public static async void RegisterCerts()
        {
            string certs = ConfigurationManager.AppSettings["MSTEST_CERTIFICATES"];
            string[] parsed = certs.Split(';');
            foreach (string key in parsed) {
                string[] KVCert = key.Split(',');
               
                string url = KVCert[0].Substring(KVCert[0].IndexOf('=') + 1);
                string name = KVCert[1].Substring(KVCert[1].IndexOf('=') + 1);

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