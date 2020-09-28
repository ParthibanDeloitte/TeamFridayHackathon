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

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace LambdaRekognize
{
    public class SearchFacesFunction
    {
        public const string FACE_COLLECTION_ID = "TEAM_FRIDAY_CUSTOMER_FACES";

        IAmazonS3 S3Client { get; }

        IAmazonRekognition RekognitionClient { get; }

        HashSet<string> SupportedImageTypes { get; } = new HashSet<string> { ".png", ".jpg", ".jpeg" };

        public SearchFacesFunction()
        {
            this.S3Client = new AmazonS3Client(RegionEndpoint.USEast2);
            this.RekognitionClient = new AmazonRekognitionClient(RegionEndpoint.USEast2);

        }


        /// <summary>
        /// Constructor used for testing which will pass in the already configured service clients.
        /// </summary>
        /// <param name="s3Client"></param>
        /// <param name="rekognitionClient"></param>
        /// <param name="minConfidence"></param>
        public SearchFacesFunction(IAmazonS3 s3Client, IAmazonRekognition rekognitionClient)
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
            try
            {
                AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient();

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
                    if (!SupportedImageTypes.Contains(Path.GetExtension(record.S3.Object.Key)))
                    {
                        Debug.WriteLine($"Object {record.S3.Bucket.Name}:{record.S3.Object.Key} is not a supported image type");
                        continue;
                    }

                    Image image = new Image()
                    {
                        S3Object = new Amazon.Rekognition.Model.S3Object
                        {
                            Bucket = record.S3.Bucket.Name,
                            Name = record.S3.Object.Key
                        }
                    };

                    SearchFacesByImageRequest searchFacesByImageRequest = new SearchFacesByImageRequest()
                    {
                        CollectionId = FACE_COLLECTION_ID,
                        Image = image,
                        FaceMatchThreshold = 90F,
                        MaxFaces = 1
                    };

                    SearchFacesByImageResponse searchFacesByImageResponse = rekognitionClient.SearchFacesByImageAsync(searchFacesByImageRequest).Result;

                    Debug.WriteLine("Faces matching largest face in image from " + record.S3.Object.Key);
                    foreach (FaceMatch match in searchFacesByImageResponse.FaceMatches)
                    {
                        Debug.WriteLine("FaceId: " + match.Face.FaceId + ", Similarity: " + match.Similarity);

                        // Initialize the Amazon Cognito credentials provider
                        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
                            "us-east-1:6d711ae9-1084-4a71-9ef6-7551ca74ad0b", // Identity pool ID
                            RegionEndpoint.USEast1 // Region
                        );

                        var dynamoDbClient = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
                        Table customersTbl = Table.LoadTable(dynamoDbClient, "Customer");


                        GetItemOperationConfig config = new GetItemOperationConfig
                        {
                            AttributesToGet = new List<string> { "Id", "CustomerName", "FaceId", "StoreLocation" },
                            ConsistentRead = true
                        };

                        for (int i = 1; i <= 5; i++)
                        {
                            Document retrievedCustomer = customersTbl.GetItemAsync(i, config).Result;
                            if (retrievedCustomer["FaceId"].AsString() == match.Face.FaceId)//retrieved customer's faceID matches with the faceId currently being searched - we know who's in the store
                            {
                                //let us update customer's location
                                var customer = new Document();
                                customer["Id"] = Int32.Parse(retrievedCustomer["Id"].AsString());

                                string location = "";
                                string cameraFeedFolder = "CAM-Exit";
                                string[] cameraFeedFolderPath = record.S3.Object.Key.Split("/");
                                if (cameraFeedFolderPath.Length > 0)
                                    cameraFeedFolder = cameraFeedFolderPath[0];
                                switch (cameraFeedFolder)
                                {
                                    case "CAM-Entrance":
                                        location = "entrance";
                                        break;
                                    case "CAM-Aisle1":
                                        location = "aisle1";
                                        break;
                                    case "CAM-Aisle2":
                                        location = "aisle2";
                                        break;
                                    case "CAM-Aisle3":
                                        location = "aisle3";
                                        break;
                                    case "CAM-Aisle4":
                                        location = "aisle4";
                                        break;
                                    case "CAM-Checkout":
                                        location = "checkout";
                                        break;
                                    default:
                                        location = "entrance";
                                        break;
                                }

                                customer["StoreLocation"] = location;

                                // Optional parameters.
                                UpdateItemOperationConfig updateConfig = new UpdateItemOperationConfig
                                {
                                    // Get updated item in response.
                                    ReturnValues = ReturnValues.AllNewAttributes
                                };
                                Document updatedCustomer = customersTbl.UpdateItemAsync(customer, updateConfig).Result;
                                Console.WriteLine("UpdateMultipleAttributes: Printing item after updates ...");
                                PrintDocument(updatedCustomer);
                                break;
                            }
                        }

                    }
                }
                return;
            }
            catch(Exception ex)
            {
                throw new Exception("Deloitte Mart Exception " + ex.Message, ex);
            }
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
