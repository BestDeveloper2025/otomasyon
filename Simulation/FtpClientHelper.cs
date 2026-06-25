using System.Net;
using System.Text;
using otomasyon.Localization;
using otomasyon.Settings;

namespace otomasyon.Simulation;

internal static class FtpClientHelper
{
    public static string BuildDirectoryUri(FtpSettings settings)
    {
        string host = settings.Host.Trim();
        string dir = FtpSettings.NormalizeRemoteDirectory(settings.RemoteDirectory);
        string path = string.IsNullOrEmpty(dir) ? string.Empty : $"{dir}/";
        return settings.Port == FtpSettings.DefaultPort
            ? $"ftp://{host}/{path}"
            : $"ftp://{host}:{settings.Port}/{path}";
    }

    public static string BuildFileUri(FtpSettings settings, string fileName)
    {
        string host = settings.Host.Trim();
        string remotePath = settings.GetRemoteFilePath(fileName);
        return settings.Port == FtpSettings.DefaultPort
            ? $"ftp://{host}/{remotePath}"
            : $"ftp://{host}:{settings.Port}/{remotePath}";
    }

    public static bool TryListFileNames(FtpSettings settings, out IReadOnlyList<string> fileNames, out string? error)
    {
        fileNames = Array.Empty<string>();
        error = null;

        if (!settings.IsConfigured)
        {
            error = L.Get("Msg.FtpNotConfiguredShort");
            return false;
        }

        try
        {
            var request = CreateRequest(settings, WebRequestMethods.Ftp.ListDirectory, BuildDirectoryUri(settings));
            using var response = (FtpWebResponse)request.GetResponse();
            using var stream = response.GetResponseStream();
            if (stream is null)
            {
                error = L.Get("Error.FtpListFailed");
                return false;
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            var names = new List<string>();
            while (reader.ReadLine() is { } line)
            {
                string name = line.Trim();
                if (name.Length == 0 || name is "." or "..")
                    continue;
                names.Add(name);
            }

            names.Sort(StringComparer.OrdinalIgnoreCase);
            fileNames = names;
            return true;
        }
        catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
        {
            error = L.F("Error.FtpListFailedDetail", ftpResponse.StatusDescription?.Trim() ?? ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            error = L.F("Error.FtpListFailedDetail", ex.Message);
            return false;
        }
    }

    public static bool TryDeleteFile(FtpSettings settings, string fileName, out string? error)
    {
        error = null;

        if (!settings.IsConfigured)
        {
            error = L.Get("Msg.FtpNotConfiguredShort");
            return false;
        }

        string safeName = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            error = L.Get("Error.FtpInvalidFileName");
            return false;
        }

        try
        {
            var request = CreateRequest(settings, WebRequestMethods.Ftp.DeleteFile, BuildFileUri(settings, safeName));
            using var response = (FtpWebResponse)request.GetResponse();
            if (response.StatusCode is FtpStatusCode.FileActionOK or FtpStatusCode.CommandOK)
                return true;

            error = L.F("Error.FtpDeleteFailed", response.StatusDescription?.Trim() ?? response.StatusCode.ToString());
            return false;
        }
        catch (WebException ex) when (ex.Response is FtpWebResponse ftpResponse)
        {
            error = L.F("Error.FtpDeleteFailed", ftpResponse.StatusDescription?.Trim() ?? ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            error = L.F("Error.FtpDeleteFailed", ex.Message);
            return false;
        }
    }

    public static FtpWebRequest CreateRequest(FtpSettings settings, string method, string uri)
    {
#pragma warning disable SYSLIB0014
        var request = (FtpWebRequest)WebRequest.Create(uri);
#pragma warning restore SYSLIB0014
        request.Method = method;
        request.Credentials = new NetworkCredential(settings.Username, settings.Password);
        request.UseBinary = true;
        request.UsePassive = true;
        request.KeepAlive = false;
        return request;
    }
}
