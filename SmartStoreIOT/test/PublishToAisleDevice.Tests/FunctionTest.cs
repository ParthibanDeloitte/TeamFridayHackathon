using System;
using System.Collections.Generic;
using Xunit;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.Lambda.TestUtilities;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PublishToAisleDevice;

namespace LambdaTest
{
    public class LambdaHandler
    {
        [Fact]
        public void handleRequest()
        {
            DynamoDBEvent evnt = new DynamoDBEvent
            {
                Records = new List<DynamoDBEvent.DynamodbStreamRecord>
                {
                    // Test Record for the user to enter the store
                    new DynamoDBEvent.DynamodbStreamRecord
                    {
                        AwsRegion = "us-east-1",
                        EventName = "MODIFY",
                        Dynamodb = new StreamRecord
                        {
                            ApproximateCreationDateTime = DateTime.Now,
                            Keys = new Dictionary<string, AttributeValue> { {"Id", new AttributeValue { N = "1" } } },
                            NewImage = new Dictionary<string, AttributeValue> { { "StoreLocation", new AttributeValue { S = "entrance" } } },
                            OldImage = new Dictionary<string, AttributeValue> { { "StoreLocation", new AttributeValue { S = "" } } },
                            StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
                        }
                    },
                    // Test Record for the user to move from entrance to aisle
                    new DynamoDBEvent.DynamodbStreamRecord
                    {
                        AwsRegion = "us-east-1",
                        EventName = "MODIFY",
                        Dynamodb = new StreamRecord
                        {
                            ApproximateCreationDateTime = DateTime.Now,
                            Keys = new Dictionary<string, AttributeValue> { {"Id", new AttributeValue { N = "2" } } },
                            NewImage = new Dictionary<string, AttributeValue> { { "StoreLocation", new AttributeValue { S = "aisle1" } } },
                            OldImage = new Dictionary<string, AttributeValue> { { "StoreLocation", new AttributeValue { S = "entrance" } } },
                            StreamViewType = StreamViewType.NEW_AND_OLD_IMAGES
                        }
                    }
                }
            };

            var context = new TestLambdaContext();
            var function = new Function();

            function.FunctionHandler(evnt, context);

            var testLogger = context.Logger as TestLambdaLogger;
			Assert.Contains("Stream processing complete", testLogger.Buffer.ToString());
        }  
    }
}
