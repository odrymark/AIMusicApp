using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Api.Services.R2;

public class R2Service : IR2Service
{
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _bucketName;
    private readonly string _endpoint;

    public R2Service(IConfiguration config)
    {
        _accessKey = config["R2:AccessKey"] ?? throw new InvalidOperationException("R2:AccessKey is missing");
        _secretKey = config["R2:SecretKey"] ?? throw new InvalidOperationException("R2:SecretKey is missing");
        _bucketName = config["R2:BucketName"] ?? throw new InvalidOperationException("R2:BucketName is missing");
        _endpoint = config["R2:Endpoint"] ?? throw new InvalidOperationException("R2:Endpoint is missing");

        if (string.IsNullOrWhiteSpace(_endpoint))
        {
            throw new InvalidOperationException("R2 Endpoint is empty or whitespace");
        }
    }

    public async Task<string> UploadSongStorage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var allowedExtensions = new[] { ".mp3", ".wav" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        using var s3Client = new AmazonS3Client(_accessKey, _secretKey, s3Config);
        var fileTransferUtility = new TransferUtility(s3Client);

        var fileName = $"songs/{Guid.NewGuid()}_{file.FileName}";

        await using var stream = file.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = _bucketName,
            Key = fileName,
            ContentType = file.ContentType,
            DisablePayloadSigning = true,
        };

        await fileTransferUtility.UploadAsync(uploadRequest);

        return fileName;
    }
    
    public async Task<string> UploadImageStorage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new ArgumentException("Invalid file type");

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        using var s3Client = new AmazonS3Client(_accessKey, _secretKey, s3Config);
        var fileTransferUtility = new TransferUtility(s3Client);

        var fileName = $"images/{Guid.NewGuid()}{extension}";

        await using var stream = file.OpenReadStream();

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            BucketName = _bucketName,
            Key = fileName,
            ContentType = file.ContentType,
            DisablePayloadSigning = true,
        };

        await fileTransferUtility.UploadAsync(uploadRequest);

        return fileName;
    }
    
    public string GenerateSignedUrl(string key, int expirationHours = 1)
    {
        var s3Config = new AmazonS3Config
        {
            ServiceURL = _endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        using var s3Client = new AmazonS3Client(_accessKey, _secretKey, s3Config);

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(expirationHours)
        };

        return s3Client.GetPreSignedURL(request);
    }
    
    public async Task DeleteFile(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var s3Config = new AmazonS3Config
        {
            ServiceURL = _endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        using var s3Client = new AmazonS3Client(_accessKey, _secretKey, s3Config);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await s3Client.DeleteObjectAsync(deleteRequest);
    }
}
