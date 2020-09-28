import React, { Component } from 'react';
import { Button,Modal } from 'react-bootstrap';
import './CameraFeed.css';
import AWS from 'aws-sdk';
var feedBucketName = "store-input-feed";
var bucketRegion = "us-east-2";
var IdentityPoolId = "us-east-2:b5fe2cdd-2c33-4bdf-9c74-39d28b29cdf6";

/*
// Initialize the Amazon Cognito credentials provider
AWS.config.region = 'us-east-2'; // Region
AWS.config.credentials = new AWS.CognitoIdentityCredentials({
    IdentityPoolId: 'us-east-2:b5fe2cdd-2c33-4bdf-9c74-39d28b29cdf6',
});
*/
class CameraFeed extends Component
{
    uploadPhotoToS3(closeModal, cameraLocation)
    {       
        var AWS = require('aws-sdk');
        debugger;
        // AWS.config.update({
        //     region: bucketRegion,
        //     credentials: new AWS.CognitoIdentityCredentials({
        //       IdentityPoolId: IdentityPoolId
        //     })
        //   });

       // Initialize the Amazon Cognito credentials provider
        AWS.config.region = 'us-east-2'; // Region
        AWS.config.credentials = new AWS.CognitoIdentityCredentials({
            IdentityPoolId: 'us-east-2:b5fe2cdd-2c33-4bdf-9c74-39d28b29cdf6',
        });

          var s3 = new AWS.S3({
            apiVersion: "2006-03-01",
            params: { Bucket: feedBucketName }
          });

          // alert(document.getElementById('feedFile').value);

          var files = document.getElementById("feedFile").files;
            if (!files.length) {
                return alert("Please choose a file to upload first.");
            }
            var file = files[0];
            var fileName = file.name;
            var s3KeyFolderPath = encodeURIComponent("CAM-Aisle1") + "/";

            switch(cameraLocation.toUpperCase())
            {
                case "ENTRANCE":
                     s3KeyFolderPath = encodeURIComponent("CAM-Entrance") + "/";
                     break;
                case "AISLE1":
                     s3KeyFolderPath = encodeURIComponent("CAM-Aisle1") + "/";
                     break;
                case "AISLE2":
                     s3KeyFolderPath = encodeURIComponent("CAM-Aisle2") + "/";
                     break;
                case "AISLE3":
                     s3KeyFolderPath = encodeURIComponent("CAM-Aisle3") + "/";
                     break;
                case "AISLE4":
                     s3KeyFolderPath = encodeURIComponent("CAM-Aisle4") + "/";
                     break;
                case "CHECKOUT":
                    s3KeyFolderPath = encodeURIComponent("CAM-Checkout") + "/";
                    break;
                default:
                     s3KeyFolderPath = encodeURIComponent("CAM-Aisle1") + "/";
                     break;
            }

            var s3Key = s3KeyFolderPath + fileName;

            // Use S3 ManagedUpload class as it supports multipart uploads
            // var upload = new AWS.S3.ManagedUpload({
            //     params: {
            //     Bucket: feedBucketName,
            //     Key: s3Key,
            //     Body: file
            //     }
            // });

            var upload = s3.upload(
                {Bucket: feedBucketName, Key: s3Key, Body: file}
            );


            var promise = upload.promise();

            promise.then(
                function(data) {
                //alert("Successfully uploaded photo.");
                closeModal();
                
                },
                function(err) {
                return alert("There was an error uploading your photo: ", err.message);
                }
            );
    }


    render(){
       return (
        <Modal show={this.props.showCameraFeed} onHide={this.props.close}  className="text-left" centered>
            <Modal.Header closeButton>
            <Modal.Title>Mimic Camera Feed - <b><font color='red'>{this.props.cameraLocation}</font></b></Modal.Title>
            </Modal.Header>
            <Modal.Body>
            <div className="camera "></div>
               
           
            <div class="input-group mb-2 mr-sm-2">
                <input type="file"  id="feedFile" className="form-control mr-3 mb-2 camera-upload " />
                
                <button type="button" class="btn btn-primary mb-2" onClick={()=>this.uploadPhotoToS3(this.props.close, this.props.cameraLocation)}>Go</button>
            </div>
               
            </Modal.Body>
            
        </Modal>

       );
    }
}

export default CameraFeed;