using Amazon.IotData;
using Amazon.IotData.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StoreDeviceSubscriber
{
    // Good Intro
    //https://iotbytes.wordpress.com/device-shadows-part-1-basics/
    public class Program
    {
        private static AmazonIotDataClient _iotClient;
        private static AmazonDynamoDBClient _dynamoDbClient;
        private static Table _recommendationsTable;
        private static string tableName = "Recommendations";
        private static string AisleIdIdentifier = "AisleId";
        private static string ItemIdentifier = "Item";
        private static string HighlightIdentifier = "Highlight";
        private static string RestockIdentifier = "Restock";
        private static string CustomersIdentifier = "Customers";
        private static string[] _devices;
        private static Dictionary<string, string> _responses;
        private static ManualResetEvent manualResetEvent;

        static void Main(string[] devices)
        {
            //if (devices != null)
            //    _devices = devices;
            //else
                _devices = new string[] { "store1_aisle1", "store1_aisle2", "store1_aisle3", "store1_aisle4" };
            
            _responses = new Dictionary<string, string>(_devices.Length);
            _iotClient = new AmazonIotDataClient("https://data.iot.us-east-1.amazonaws.com/");
            _dynamoDbClient = new AmazonDynamoDBClient();
            _recommendationsTable = Table.LoadTable(_dynamoDbClient, tableName);

            Thread thread = new Thread(KeepGettingShadows);
            thread.Start();

            // Keep the main thread alive for the event receivers to get invoked
            KeepConsoleAppRunning(() => {
                Console.WriteLine("Disconnecting client..");
            });
        }

        private static void KeepGettingShadows()
        {
            while (true) 
            {
                foreach (string device in _devices)
                {
                    Console.WriteLine($"Getting shadow for {device}");
                    GetThingShadowRequest updateRequest = new GetThingShadowRequest();
                    updateRequest.ThingName = device;
                    Task<GetThingShadowResponse> response = null;
                    try
                    {
                        response = _iotClient.GetThingShadowAsync(updateRequest, new System.Threading.CancellationToken());
                        response.Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"No shadow found for {device}. The following error occured: {ex.Message}");
                        continue;
                    }
                    
                    string updatedResponse = null;
                    if ((response.Result != null) && (response.Result.Payload != null)) 
                    {
                        //Console.WriteLine($"Reading Payload from Response");
                        using (StreamReader reader = new StreamReader(response.Result.Payload))
                            updatedResponse = reader.ReadToEnd();

                        var json = JObject.Parse(updatedResponse);
                        string eventType = json.SelectToken("state.reported.type").Value<string>().Trim();
                        string itemsStr = json.SelectToken("state.reported.items").Value<string>().Trim();
                        string aisleIdentifier = device.Substring(7).Trim(); //removing "store1_"
                        int customerId = int.Parse(json.SelectToken("state.reported.customerId").Value<string>().Trim());
                        string newImage = $"{eventType}{itemsStr}{customerId}";

                        //Console.WriteLine($"Payload read: {updatedResponse}");
                        if (!string.IsNullOrWhiteSpace(updatedResponse))
                        {
                            if (!_responses.ContainsKey(device))
                            {
                                _responses.Add(device, newImage);
                                UpdateDynamoDB(eventType, aisleIdentifier, itemsStr, customerId);
                                Console.WriteLine($"The aisle {device} state got changed to {updatedResponse}");
                            }
                            else if (string.Compare(newImage, _responses[device], StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                Console.WriteLine($"New Time Stamp {newImage}");
                                Console.WriteLine($"Old Time Stamp {_responses[device]}");
                                _responses[device] = newImage;
                                UpdateDynamoDB(eventType, aisleIdentifier, itemsStr, customerId);
                                Console.WriteLine($"The aisle {device} state got changed to {updatedResponse}");
                            }
                            else
                            {
                                Console.WriteLine($"No updates for {device}");
                            }
                        }
                    }
                }
                Thread.Sleep(30000);
            }
        }

        private static void UpdateDynamoDB(string type, string aisleIdentifier, string itemsStr, int customerId)
        {
            Document document = null;
            if ((string.IsNullOrWhiteSpace(aisleIdentifier)) || (string.IsNullOrWhiteSpace(itemsStr)) || (customerId < 1))
            {
                Console.WriteLine($"Incorrect event type({type}), data aisle({aisleIdentifier}), items({itemsStr}), cusomter({customerId})");
                return;
            }
            
            string[] items = itemsStr.Split(new char[] { ',' });
            for (int j=0; j<items.Length; j++)
                items[j] = items[j].Trim();

            switch (type)
            {
                case "TurnOff":
                    foreach (string item in items) 
                    {
                        document = GetRecommendation(aisleIdentifier, item);
                        if ((document != null) && (!string.IsNullOrWhiteSpace(document[AisleIdIdentifier].AsString())))
                        {
                            DynamoDBEntry customersValue = document[CustomersIdentifier];
                            if (customersValue is PrimitiveList)
                            {
                                List<Primitive> customerIds = customersValue.AsPrimitiveList().Entries;

                                bool containsCustomer = false;
                                foreach (Primitive cusId in customerIds)
                                {
                                    if (cusId.AsInt() == customerId)
                                    {
                                        containsCustomer = true;
                                        break;
                                    }
                                }

                                // If this is the only customer interested in that recommendation, delete else update
                                if (containsCustomer) 
                                {
                                    if (customerIds.Count <= 1)
                                    {
                                        DeleteRecommendation(document);
                                    }
                                    else
                                    {
                                        for (int k=customerIds.Count - 1; k > -1; k--)
                                        {
                                            if (customerIds[k].AsInt() == customerId)
                                            {
                                                customerIds.RemoveAt(k);
                                                break;
                                            }
                                        }
                                        document[CustomersIdentifier] = customerIds;
                                        UpdateRecommendation(document);
                                    }
                                }
                            }
                        }
                    }
                    break;
                case "Highlight":
                case "Restock":
                    foreach (string item in items)
                    {
                        document = GetRecommendation(aisleIdentifier, item);
                        if ((document != null) && (!string.IsNullOrWhiteSpace(document[AisleIdIdentifier].AsString())))
                        {
                            Console.WriteLine("Updating recommendation");
                            Console.WriteLine($"Event type({type}), data aisle({aisleIdentifier}), items({item}), cusomter({customerId})");
                            if (type == "Highlight")
                                document[HighlightIdentifier] = new DynamoDBBool(true);
                            else
                                document[RestockIdentifier] = new DynamoDBBool(true);

                            DynamoDBEntry customersValue = document[CustomersIdentifier];
                            if (customersValue is PrimitiveList)
                            {
                                List<Primitive> customerIds = customersValue.AsPrimitiveList().Entries;
                                if (!customerIds.Contains(customerId))
                                    customersValue.AsPrimitiveList().Add(customerId);
                                document[CustomersIdentifier] = customersValue;
                            }
                            UpdateRecommendation(document);
                        }
                        else
                        {
                            Console.WriteLine("Creating recommendation");
                            Console.WriteLine($"Event type({type}), data aisle({aisleIdentifier}), items({item}), cusomter({customerId})");
                            if (type == "Highlight")
                                CreateEntry(aisleIdentifier, item, customerId, true, false);
                            else
                                CreateEntry(aisleIdentifier, item, customerId, false, true);
                        }
                    }
                    break;
            }
        }

        private static void KeepConsoleAppRunning(Action onShutdown)
        {
            manualResetEvent = new ManualResetEvent(false);
            Console.WriteLine("Press CTRL + C or CTRL + Break to exit...");

            Console.CancelKeyPress += (sender, e) =>
            {
                onShutdown();
                e.Cancel = true;
                manualResetEvent.Set();
                Environment.Exit(0);
            };

            manualResetEvent.WaitOne();
        }

        // Creates a sample book item.
        private static void CreateEntry(string aisleIdentifier, string item, int customerId, bool highlight, bool restock)
        {
            var recommendation = new Document();
            recommendation[AisleIdIdentifier] = aisleIdentifier;
            recommendation[ItemIdentifier] = item;
            recommendation[HighlightIdentifier] = new DynamoDBBool(highlight);
            recommendation[RestockIdentifier] = new DynamoDBBool(restock);
            recommendation[CustomersIdentifier] = new List<string> { customerId.ToString() };
            
            _recommendationsTable.PutItemAsync(recommendation);
        }

        private static Document GetRecommendation(string aisleId, string item)
        {
            // Optional configuration.
            GetItemOperationConfig config = new GetItemOperationConfig
            {
                AttributesToGet = new List<string> { AisleIdIdentifier, ItemIdentifier, HighlightIdentifier, RestockIdentifier, CustomersIdentifier },
                ConsistentRead = true
            };

            //Console.WriteLine($"Fetching record for {aisleId} and {item}");
            Task<Document> documentTask = _recommendationsTable.GetItemAsync(new Primitive { Type = DynamoDBEntryType.String, Value = aisleId }, 
                new Primitive { Type = DynamoDBEntryType.String, Value = item }, config);
            documentTask.Wait();
            
            return documentTask.Result;
        }

        private static void UpdateRecommendation(Document updatedDocument)
        {
            // Optional parameters.
            UpdateItemOperationConfig config = new UpdateItemOperationConfig
            {
                // Get updated item in response.
                ReturnValues = ReturnValues.None,
            };
            _recommendationsTable.UpdateItemAsync(updatedDocument, config);
        }

        private static void DeleteRecommendation(Document document)
        {
            DeleteItemOperationConfig config = new DeleteItemOperationConfig
            {
                ReturnValues = ReturnValues.AllOldAttributes
            };

            //Console.WriteLine($"Turning off {document[ItemIdentifier]} from {document[AisleIdIdentifier]}");
            var task = _recommendationsTable.DeleteItemAsync(document);
            task.Wait();
        }
    }
}
