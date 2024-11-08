using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using S3BucketwithAuth.Models.Contracts;
using S3BucketwithAuth.Services;
using System.Net.Mime;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace S3BucketwithAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private string[] permittedImageExtensions = { ".doc", ".docx", ".xls", ".xlsx" };
        private readonly IAmazonS3 _amazonS3;
        private readonly IOptions<S3BucketSettings> _s3Settings;
        private readonly ITokenService _tokenService;
        private readonly IBucketServices _BucketService;
        public DocumentsController(IAmazonS3 amazonS3, IOptions<S3BucketSettings> s3Settings, ITokenService tokenService, IBucketServices bucketService)
        {
            _amazonS3 = amazonS3;
            _s3Settings = s3Settings;
            _tokenService = tokenService;
            _BucketService = bucketService;

        }
        // GET: api/<DocumentsController>
        [Authorize]
        [HttpGet("ListDocumentsperUserDept")]
        public async Task<ActionResult<List<BucketListResponse>>> GetAsync()
        {
            string pref = _tokenService.GetUserDepartment();
            List<BucketListResponse> fileobjects = new List<BucketListResponse>();
            ListObjectsRequest request = new ListObjectsRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Prefix = pref
            };

            ListObjectsResponse response = await _amazonS3.ListObjectsAsync(request);
            foreach (S3Object obj in response.S3Objects)
            {
                string key = obj.Key; //Key
                //var fileid = key.Split('/').Last();
                string doctitle = await _BucketService.documenttitleAsync(key);
                fileobjects.Add(new BucketListResponse(key, doctitle, obj.Size, obj.LastModified) );
            }
            return Ok(fileobjects);
        }

        // GET api/<DocumentsController>/5
        [Authorize]
        [HttpPost("Get Presigned Url")]
        public IActionResult GetPresignedUrl([FromBody] FileKey key)
        {
            //string keyfolder = _tokenService.GetUserDepartment();
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = key.keyvalue,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(10)
            };
            string presignedurl = _amazonS3.GetPreSignedURL(request);
            return Ok(new { url = presignedurl });
        }

        [Authorize]
        [HttpPost("Download Files")]
        public async Task<IActionResult> DownloadDocumentFile([FromBody] FileKey key)
        {
            //string keyfolder = _tokenService.GetUserDepartment();
            var request = new GetObjectRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = key.keyvalue
            };
            var response = await _amazonS3.GetObjectAsync(request);
            return File(response.ResponseStream, response.Headers.ContentType, response.Metadata["file-name"]);
        }

        [Authorize]
        [HttpPost("File Upload")]
        public async Task<IActionResult> UploadNewImage(IFormFile file)
        {
            string keyfolder = string.Empty;

            if (file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                return BadRequest("The extension is invalid");
            }
            else
            {                
                using var stream = file.OpenReadStream();
                var key = Guid.NewGuid();
                if (permittedImageExtensions.Contains(extension))
                {
                    keyfolder = _tokenService.GetUserDepartment();
                }
                var putrequest = new PutObjectRequest
                {
                    BucketName = _s3Settings.Value.BucketName,
                    Key = $"{keyfolder}/{key}",
                    InputStream = stream,
                    ContentType = file.ContentType,
                    Metadata =
                    {
                        ["file-name"] = file.FileName
                    }
                };
                await _amazonS3.PutObjectAsync(putrequest);
                return Ok(key);
            }
        }

        [Authorize]
        [HttpPost("File Upload/Presigned")]
        public async Task<IActionResult> UploadFile(FileDocumentUploadRequest fileDocumentUpload)
        {
            try
            {
                var key = Guid.NewGuid();
                string keyfolder = _tokenService.GetUserDepartment();
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _s3Settings.Value.BucketName,
                    Key = $"{keyfolder}/{key}",
                    Verb = HttpVerb.PUT,
                    Expires = DateTime.UtcNow.AddMinutes(15),
                    ContentType = fileDocumentUpload.contentType,
                    Metadata =
            {
                ["file-name"] = fileDocumentUpload.filename
            }
                };

                string preSignedUrl = await _amazonS3.GetPreSignedURLAsync(request);

                return Ok(new { key, url = preSignedUrl });
            }
            catch (AmazonS3Exception ex)
            {
                return BadRequest($"S3 error generating pre-signed URL: {ex.Message}");
            }
        }

        // PUT api/<DocumentsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DocumentsController>/5
        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromBody] FileKey key)
        {
            //string keyfolder = _tokenService.GetUserDepartment();
            var getrequest = new DeleteObjectRequest
            {
                BucketName = _s3Settings.Value.BucketName,
                Key = key.keyvalue
            };
            await _amazonS3.DeleteObjectAsync(getrequest);
            return Ok($"File {key} deleted successfully");
        }
    }
}
