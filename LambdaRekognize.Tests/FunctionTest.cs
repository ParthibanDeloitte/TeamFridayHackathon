using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using Amazon.Rekognition;

using LambdaRekognize;
using Amazon.Lambda.S3Events;

namespace LambdaRekognize.Tests
{
    public class FunctionTest
    {
        /*
        [Fact]
        public async Task Test_LabelImage()
        {
            const string fileName = "test.jpg";
            const string s3Key = "CAM-Entrance/test.jpg";
            IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast2);
            IAmazonRekognition rekognitionClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);

            var bucketName = "store-input-feed-test" + DateTime.Now.Ticks.ToString();
            await s3Client.PutBucketAsync(bucketName); 
            try
            {
                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = fileName,
                    Key = s3Key
                });

                // Setup the S3 event object that S3 notifications would create and send to the Lambda function if
                // the bucket was configured as an event source.
                var s3Event = new S3Event
                {
                    Records = new List<S3EventNotification.S3EventNotificationRecord>
                    {
                        new S3EventNotification.S3EventNotificationRecord
                        {
                            S3 = new S3EventNotification.S3Entity
                            {
                                Bucket = new S3EventNotification.S3BucketEntity {Name = bucketName },
                                Object = new S3EventNotification.S3ObjectEntity {Key = s3Key }
                            }
                        }
                    }
                };

                // Use test constructor for the function with the service clients created for the test
                var function = new LabelImageFunction(s3Client, rekognitionClient, LabelImageFunction.DEFAULT_MIN_CONFIDENCE);

                var context = new TestLambdaContext();
                await function.FunctionHandler(s3Event, context);

                var getTagsResponse = await s3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
                {
                    BucketName = bucketName,
                    Key = s3Key
                });

                Assert.True(getTagsResponse.Tagging.Count > 0);
            }
            finally
            {
                // Clean up the test data
               // await AmazonS3Util.DeleteS3BucketWithObjectsAsync(s3Client, bucketName);
            }
        }

        [Fact]
        public async Task Test_AddToFacesCollection()
        {
            const string fileName = "subject05.jpg";
            const string s3Key = "subject05.jpg";
            IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast2);
            IAmazonRekognition rekognitionClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);

            var bucketName = "customer-faces";
            //await s3Client.PutBucketAsync(bucketName);
            try
            {
                var tags = new List<Tag>();
                tags.Add(new Tag { Key = "CustomerID", Value = "5" });

                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = fileName,
                    Key = s3Key,
                    TagSet = tags
                });

                // Setup the S3 event object that S3 notifications would create and send to the Lambda function if
                // the bucket was configured as an event source.
                var s3Event = new S3Event
                {
                    Records = new List<S3EventNotification.S3EventNotificationRecord>
                    {
                        new S3EventNotification.S3EventNotificationRecord
                        {
                            S3 = new S3EventNotification.S3Entity
                            {
                                Bucket = new S3EventNotification.S3BucketEntity {Name = bucketName },
                                Object = new S3EventNotification.S3ObjectEntity {Key = s3Key }
                            }
                        }
                    }
                };

                // Use test constructor for the function with the service clients created for the test
                var function = new CreateFaceCollectionFunction(s3Client, rekognitionClient);

                var context = new TestLambdaContext();
                await function.FunctionHandler(s3Event, context);

                //TODO: Assert to make sure the image was added to the collection
            }
            finally
            {
                // Clean up the test data
                // await AmazonS3Util.DeleteS3BucketWithObjectsAsync(s3Client, bucketName);
            }
        }
        */

        [Fact]
        public async Task Test_SearchFacesCollection()
        {
            const string fileName = "subject02.jpg";
            const string s3Key = "CAM-Entrance/subject02.jpg";
            IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast2);
            IAmazonRekognition rekognitionClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);

            var bucketName = "store-input-feed";// -test" + DateTime.Now.Ticks.ToString();
            //await s3Client.PutBucketAsync(bucketName);
            try
            {
                await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = fileName,
                    Key = s3Key                    
                });

                // Setup the S3 event object that S3 notifications would create and send to the Lambda function if
                // the bucket was configured as an event source.
                var s3Event = new S3Event
                {
                    Records = new List<S3EventNotification.S3EventNotificationRecord>
                    {
                        new S3EventNotification.S3EventNotificationRecord
                        {
                            S3 = new S3EventNotification.S3Entity
                            {
                                Bucket = new S3EventNotification.S3BucketEntity {Name = bucketName },
                                Object = new S3EventNotification.S3ObjectEntity {Key = s3Key }
                            }
                        }
                    }
                };

                // Use test constructor for the function with the service clients created for the test
                var function = new SearchFacesFunction(s3Client, rekognitionClient);

                var context = new TestLambdaContext();
                await function.FunctionHandler(s3Event, context);

                //TODO: Assert to make sure the image was added to the collection
            }
            finally
            {
                // Clean up the test data
                //await AmazonS3Util.DeleteS3BucketWithObjectsAsync(s3Client, bucketName);
            }
        }
    }
}
