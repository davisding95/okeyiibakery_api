using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace cakeshop_api.Services
{
    public class CloudflareR2Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string? _bucketName;

        public CloudflareR2Service(IConfiguration configuration)
        {
            var accessKey = configuration["CloudflareR2:AccessKey"];
            var secretKey = configuration["CloudflareR2:SecretKey"];
            _bucketName = configuration["CloudflareR2:BucketName"];
            var endpoint = configuration["CloudflareR2:Endpoint"];

            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            var s3Config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true,
                SignatureVersion = "4",
                RequestChecksumCalculation = RequestChecksumCalculation.WHEN_REQUIRED, // when use s3 sdk 3.7 or above
                ResponseChecksumValidation = ResponseChecksumValidation.WHEN_REQUIRED,  // when use s3 sdk 3.7 or above
            };

            _s3Client = new AmazonS3Client(credentials, s3Config);
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead, // Ensure the file is publicly readable
                DisablePayloadSigning = true  // Do as the documentation says
            };

            await _s3Client.PutObjectAsync(request);

            // Change to the first one after registering domain
            // return $"http://{_bucketName}.r2.cloudflarestorage.com/{fileName}";
            return $"https://pub-30093c32a1b34bbba5e4cc060ce37e54.r2.dev/{fileName}";
        }

        public async Task DeleteFileAsync(string imageUrl)
        {
            var fileName = imageUrl.Split('/').Last();
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileName
            };

            await _s3Client.DeleteObjectAsync(request);
        }
    }
}