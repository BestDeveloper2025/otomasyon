using System.Net;
using System.Text;
using otomasyon.Localization;
using otomasyon.Settings;

namespace otomasyon.Simulation;

/// <summary>Reçete dosyasını yapılandırılmış FTP sunucusuna yükler.</summary>
public static class FtpFileUploader
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static bool TryUploadText(
        FtpSettings settings,
        string remoteFileName,
        string content,
        out string? error)
    {
        byte[] bytes = Utf8WithBom.GetBytes(content);
        return TryUploadBytes(settings, remoteFileName, bytes, out error);
    }

    public static bool TryUploadBytes(
        FtpSettings settings,
        string remoteFileName,
        byte[] content,
        out string? error)
    {
        error = null;

        if (!settings.IsConfigured)
        {
            error = L.Get("Msg.FtpNotConfigured");
            return false;
        }

        if (content.Length == 0)
        {
            error = L.Get("Error.FtpEmptyContent");
            return false;
        }

        string fileName = SanitizeRemoteFileName(remoteFileName);
        if (string.IsNullOrEmpty(fileName))
        {
            error = L.Get("Error.FtpInvalidFileName");
            return false;
        }

        try
        {
            var request = FtpClientHelper.CreateRequest(
                settings,
                WebRequestMethods.Ftp.UploadFile,
                FtpClientHelper.BuildFileUri(settings, fileName));
            request.ContentLength = content.Length;

            using (var requestStream = request.GetRequestStream())
                requestStream.Write(content, 0, content.Length);

            using var response = (FtpWebResponse)request.GetResponse();
            if (response.StatusCode is not FtpStatusCode.ClosingData
                and not FtpStatusCode.FileActionOK
                and not FtpStatusCode.CommandOK)
            {
                error = L.F("Error.FtpUploadFailed", response.StatusDescription?.Trim() ?? response.StatusCode.ToString());
                return false;
            }

            return true;
        }
        catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
        {
            error = L.F("Error.FtpUploadFailed", ftpResponse.StatusDescription?.Trim() ?? ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            error = L.F("Error.FtpUploadFailed", ex.Message);
            return false;
        }
    }

    private static string SanitizeRemoteFileName(string remoteFileName)
    {
        string name = Path.GetFileName(remoteFileName.Trim());
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        return name;
    }
}
