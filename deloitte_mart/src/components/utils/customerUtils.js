import AWS from 'aws-sdk';

export class cusomterUtils { 

    static getDDB()
    {
        var AWS = require('aws-sdk');
        var myCredentials = new AWS.CognitoIdentityCredentials({IdentityPoolId:'us-east-1:6d711ae9-1084-4a71-9ef6-7551ca74ad0b'});
        var myConfig = new AWS.Config({
        credentials: myCredentials, region: 'us-east-1'
        });
 
        // Set the region 
        AWS.config.update(myConfig);
 
         // Create DynamoDB service object
        return  new AWS.DynamoDB({apiVersion: '2012-08-10'});
    }

    static getCustomerRecommendations = (updateCustomerRecommendations) => {
        var ddb = this.getDDB();

        var params = {
            RequestItems: {
            'Recommendations': {
             "Keys": [
                 {"AisleId":{"S":"aisle1"}, "Item": {"S":"tomatos"} },
                 {"AisleId":{"S":"aisle1"}, "Item": {"S":"lettuce"}},
                 {"AisleId":{"S":"aisle1"}, "Item": {"S":"apples"}},
                 {"AisleId":{"S":"aisle1"}, "Item": {"S":"oranges"}},

                 {"AisleId":{"S":"aisle2"}, "Item": {"S":"chips"} },
                 {"AisleId":{"S":"aisle2"}, "Item": {"S":"oreos"}},
                 {"AisleId":{"S":"aisle2"}, "Item": {"S":"bars"}},
                 {"AisleId":{"S":"aisle2"}, "Item": {"S":"candies"}},  
                 
                 {"AisleId":{"S":"aisle3"}, "Item": {"S":"sugar"} },
                 {"AisleId":{"S":"aisle3"}, "Item": {"S":"salt"}},
                 {"AisleId":{"S":"aisle3"}, "Item": {"S":"flour"}},
                 {"AisleId":{"S":"aisle3"}, "Item": {"S":"pasta"}},  


                 {"AisleId":{"S":"aisle4"}, "Item": {"S":"milk"} },
                 {"AisleId":{"S":"aisle4"}, "Item": {"S":"cheese"}},
                 {"AisleId":{"S":"aisle4"}, "Item": {"S":"cake"}},
                 {"AisleId":{"S":"aisle4"}, "Item": {"S":"bread"}},  

                 ],
                "ProjectionExpression": 'AisleId, #I, Customers, Highlight, Restock',
                "ExpressionAttributeNames": {"#I":"Item"}
             }
            }
        };
 
        ddb.batchGetItem(params, function(err, data) {
            var recosByAisle = [];
            if (err) {
                //debugger;
                console.log("Error", err);
            } else {
                //debugger;
                data.Responses.Recommendations.forEach(function(element, index, array) {

                    var cust_ids = element.Customers.SS;
                    var cust_names = [];
                    for(var i=0;i<cust_ids.length;i++)
                    {
                        cust_names.push("subject0" + cust_ids[i]);
                    }

                    //debugger;
                    recosByAisle.push(
                        {
                            aisleId: element.AisleId.S,
                            bin: element.Item.S,
                            customers: cust_names,
                            highlight: element.Highlight.BOOL.toString(),
                            restock: element.Restock.BOOL.toString(),
                        }
                    );
                //console.log(element);
            });
            updateCustomerRecommendations(recosByAisle);
            }
        });

    }


    static getCustomerLocations = (updateCustomerLocations) => {

        var ddb = cusomterUtils.getDDB();

        var params = {
            RequestItems: {
            'Customer': {
             "Keys": [
                 {"Id":{"N":"1"}},
                 {"Id":{"N":"2"}},
                 {"Id":{"N":"3"}},
                 {"Id":{"N":"4"}},
                 {"Id":{"N":"5"}},                
                 ],
                "ProjectionExpression": 'Id, StoreLocation, CustomerName'
             }
            }
        };
 
        ddb.batchGetItem(params, function(err, data) {
            var customers = [];
            if (err) {
                console.log("Error", err);
            } else {
                data.Responses.Customer.forEach(function(element, index, array) {
                    //debugger;
                    customers.push(
                        {
                            id: element.Id.N,
                            name: element.CustomerName.S,
                            storeLocation: element.StoreLocation.S
                        }
                    );
                //console.log(element);
            });
            updateCustomerLocations(customers);
            }
        });

    }

    static  getCustomers = (updateCustomers) => {

       var ddb = cusomterUtils.getDDB();

       var params = {
           RequestItems: {
           'Customer': {
            "Keys": [
                {"Id":{"N":"1"}},
                {"Id":{"N":"2"}},
                {"Id":{"N":"3"}},
                {"Id":{"N":"4"}},
                {"Id":{"N":"5"}},                
                ],
               "ProjectionExpression": 'Id, CreditCardInfo,StoreLocation,CustomerName,Photo'
            }
           }
       };

       ddb.batchGetItem(params, function(err, data) {
           var customers = [];
           if (err) {
           console.log("Error", err);
           } else {
           data.Responses.Customer.forEach(function(element, index, array) {
               //debugger;
                customers.push(
                    {
                        id: element.Id.N,
                        name: element.CustomerName.S,
                        //photo: element.Photo.S    <--- this URL will not load because of permission issues. we need to download image and then load. for now, hack - load local image
                        photo: element.CustomerName.S
                    }
                );

               //console.log(element);
           });

           updateCustomers(customers);
           }
       });
    
    }

}

