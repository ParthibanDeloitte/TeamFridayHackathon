import React, {Component} from 'react';
import './App.css';
import DMFooter from './components/DOM/DMFooter';
import Area from './components/Shop/Area/Area';
import Shelf from './components/Shop/Shelf/Shelf';
import Aisle from './components/Shop/Aisle/Aisle';
import {cusomterUtils} from './components/utils/customerUtils';
import CameraFeed from './components/Shop/Camera/CameraFeed';
import CustomerGrid from './components/Customer/CustomerGrid';
import ShopperInfo from './components/Shop/ShopperInfo/ShopperInfo';
import { Button,Modal } from 'react-bootstrap';

class App extends Component {

  componentDidMount() {

    this.timer1 = setInterval(
        () => this.checkForLocationUpdates(),
        5000
    );

    this.timer2 = setInterval(
      () => this.checkForRecommendations(),
      5000
    );

  }

  componentWillUnmount() {
    clearInterval(this.timer1);
    clearInterval(this.timer2);
  }  

  state = {
    shopper_selected: "",
    showCustomerModal: false,
    showCameraFeed: false,
    cameraFeedAisle: '',
    showLogs:false,
    customers:[
    ],
    entrance:[
    ],
    checkout:[
    ],
    aisles: [
      {
        id:"Aisle1", 
        shoppers:[          
        ]
      },
      {
        id:"Aisle2", 
        shoppers:[
        ]
      },
      {
        id:"Aisle3", 
        shoppers:[
          // {name:"subject04"},
          // {name:"subject05"}
        ]
      },
      {
        id:"Aisle4", 
        shoppers:[
        ]
      }     
    ],
    shelves:[
      {
        id:"shelf1", 
        bins:[
          {
            id:"tomatos",
            highlights:[],
            restock: "false"
          },
          {id:"lettuce",
          highlights:[],
          restock: "false"
        },
          {id:"apples",
          highlights:[],
          restock: "false"
        },
          {id:"oranges",
          highlights:[],
          restock: "false"
        },
        ]
      },
      {
        id:"shelf2", 
        bins:[
          {id:"chips",
          highlights:[],
          restock: "false"
        },
          {id:"oreos",
          highlights:[],
          restock: "false"
        },
          {id:"bars",
          highlights:[],
          restock: "false"
        },
          {id:"candies",
          highlights:[],
          restock: "false"
        },
        ]
      },
      {
        id:"shelf3", 
        bins:[
          {id:"sugar",
          highlights:[],
          restock: "false"
        },
          {id:"salt",
          highlights:[],
          restock: "false"
        },
          {id:"flour",
          highlights:[],
          restock: "false"
        },
          {id:"pasta",
          highlights:[],
          restock: "false"
        },
        ]
      },
      {
        id:"shelf4", 
        bins:[
          {id:"milk",
          highlights:[],
          restock: "false"
        },
          {id:"cheese",
          highlights:[],
          restock: "false"
        },
          {id:"cake",
          highlights:[],
          restock: "false"
        },
          {id:"bread",
          highlights:[],
          restock: "false"
        },
        ]
      }
    ]
  };

  checkForLocationUpdates()
  {
    console.log('hi there! checking for location updates...');
    cusomterUtils.getCustomerLocations((customers) => this.updateCustomerLocations(customers));
  }

  checkForRecommendations()
  {
    console.log('hi there! checking for recommendation & restock updates...');
    cusomterUtils.getCustomerRecommendations((customers) => this.updateCustomerRecommendations(customers));
  }

  updateCustomerRecommendations(recosByAisle)
  {
      var updated_shelves = this.state.shelves;
      for(var i=0;i<updated_shelves.length;i++)
      {
        var shelf = updated_shelves[i];
        for(var k=0;k<shelf.bins.length;k++)
        {
          var bin = shelf.bins[k];
          bin.highlights = [];
          bin.restock= "false";
        }
       
      }

      for(var i=0;i<recosByAisle.length;i++)
      {
        var reco = recosByAisle[i];
        var shelfTargeted = reco.aisleId.replace("aisle", "shelf");
        for(var j=0;j<updated_shelves.length;j++)
        {
          var shelf = updated_shelves[j];
          if(shelf.id === shelfTargeted)
          {
            for(var k=0;k<shelf.bins.length;k++)
            {
              var bin = shelf.bins[k];
              if(bin.id === reco.bin)
              {
                  if(reco.highlight !== null &&  reco.highlight !== undefined  && reco.highlight === "true")
                  {
                    bin.highlights = reco.customers;
                  }
                  if(reco.restock !== null &&  reco.restock !== undefined  && reco.restock === "true")
                  {
                    bin.restock = reco.restock;
                  }
              }
            } //for each bin
          }// if target shelf
        }//for every shelf
      }//for every reco


      this.setState(
        {
         shelves: updated_shelves
        }
      );

  }

  updateCustomerLocations(customerLocations){
    //debugger;
    var updated_aisles=this.state.aisles;
    var updated_entrance=this.state.entrance;
    var updated_checkout=this.state.checkout;

    updated_entrance= [];
    updated_checkout=[];
    //first, clear all shopper info 
    for(var j=0;j< updated_aisles.length; j++) //for every aisle
    {
      var aisle = updated_aisles[j];
      aisle.shoppers = [];
    }

    for(var i=0;i< customerLocations.length; i++)//for every customer
    {
      var customerLocation = customerLocations[i];
      if(customerLocation.storeLocation !== null || customerLocation.storeLocation !== undefined)//if we have location for customer
      {
        for(var j=0;j< updated_aisles.length; j++) //for every aisle
        {
          var aisle = updated_aisles[j];
          if(aisle.id.toUpperCase() === customerLocation.storeLocation.toUpperCase())
          {
            aisle.shoppers.push({name:customerLocation.name});
          }
        }
        if('ENTRANCE' === customerLocation.storeLocation.toUpperCase())
        {
          updated_entrance.push({name:customerLocation.name})
        }
        if('CHECKOUT' === customerLocation.storeLocation.toUpperCase())
        {
          updated_checkout.push({name:customerLocation.name})
        }
      }//if customer has location

    }
     this.setState(
       {
        aisles: updated_aisles,
        entrance: updated_entrance,
        checkout: updated_checkout
       }
     );
  }

  openLogs = () =>{
    this.setState(
      {
        showLogs: true
      }
    );
  }

  closeLogs = () =>{
    this.setState(
      {
        showLogs: false
      }
    );
  }

  handleCloseCustomerPick = () => {
    this.setState(
      {
        showCustomerModal: false
      }
    )
  };

  handleShowCustomerPick = () => {

    cusomterUtils.getCustomers((customers) => this.updateCustomers(customers));

    this.setState(
      {
        showCustomerModal: true
      }
    )
  }

  openShopperInfo(shopperName) {
    //alert(shopperName);
      this.setState({
        shopper_selected: shopperName
      }
      );

      document.getElementById("mySidepanel").style.width = "250px";
  } 
  closeShopperInfo() {
    document.getElementById("mySidepanel").style.width = "0";
  }

  aisleClicked(index){
    //alert('aisle clicked: ' + index);
    var aisleFeeding = this.state.aisles[index].id;
    this.setState(
      {
        showCameraFeed: true,
        cameraFeedAisle: aisleFeeding
      }
    );
  }

  handleCloseCameraFeed = () => {
    this.setState(
      {
        showCameraFeed: false
      }
    )
  };


  AreaClicked(areaName){
    //debugger;
    //alert('area clicked: ' + areaName);

    this.setState(
      {
        showCameraFeed: true,
        cameraFeedAisle: areaName
      });


    //cusomterUtils.getCustomers((customers) => this.updateCustomers(customers));
    //this.handleShowCustomerPick();
 }

 updateCustomers(updatedCustomers){
   debugger;
    this.setState(
      {
        customers: updatedCustomers
      }
    );
 }

 arriveCustomer(arrivingCustomer)
 {
    var newValues = [...this.state.entrance, {"name":arrivingCustomer}];
    //alert(newValues);
    this.setState(
      {
        entrance: newValues
      }
    );
    this.handleCloseCustomerPick();
 }

  render() {
    let aisles =  this.state.aisles.map( 
                        (aisle, index) => {
                          return <Aisle
                                  id ={aisle.id}
                                  key={aisle.id}
                                  shoppers = {aisle.shoppers}           
                                  showShopperInfo = {(name) =>this.openShopperInfo(name)}                      
                                  click={() => this.aisleClicked(index)}
                                  />
                        }
                      )
                  ;
    let shelves = this.state.shelves.map( 
                        (shelf, index) => {
                          return <Shelf
                                  id ={shelf.id}
                                  key={shelf.id}
                                  bins = {shelf.bins} 
                                  // click={() => this.shelfClicked(index)}
                                  />
                        }
                      )
                  ;
  return (
    <div className="App container-fluid">
          <div class="row w-100 h-100" >
            <div class="d-flex  h-100 p-3 flex-column col-sm-10">
              <header class="masthead  mx-auto" >
                <div>
                  <h3 class="masthead-brand">Deloitte Mart</h3>
                  <nav class="nav nav-masthead justify-content-center">
                    <a class="nav-link active" href="#">Home</a>
                    <a class="nav-link active" href="#" onClick={this.handleShowCustomerPick}>Customers</a>
                  </nav>
                </div>
              </header>
              
              <main role="main"  >
              <div class="card-deck mb-3 ml-1 text-center" >
              {aisles}
              {/*} whereever we don't have aisle signs, we need empty divs (spacers) below so that the aisle signs align with the shelves below them. */}
              <div class="card mb-4 " > </div>
              <div class="card mb-4 " ></div>
              <div class="card mb-4 " ></div>
              </div>
            
              <div class="card-deck mb-3 ml-1 text-center" >
               <Area name="entrance" click={()=>this.AreaClicked('entrance')} shoppers={this.state.entrance}/>
               {shelves}              
                <Area name="checkout" click={()=>this.AreaClicked('checkout')} shoppers={this.state.checkout}/>
                <Area name="exit" shoppers={[]}/>
              </div>

              </main>

             <DMFooter/>
            </div>
            <div class="sidebar col-sm-2">
              <div class="text-right h-1"><a href="javascript:void(0)" onClick={this.openLogs}>&#128470;</a></div>
            <h5>/aws/lambda/SearchFacesFunction</h5>
                  2020-09-28T07:47:17.165-04:00	START RequestId: db169748-59b6-4495-9a0e-f78107541b56 Version: $LATEST
                  <br/>2020-09-28T07:47:22.725-04:00	UpdateMultipleAttributes: updated location for subject01 ...
                  <br/>2020-09-28T07:47:22.763-04:00	END RequestId: db169748-59b6-4495-9a0e-f78107541b56
                  <br/>2020-09-28T07:47:22.763-04:00	REPORT RequestId: db169748-59b6-4495-9a0e-f78107541b56 Duration: 5598.37 ms Billed Duration: 5600 ms Memory Size: 256 MB Max Memory Used: 100 MB Init Duration: 315.29 ms
                  <br/>2020-09-28T07:50:49.860-04:00	START RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1 Version: $LATEST
                  <br/>2020-09-28T07:50:51.087-04:00	UpdateMultipleAttributes:  updated location for subject02 ...
                  <br/>2020-09-28T07:50:51.088-04:00	END RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1
                  <br/>2020-09-28T07:50:51.088-04:00	REPORT RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1 Duration: 1223.89 ms Billed Duration: 1300 ms Memory Size: 256 MB Max Memory Used: 104 MB
                  <br/><h5 class="pt-1">/aws/lambda/PublishToAisleDevice</h5>
                  2020-09-28T07:47:23.160-04:00	User information updated;
                  <br/>2020-09-28T07:47:23.160-04:00	CustomerId:1;OldLocation:;NewLocation:entrance;
                  <br/>2020-09-28T07:47:23.160-04:00	Customer entered the store. Updating Missing stock information.
                  <br/>2020-09-28T07:47:23.226-04:00	Please refill following stock items for customer(1): aisle4: cheese, bread
                  <br/>2020-09-28T07:47:23.226-04:00	---------------------------------------------
                  <br/>2020-09-28T07:47:23.226-04:00	Stream processing complete.
                  <br/>2020-09-28T07:47:23.227-04:00	END RequestId: 0ae4c823-cb40-4f15-85bd-7adea261746e
                  <br/>2020-09-28T07:47:23.227-04:00	REPORT RequestId: 0ae4c823-cb40-4f15-85bd-7adea261746e Duration: 66.88 ms Billed Duration: 100 ms Memory Size: 512 MB Max Memory Used: 116 MB
                  <br/>2020-09-28T07:50:51.437-04:00	START RequestId: e0410195-3d8a-40da-99de-11b7980573e1 Version: $LATEST
                  <br/>2020-09-28T07:50:51.439-04:00	Beginning to process customer updates for 1 record(s)...
                  <br/>2020-09-28T07:50:51.439-04:00	---------------------------------------------
                  <br/>2020-09-28T07:50:51.439-04:00	User information updated;
                  <br/>2020-09-28T07:50:51.439-04:00	CustomerId:1;OldLocation:entrance;NewLocation:aisle1;
                  <br/>2020-09-28T07:50:51.439-04:00	Customer location updated from entrance to aisle1.
                  <br/>2020-09-28T07:50:51.590-04:00	Notifcations Sent to aisle1 to highlight items of customer recommendations: apples, oranges, .
                  <br/>2020-09-28T07:50:51.590-04:00	---------------------------------------------
                  <br/>2020-09-28T07:50:51.590-04:00	Stream processing complete.
                  <br/>2020-09-28T07:50:51.591-04:00	END RequestId: e0410195-3d8a-40da-99de-11b7980573e1
                  <br/>2020-09-28T07:50:51.591-04:00	REPORT RequestId: e0410195-3d8a-40da-99de-11b7980573e1 Duration: 151.97 ms Billed Duration: 200 ms Memory Size: 512 MB Max Memory Used: 117 MB
                  <br/>2020-09-28T07:51:19.280-04:00	START RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014 Version: $LATEST
                  <br/>2020-09-28T07:51:19.283-04:00	Beginning to process customer updates for 1 record(s)...
                  <br/>2020-09-28T07:51:19.283-04:00	---------------------------------------------
                  <br/>2020-09-28T07:51:19.283-04:00	User information updated;
                  <br/>2020-09-28T07:51:19.283-04:00	CustomerId:1;OldLocation:aisle1;NewLocation:aisle2;
                  <br/>2020-09-28T07:51:19.284-04:00	Customer location updated from aisle1 to aisle2.
                  <br/>2020-09-28T07:51:19.352-04:00	Notifcations Sent to aisle2 to highlight items of customer recommendations: candies, oreos, .
                  <br/>2020-09-28T07:51:19.370-04:00	Notifcations Sent to aisle1 to stop highlighting items customer recommendations.
                  <br/>2020-09-28T07:51:19.370-04:00	---------------------------------------------
                  <br/>2020-09-28T07:51:19.370-04:00	Stream processing complete.
                  <br/>2020-09-28T07:51:19.371-04:00	END RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014
                  <br/>2020-09-28T07:51:19.371-04:00	REPORT RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014 Duration: 87.48 ms Billed Duration: 100 ms Memory Size: 512 MB Max Memory Used: 117 MB
                  <br/>2020-09-28T07:56:03.792-04:00	START RequestId: 2e3783ae-e8eb-478b-b9b7-616bcca58526 Version: $LATEST
                  <br/>2020-09-28T07:56:03.796-04:00	Beginning to process customer updates for 1 record(s)...
            </div>
          </div>

         <ShopperInfo close={this.closeShopperInfo} shopper_loc={this.state.aisles} shopper={this.state.shopper_selected}/>

          <CustomerGrid showCustomerModal={this.state.showCustomerModal} customers={this.state.customers} close={this.handleCloseCustomerPick} submit={(a)=>this.arriveCustomer(a)}/>
          <CameraFeed showCameraFeed={this.state.showCameraFeed} cameraLocation={this.state.cameraFeedAisle} close={this.handleCloseCameraFeed}/>
          
          <Modal show={this.state.showLogs} onHide={this.closeLogs} size="lg" className="text-left fade" centered>
          <Modal.Header closeButton>
            <Modal.Title>CloudWatch Logs</Modal.Title>
          </Modal.Header>
              <Modal.Body>
              <h5>/aws/lambda/SearchFacesFunction</h5>
                  2020-09-28T07:47:17.165-04:00	START RequestId: db169748-59b6-4495-9a0e-f78107541b56 Version: $LATEST
                  <br/>2020-09-28T07:47:22.725-04:00	UpdateMultipleAttributes: updated location for subject01 ...
                  <br/>2020-09-28T07:47:22.763-04:00	END RequestId: db169748-59b6-4495-9a0e-f78107541b56
                  <br/>2020-09-28T07:47:22.763-04:00	REPORT RequestId: db169748-59b6-4495-9a0e-f78107541b56 Duration: 5598.37 ms Billed Duration: 5600 ms Memory Size: 256 MB Max Memory Used: 100 MB Init Duration: 315.29 ms
                  <br/>2020-09-28T07:50:49.860-04:00	START RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1 Version: $LATEST
                  <br/>2020-09-28T07:50:51.087-04:00	UpdateMultipleAttributes:  updated location for subject02 ...
                  <br/>2020-09-28T07:50:51.088-04:00	END RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1
                  <br/>2020-09-28T07:50:51.088-04:00	REPORT RequestId: 79003070-cefb-47e9-84f4-40a1edf1dce1 Duration: 1223.89 ms Billed Duration: 1300 ms Memory Size: 256 MB Max Memory Used: 104 MB
                  <br/><h5 class="pt-1">/aws/lambda/PublishToAisleDevice</h5>
                  2020-09-28T07:47:23.160-04:00	User information updated;
                  <br/>2020-09-28T07:47:23.160-04:00	CustomerId:1;OldLocation:;NewLocation:entrance;
                  <br/>2020-09-28T07:47:23.160-04:00	Customer entered the store. Updating Missing stock information.
                  <br/>2020-09-28T07:47:23.226-04:00	Please refill following stock items for customer(1): aisle4: cheese, bread
                  <br/>2020-09-28T07:47:23.226-04:00	---------------------------------------------
                  <br/>2020-09-28T07:47:23.226-04:00	Stream processing complete.
                  <br/>2020-09-28T07:47:23.227-04:00	END RequestId: 0ae4c823-cb40-4f15-85bd-7adea261746e
                  <br/>2020-09-28T07:47:23.227-04:00	REPORT RequestId: 0ae4c823-cb40-4f15-85bd-7adea261746e Duration: 66.88 ms Billed Duration: 100 ms Memory Size: 512 MB Max Memory Used: 116 MB
                  <br/>2020-09-28T07:50:51.437-04:00	START RequestId: e0410195-3d8a-40da-99de-11b7980573e1 Version: $LATEST
                  <br/>2020-09-28T07:50:51.439-04:00	Beginning to process customer updates for 1 record(s)...
                  <br/>2020-09-28T07:50:51.439-04:00	---------------------------------------------
                  <br/>2020-09-28T07:50:51.439-04:00	User information updated;
                  <br/>2020-09-28T07:50:51.439-04:00	CustomerId:1;OldLocation:entrance;NewLocation:aisle1;
                  <br/>2020-09-28T07:50:51.439-04:00	Customer location updated from entrance to aisle1.
                  <br/>2020-09-28T07:50:51.590-04:00	Notifcations Sent to aisle1 to highlight items of customer recommendations: apples, oranges, .
                  <br/>2020-09-28T07:50:51.590-04:00	---------------------------------------------
                  <br/>2020-09-28T07:50:51.590-04:00	Stream processing complete.
                  <br/>2020-09-28T07:50:51.591-04:00	END RequestId: e0410195-3d8a-40da-99de-11b7980573e1
                  <br/>2020-09-28T07:50:51.591-04:00	REPORT RequestId: e0410195-3d8a-40da-99de-11b7980573e1 Duration: 151.97 ms Billed Duration: 200 ms Memory Size: 512 MB Max Memory Used: 117 MB
                  <br/>2020-09-28T07:51:19.280-04:00	START RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014 Version: $LATEST
                  <br/>2020-09-28T07:51:19.283-04:00	Beginning to process customer updates for 1 record(s)...
                  <br/>2020-09-28T07:51:19.283-04:00	---------------------------------------------
                  <br/>2020-09-28T07:51:19.283-04:00	User information updated;
                  <br/>2020-09-28T07:51:19.283-04:00	CustomerId:1;OldLocation:aisle1;NewLocation:aisle2;
                  <br/>2020-09-28T07:51:19.284-04:00	Customer location updated from aisle1 to aisle2.
                  <br/>2020-09-28T07:51:19.352-04:00	Notifcations Sent to aisle2 to highlight items of customer recommendations: candies, oreos, .
                  <br/>2020-09-28T07:51:19.370-04:00	Notifcations Sent to aisle1 to stop highlighting items customer recommendations.
                  <br/>2020-09-28T07:51:19.370-04:00	---------------------------------------------
                  <br/>2020-09-28T07:51:19.370-04:00	Stream processing complete.
                  <br/>2020-09-28T07:51:19.371-04:00	END RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014
                  <br/>2020-09-28T07:51:19.371-04:00	REPORT RequestId: f2c8b546-7a51-49b8-a796-24296fd5b014 Duration: 87.48 ms Billed Duration: 100 ms Memory Size: 512 MB Max Memory Used: 117 MB
                  <br/>2020-09-28T07:56:03.792-04:00	START RequestId: 2e3783ae-e8eb-478b-b9b7-616bcca58526 Version: $LATEST
                  <br/>2020-09-28T07:56:03.796-04:00	Beginning to process customer updates for 1 record(s)...
              </Modal.Body>
              <Modal.Footer>
                  <Button variant="primary" onClick={this.props.close}>
                  Close
                  </Button>
                  {/* <Button variant="primary" onClick={()=>this.props.submit(this.selectedSubject)}>
                  Save Changes
                  </Button> */}
              </Modal.Footer>
          </Modal>

    </div>
  );
}
}

export default App;
