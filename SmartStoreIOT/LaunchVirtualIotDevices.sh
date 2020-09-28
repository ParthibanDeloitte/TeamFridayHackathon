#!/bin/bash
sudo yum update -y

# install git and jq to read JSON Output
sudo yum install git -y
sudo yum install jq

# to install node uncomment following lines
# curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.34.0/install.sh | bash
# . ~/.nvm/nvm.sh
# nvm install node
# node -v

# check iot end point and create thing type
aws iot describe-endpoint --endpoint-type iot:Data-ATS
aws iot create-thing-type --thing-type-name "StoreAisle" --thing-type-properties "thingTypeDescription=IOT device installed on store aisles, searchableAttributes=category"

# create certificates to use with things
mkdir ~/certs
curl -o ~/certs/Amazon-root-CA-1.pem https://www.amazontrust.com/repository/AmazonRootCA1.pem
cert_output=`aws iot create-keys-and-certificate  --set-as-active --certificate-pem-outfile "~/certs/device.pem.crt" --public-key-outfile "~/certs/public.pem.key" --private-key-outfile "~/certs/private.pem.key"`
echo $cert_output > certInfo.json
cert_arn=`jq '.certificateArn' certInfo.json`

# create policy file for IOT pub sub
echo "{    \"Version\": \"2012-10-17\",    \"Statement\": [        {            \"Effect\": \"Allow\",            \"Action\": [                \"iot:Publish\",                \"iot:Subscribe\",                \"iot:Receive\",                \"iot:Connect\"            ],            \"Resource\": [                \"*\"            ]        }    ]}" > policy.json
aws iot create-policy --policy-name "IotPubSubPolicy" --policy-document "file://~/policy.json"

# create things, attach certificate to thing, attach policy to certificate
entrance="store1_entrance"
aws iot create-thing --thing-name $entrance --thing-type-name "StoreAisle" 
aws iot attach-thing-principal --thing-name $entrance --principal $cert_arn
aws iot attach-policy --policy-name "IotPubSubPolicy" --target $cert_arn
checkout="store1_checkout"
aws iot create-thing --thing-name $checkout --thing-type-name "StoreAisle" 
aws iot attach-thing-principal --thing-name $checkout --principal $cert_arn
aws iot attach-policy --policy-name "IotPubSubPolicy" --target $cert_arn
storeexit="store1_exit"
aws iot create-thing --thing-name $storeexit --thing-type-name "StoreAisle" 
aws iot attach-thing-principal --thing-name $storeexit --principal $cert_arn
aws iot attach-policy --policy-name "IotPubSubPolicy" --target $cert_arn
n=1
while [ $n -le 5 ]
do
       aisle="store1_aisle$n"
       aws iot create-thing --thing-name $aisle --thing-type-name "StoreAisle" 
       aws iot attach-thing-principal --thing-name $aisle --principal $cert_arn
       aws iot attach-policy --policy-name "IotPubSubPolicy" --target $cert_arn
       n=$((n+1))
done


#install dotnet core
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install dotnet-sdk-3.1
