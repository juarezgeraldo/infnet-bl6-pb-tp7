using Azure.Identity;
using Azure.Storage.Blobs;

namespace AzureBlobs;

public class BlobService
{
    public BlobServiceClient BlobServiceClient { get; set; }
    public BlobContainerClient Container { get; set; }

    public BlobService()
    {
        BlobServiceClient = new BlobServiceClient(
            new Uri("https://photosatinfnet.blob.core.windows.net"),
            new DefaultAzureCredential());

        Container = BlobServiceClient.GetBlobContainerClient("pb-at");
    }

    public async Task<string> AdicionarBlobAoContainer(string base64)
    {
        var id = $"{Guid.NewGuid()}.{GetFileExtension(base64)}";
        var blob = Container.GetBlobClient(id); // update as necessary

        var sourceData = Convert.FromBase64String(base64);
        var uploadData = new BinaryData(sourceData);
        await blob.UploadAsync(uploadData, true);

        return blob.Uri.AbsoluteUri;
    }

    private static string GetFileExtension(string base64String)
    {
        var data = base64String.Substring(0, 5);

        return data.ToUpper() switch
        {
            "IVBOR" => "png",
            "/9J/4" => "jpg",
            "AAAAF" => "mp4",
            "JVBER" => "pdf",
            "AAABA" => "ico",
            "UMFYI" => "rar",
            "E1XYD" => "rtf",
            "U1PKC" => "txt",
            "MQOWM" => "srt",
            "77U/M" => "srt",
            _ => "jpeg"
        };
    }
}