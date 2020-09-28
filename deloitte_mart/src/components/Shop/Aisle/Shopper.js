import React, { Component } from 'react';

class Shopper extends Component
{
    render(){
        //debugger;
        var classes_shopper = [];
        classes_shopper.push(this.props.name);
        
        var shopper_class = 'shopper' + (this.props.index == 0?'':this.props.index+1);
        classes_shopper.push(shopper_class);
        var middle_class = 'middle' + (this.props.index == 0?'':this.props.index+1);
        return (
            <div className="shopper-container" >
            <img className={classes_shopper.join(' ')} onClick={() => this.props.showShopperInfo(this.props.name)}/>
            <div className={middle_class} >
                <img className="mover-right" src="../right.png" onClick={this.props.movedRight}/>
                <img src="../left.png" onClick={this.props.movedLeft}/>
            </div>
            </div>
        );
    }

}

export default Shopper;