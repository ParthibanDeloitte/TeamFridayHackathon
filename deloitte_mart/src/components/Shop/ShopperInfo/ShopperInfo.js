import React, { Component } from 'react';
import './ShopperInfo.css';

class ShopperInfo extends Component
{
    render(){
        var classes = ["card-img-top"];
        classes.push(this.props.shopper);
        //console.log(this.props.shopper_loc);

        var credit_card = null, status = null;
        if(this.props.shopper == "subject01")
        {
            credit_card = <p className="card-text bg-success">Credit Card on file</p>;
            status = (<div class="alert alert-success text-left" role="alert">
            Status: <b>Good</b>
            </div>   );
        }
        else
        {
            credit_card = <p className="card-text bg-danger">No Credit Card on file</p>;
            status = (<div class="alert alert-danger text-left" role="alert">
            Status: <b>Help needed</b>
            </div>   );
        }

       return (
                <div id="mySidepanel" class="sidepanel">                    
                <div class="card text-left" >
                <img  className={classes.join(' ')} alt="..."/><a href="javascript:void(0)" class="closebtn" onClick={this.props.close}>&times;</a>
                <div className="card-body">
                    <h5 className="card-title">{this.props.shopper}</h5>
                    {credit_card}
                </div>
                <ul class="list-group list-group-flush">
                    <li className="list-group-item"><span className='lineItem'>Tomatos</span><span>$3.99</span></li>
                    <li className="list-group-item"><span className='lineItem'>Apples</span><span>$6.99</span></li>
                    <li className="list-group-item"><span className='lineItem'>Cheese</span><span>$1.99</span></li>
                </ul>
                </div>
                {status}             
            </div>
       );
    }

}

export default ShopperInfo;