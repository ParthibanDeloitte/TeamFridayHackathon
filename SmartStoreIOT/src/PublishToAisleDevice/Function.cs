using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Amazon.DynamoDBv2.Model;
using Amazon.IotData;
using Amazon.IotData.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using static Amazon.Lambda.DynamoDBEvents.DynamoDBEvent;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PublishToAisleDevice
{
    /// <summary>
    /// 
    /// </summary>
    public class Function
    {
        private AmazonIotDataClient _client;
        private const string IdIdentifier = "Id";
        private const string LocationIdentifier = "StoreLocation";
        private const string NameIdentifier = "CustomerName";
        private const string Entrance = "entrance";
        private const string Aisle = "aisle";
        private const string Checkout = "checkout";
        private const string Exit = "exit";

        public Function()
        {
            _client = new AmazonIotDataClient("https://data.iot.us-east-1.amazonaws.com/");

            InitializeTestData();
        }

        // LambdaTest::LambdaTest.LambdaHandler::handleRequest
        // PublishToAisleDevice::PublishToAisleDevice.Function::FunctionHandler
        public void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process customer updates for {dynamoEvent.Records.Count} record(s)...");
            foreach (var record in dynamoEvent.Records)
            {
                context.Logger.LogLine($"---------------------------------------------");
                context.Logger.LogLine($"User information updated;");
                switch (record.EventName)
                {
                    case "INSERT":
                        // Do we need to do anything here for new customer registration?
                        // Handle new customer registration. May be he entered the store and registered
                        break;
                    case "MODIFY":
                        SendUpdatesToStore(record, context);
                        break;
                    case "DELETE":
                    // Do we need to do anything here for new customer un-registration?
                    default:
                        break;
                }
                
				
				// TODO: Add business logic processing the record.Dynamodb object.
            }

            context.Logger.LogLine($"---------------------------------------------");
            context.Logger.LogLine("Stream processing complete.");
        }

        
        private void SendUpdatesToStore(DynamodbStreamRecord record, ILambdaContext context)
        {
            // We need to extract Person get his purchase history and come up with payload using Machine Learning
            // It should also have the device name to be published to 
            AttributeValue customerIdValueAttr = record.Dynamodb.Keys[IdIdentifier];
            AttributeValue newLocationAttr = (record.Dynamodb.NewImage != null) ? record.Dynamodb.NewImage[LocationIdentifier] : null;
            AttributeValue oldLocationAttr = (record.Dynamodb.OldImage != null) ? record.Dynamodb.OldImage[LocationIdentifier] : null;

            int customerId = Convert.ToInt32(customerIdValueAttr.N);
            string newLocation = newLocationAttr?.S;
            string oldLocation = oldLocationAttr?.S;

            context.Logger.LogLine($"CustomerId:{customerId};OldLocation:{oldLocation};NewLocation:{newLocation};");
            if (string.Compare(oldLocation, newLocation, true) != 0)
            {
                if ((string.IsNullOrWhiteSpace(oldLocation)) && (newLocation == Entrance))
                {
                    context.Logger.LogLine($"Customer entered the store. Updating Missing stock information.");
                    UpdateMissingStockInformation(customerId, context);
                }
                else if (newLocation.StartsWith(Aisle))
                {
                    context.Logger.LogLine($"Customer location updated from {oldLocation} to {newLocation}.");
                    // Send highlight signal for new location and clear signal to old location
                    if (newLocation.StartsWith(Aisle))
                    {
                        List<string> customerPreferences = GetCustomerPreferences(customerId, newLocation);
                        NotifyAisle(customerId, newLocation, customerPreferences, "Highlight");

                        string allPrefs = string.Empty;
                        foreach (string preference in customerPreferences)
                            allPrefs += (preference + ", ");
                        context.Logger.LogLine($"Notifcations Sent to {newLocation} to highlight items of customer recommendations: {allPrefs}.");
                    }
                    if (oldLocation.StartsWith(Aisle))
                    {
                        NotifyAisle(customerId, oldLocation, GetCustomerPreferences(customerId, oldLocation), "TurnOff");
                        context.Logger.LogLine($"Notifcations Sent to {oldLocation} to stop highlighting items customer recommendations.");
                    }
                }
            }
        }

        /// <summary>
        /// Notifies the Aisle about the state it needs to update
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="location"></param>
        /// <param name="isLeaving"></param>
        private void NotifyAisle(int customerId, string location, List<string> items, string status)
        {
            if (string.IsNullOrWhiteSpace(location))
                return;

            UpdateDevice(location, PreparePayLoad(customerId, location, items, status));
        }

        private void UpdateDevice(string location, string payload)
        {
            UpdateThingShadowRequest updateRequest = new UpdateThingShadowRequest();
            updateRequest.Payload = new MemoryStream(Encoding.UTF8.GetBytes(payload));

            // "store1_aisle1"
            updateRequest.ThingName = GetIotDeviceName(location);

            var response = _client.UpdateThingShadowAsync(updateRequest, new System.Threading.CancellationToken());
            response.Wait();
        }

        private void UpdateMissingStockInformation(int customerId, ILambdaContext context)
        {
            Dictionary<string, List<string>> missingCustomerRecommendation = GetMissingStock(customerId);
            foreach (string aisle in missingCustomerRecommendation.Keys)
                UpdateDevice(aisle, PreparePayLoad(customerId, aisle, missingCustomerRecommendation[aisle], "Restock"));
            context.Logger.LogLine($"Please refill following stock items for customer({customerId}): {GetString(GetMissingStock(customerId))}");
        }

        private static string GetString(Dictionary<string, List<string>> dictionary)
        {
            StringBuilder sb = new StringBuilder();
            bool firstPass = true;
            foreach (string aisle in dictionary.Keys)
            {
                if (!firstPass)
                    sb.Append("; ");

                sb.Append(aisle);
                sb.Append(": ");
                
                bool itemFirstPass = true;
                foreach (string item in dictionary[aisle])
                {
                    if (!itemFirstPass)
                        sb.Append(", ");
                    sb.Append(item);
                    itemFirstPass = false;
                }
                firstPass = false;
            }

            return sb.ToString();
        }

        private string GetIotDeviceName(string location)
        {
            // This method assumes the IotDevices are named as follows
            // store1_entrance
            // store1_aisle1, store1_aisle2, store1_aisle3.....store1_aisleN
            // store1_checkout
            // store1_exit
            return $"store1_{location}";
        }

        public string PreparePayLoad(int customerId, string location, List<string> items, string status)
        {
            string typeStr = "{ \"type\" : \"" + status + "\", ";
            string customerIdStr = "\"customerId\" : \"" + customerId + "\", ";

            bool firstPass = true;
            string itemsStr = "\"items\" : \"";
            foreach (string item in items)
            {
                if (!firstPass)
                    itemsStr += ", ";
                itemsStr += item;
                firstPass = false;
            }
            itemsStr += "\" }";

            return "{ \"state\" : { \"reported\" : " + typeStr + customerIdStr + itemsStr + " } }";
        }

        private Dictionary<string, List<string>> GetMissingStock(int customerId)
        {
            Dictionary<string, List<string>> notifyAllItems = new Dictionary<string, List<string>>();

            List<string> customerPreferences = GetCustomerPreferences(customerId);
            List<string> notifyAisleItems;
            foreach (string aisle in _missingItems.Keys)
            {
                string[] aisleMissingItems = _missingItems[aisle];

                notifyAisleItems = new List<string>();
                foreach (string item in aisleMissingItems)
                {
                    if (customerPreferences.Contains(item))
                        notifyAisleItems.Add(item);
                }
                if (notifyAisleItems.Count > 0)
                    notifyAllItems.Add(aisle, notifyAisleItems);
            }
            return notifyAllItems;
        }

        #region TestData
        // TODO: This should come from actual store database and Machine Learning logic to get customer preferences
        Dictionary<string, string[]> _aisleItems;
        Dictionary<string, string[]> _missingItems;
        private void InitializeTestData()
        {
            InitializeAisleItems();
            InitializeMissingStock();
        }

        private List<string> GetCustomerPreferences(int customerId)
        {
            List<string> allItems = new List<string>();
            allItems.AddRange(GetCustomerPreferences(customerId, "aisle1"));
            allItems.AddRange(GetCustomerPreferences(customerId, "aisle2"));
            allItems.AddRange(GetCustomerPreferences(customerId, "aisle3"));
            allItems.AddRange(GetCustomerPreferences(customerId, "aisle4"));
            return allItems;
        }

        // Function that always returns same items from every aisle for each customer
        // Assumes there are 4 items on each aisle
        private List<string> GetCustomerPreferences(int customerId, string aisleIdentifier)
        {
            if ((string.IsNullOrWhiteSpace(aisleIdentifier)) || (!_aisleItems.ContainsKey(aisleIdentifier)))
                return new List<string>();

            // Adds the customer number and aisle number
            int endNumber = customerId + Int32.Parse(aisleIdentifier.ToString()[aisleIdentifier.ToString().Length - 1].ToString());

            // each aisle contain 4 items, so remainder will get indexer from 0-3
            int startItem = endNumber % 4;

            // if sum of customerid and aisle number is even number, then we will pick 2 consecutive items, else alternate items
            int nextItem = startItem + ((endNumber % 2 == 0) ? 1 : 2);

            // reset indexeer if greater than 3, depending upon startItem
            if (nextItem > 3)
                nextItem = (nextItem == 4) ? 0 : 1;

            // Get the items for particular aisle
            string[] aisleItems = _aisleItems[aisleIdentifier];
            return new List<string>() { aisleItems[startItem], aisleItems[nextItem] };
        }

        private void InitializeAisleItems()
        {
            _aisleItems = new Dictionary<string, string[]>();
            _aisleItems.Add("aisle1", new string[4] { "tomatos", "lettuce", "apples", "oranges" });
            _aisleItems.Add("aisle2", new string[4] { "chips", "oreos", "bars", "candies" });
            _aisleItems.Add("aisle3", new string[4] { "sugar", "salt", "flour", "pasta" });
            _aisleItems.Add("aisle4", new string[4] { "milk", "cheese", "cake", "bread" });
        }

        private void InitializeMissingStock()
        {
            _missingItems = new Dictionary<string, string[]>();
            _missingItems.Add("aisle2", new string[1] { "bars"});
            _missingItems.Add("aisle3", new string[1] { "pasta" });
            _missingItems.Add("aisle4", new string[2] { "cheese", "bread" });
        }

        private string[] GetAisleItems(string aisleIdentifier)
        {
            if (_aisleItems.ContainsKey(aisleIdentifier))
                return _aisleItems[aisleIdentifier];
            return null;
        }
        #endregion
    }
}