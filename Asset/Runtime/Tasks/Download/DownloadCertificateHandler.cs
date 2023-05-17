using UnityEngine.Networking;

namespace NBC.Asset
{
    public class DownloadCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}