using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;


using Amazon.Rekognition;
using Amazon.Rekognition.Model;


using Amazon.S3;
using Amazon.S3.Model;

namespace LambdaRekognize
{
    public class CreateFaceCollectionFunction
    {
        public const string FACE_COLLECTION_ID = "TEAM_FRIDAY_CUSTOMER_FACES";

        IAmazonS3 S3Client { get; }

        IAmazonRekognition RekognitionClient { get; }

        HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };
        private static AmazonDynamoDBClient dynamoDbClient = null;


        /// <summary>
        /// Constructor used for testing which will pass in the already configured service clients.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="rekognitionClient"></param>
        /// <param name="minConfidence"></param>
        public CreateFaceCollectionFunction(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient)
        {
            this.S3Client = s3Client;
            this.RekognitionClient = rekognitionClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task FunctionHandler(S3Event input, ILambdaContext context)
        {
            // Initialize the Amazon Cognito credentials provider
            CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                "us-east-1:6d711ae9-1084-4a71-9ef6-7551ca74ad0b", // Identity pool ID
                RegionEndpoint.USEast1 // Region
            );

            dynamoDbClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            Table customersTbl = Table.LoadTable(dynamoDbClient, "Customer");


            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();
            IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USEast2);

            //Debug.WriteLine("Creating collection: " + FACE_COLLECTION_ID);
            //CreateCollectionRequest createCollectionRequest = new CreateCollectionRequest()
            //{
            //    CollectionId = FACE_COLLECTION_ID
            //};

            //CreateCollectionResponse createCollectionResponse = rekognitionClient.CreateCollectionAsync(createCollectionRequest).Result;
            //Debug.WriteLine("CollectionArn : " + createCollectionResponse.CollectionArn);
            //Debug.WriteLine("Status code : " + createCollectionResponse.StatusCode);


            foreach (var record in input.Records)
            {
                //if(!SupportedImageTypes.Contains(Path.GetExtension(record.S3.Object.Key)))
                //{
                //    Debug.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
                //    continue;
                //}

                Image image = new Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object
                    {
                        Bucket = record.S3.Bucket.Name,
                        Name = record.S3.Object.Key
                    }
                };

                GetObjectTaggingResponse  taggingResponse = s3Client.GetObjectTaggingAsync(
                    new GetObjectTaggingRequest
                    {
                        BucketName = record.S3.Bucket.Name,
                        Key = record.S3.Object.Key
                    }
                    ).Result;

                Tag customerID = taggingResponse.Tagging[0];//TODO: HARDCODING!!
                

                IndexFacesRequest indexFacesRequest = new IndexFacesRequest()
                {
                    Image = image,
                    CollectionId = FACE_COLLECTION_ID,
                    ExternalImageId = record.S3.Object.Key,
                    DetectionAttributes = new List<String>() { "ALL" }
                };

                IndexFacesResponse indexFacesResponse = rekognitionClient.IndexFacesAsync(indexFacesRequest).Result;
                               
                Debug.WriteLine(record.S3.Object.Key + " added");
                foreach (FaceRecord faceRecord in indexFacesResponse.FaceRecords)
                {
                    Debug.WriteLine("Face detected: Faceid is " + faceRecord.Face.FaceId);

                    Console.WriteLine("\nAfter Indexing, Updating FaceID of the Customer....");
                    string partitionKey = customerID.Value;

                    var customer = new Document();
                    customer["Id"] = Int32.Parse( partitionKey );
                    // List of attribute updates.
                    // The following replaces the existing authors list.
                    customer["FaceId"] = faceRecord.Face.FaceId;

                    // Optional parameters.
                    UpdateItemOperationConfig config = new UpdateItemOperationConfig
                    {
                        // Get updated item in response.
                        ReturnValues = ReturnValues.AllNewAttributes
                    };
                    Document updatedCustomer= customersTbl.UpdateItemAsync(customer, config).Result;
                    Console.WriteLine("UpdateMultipleAttributes: Printing item after updates ...");
                    PrintDocument(updatedCustomer);
                }
            }
            return;
        }

        private static void PrintDocument(Document updatedDocument)
        {
            foreach (var attribute in updatedDocument.GetAttributeNames())
            {
                string stringValue = null;
                var value = updatedDocument[attribute];
                if (value is Primitive)
                    stringValue = value.AsPrimitive().Value.ToString();
                else if (value is PrimitiveList)
                    stringValue = string.Join(",", (from primitive
                                    in value.AsPrimitiveList().Entries
                                                    select primitive.Value).ToArray());
                Debug.WriteLine("{0} - {1}", attribute, stringValue);
            }
        }
    }
}
