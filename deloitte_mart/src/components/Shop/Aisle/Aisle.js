import React, { Component } from 'react';
import HOC from '../../DOM/HOC';
import Shopper from './Shopper';
import './Aisle.css';

class Aisle extends Component
{
    shopperMovedRight(index, shopperId) {
        alert('shopper moved right ' + index + ' shopper ' + shopperId);
    }

    shopperMovedLeft(index, shopperId) {
        alert('shopper moved left ' + index + ' shopper ' + shopperId);
    }

    render(){
        var aisle_img = '../' + this.props.id + '.jpg';
        let shoppers =                   
                      this.props.shoppers.map( 
                        (shopper, index) => {
                          return <Shopper
                                  name ={shopper.name}
                                  key={shopper.name}
                                  index={index}
                                  showShopperInfo = {(name)=>this.props.showShopperInfo(name)}
                                  movedRight={() => this.shopperMovedRight(index, shopper.id)}
                                  movedLeft={() => this.shopperMovedLeft(index, shopper.id)}
                                  />
                        }
                      )
                  ;

        return (
            <HOC>
                <div class="card mb-4 " ></div>
                <div class="aisle" >
                    {shoppers}
                    {/* <img class="shopper2" onClick={this.openNav}  src="../subject05.normal"  alt="" /> */}
                    <img class="sign" src={aisle_img} alt="" onClick={this.props.click} />
                </div>
            </HOC>
        );
    }
}

export default Aisle;