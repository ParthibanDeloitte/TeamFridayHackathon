import React, {Component}  from 'react';
import './Area.css'

class Area extends Component 
{
    render(){
        var classes_area = ["card", "mb-4", "shelf"];
        classes_area.push(this.props.name);

        var images = [];
       // debugger;
        if (this.props.shoppers.length > 0 && this.props.shoppers[0] !== null)
        {
            //debugger;
            var classes_img = ["shopper-entrance"];
            classes_img.push(this.props.shoppers[0].name);

            images.push( <img className={classes_img.join(' ')} /> );
        }
        if (this.props.shoppers.length > 1 && this.props.shoppers[1] !== null)
        {
            var classes_img = ["shopper2-entrance"];
            classes_img.push(this.props.shoppers[1].name);

            images.push( <img className={classes_img.join(' ')} /> );
        }
        
        return (            
            <div className={classes_area.join(' ')} onClick={this.props.click}>             
            {images}
            </div>
        );
    }
}

export default Area;