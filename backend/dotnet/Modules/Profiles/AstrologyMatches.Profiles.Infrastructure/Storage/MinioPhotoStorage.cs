using AstrologyMatches.Profiles.Application.Storage;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace AstrologyMatches.Profiles.Infrastructure.Storage;

public class MinioPhotoStorage : IPhotoStorage
{
    private readonly IMinioClient _minioClient;
    private readonly MinioOptions _options;

    public MinioPhotoStorage(IMinioClient minioClient, IOptions<MinioOptions> options)
    {
        _minioClient = minioClient;
        _options = options.Value;
    }

    public async Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var putArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putArgs, cancellationToken);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
    {
        var removeArgs = new RemoveObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey);

        await _minioClient.RemoveObjectAsync(removeArgs, cancellationToken);
    }

    public async Task<string> GetDownloadUrlAsync(string objectKey)
    {
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectKey)
            .WithExpiry(_options.UrlLifetimeMinutes * 60);

        return await _minioClient.PresignedGetObjectAsync(presignedArgs);
    }

    // Bucket создаётся при первой загрузке, чтобы на новой машине не настраивать MinIO руками
    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        bool bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName), cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_options.BucketName), cancellationToken);
        }
    }
}
