"# S3BucketWebApp"

#**Features**
The app is allow you to upload files to an S3 bucket, display files, delete files and also preview the document via a pre-signed url link. The Documents controller has token based authentication setup.

#**Prerequisites**
    The app is composed of a backend and front-end. Before you run the front-end app (Bucket UI), ensure you modify the urls in the BucketUI/config.js file to the url you are running your application on.

 In order to run the backend app:

  1. AWS Configs
  - Ensure the AWS CLI is setup on your local machine; refer to: https://docs.aws.amazon.com/streams/latest/dev/setup-awscli.html
  - As part of the configuration setup in the above step ensure you have an IAM user with AmazonS3FullAccess permissions set.
  - Create a bucket and fill in the details of the region and name in the S3BucketwithAuth/appsettings.Development.json file.
  - Enable CORS on your bucket's configuration. Refer to the page: https://docs.aws.amazon.com/AmazonS3/latest/userguide/ManageCorsUsing.html
  Sample: 
    ``` [
        {
            "AllowedHeaders": [
                "*"
            ],
            "AllowedMethods": [
                "PUT",
                "POST",
                "DELETE"
            ],
            "AllowedOrigins": [
                "http://www.example.com"
            ],
            "ExposeHeaders": [
                "x-amz-server-side-encryption",
                "x-amz-request-id",
                "x-amz-id-2"
            ],
            "MaxAgeSeconds": 3000
        }
    ] ```

  2. Setup JWT
   - Configure the S3BucketwithAuth/appsettings.json with your preferred values.

  3. Users
  Configure the list of users S3BucketwithAuth/Services/UserService.cs to your preference including their departments listed in S3BucketwithAuth/Models/Enums/Departments.cs. The bucket is prefixed based on the user department.