import React, { Component } from 'react';
import './Bin.css';
import HOC from '../../DOM/HOC';
import ReactDOM from 'react-dom'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faHeart, faUnderline } from '@fortawesome/free-solid-svg-icons'
import { faLightbulb} from '@fortawesome/free-solid-svg-icons'

class Bin extends Component
{
    render()
    {
        var classes = ["card-header", "bin"];
        classes.push(this.props.id);
        var favorites = [];
        var restock= null;

        if(this.props.restock !== null && this.props.restock !== undefined && this.props.restock === "true")
            restock = <FontAwesomeIcon icon={faLightbulb} style={{color:"#ff0414"}} size="2x" className="restock"/>;

        var class_fav = null;
        var class_fav2 = null;
        for(var i=0;i<this.props.highlights.length;i++)
        {
            if(i == 0)
                class_fav= "favorite"; 
            else
                class_fav2 = "favorite2";
        }

        if(class_fav === null || class_fav === undefined )
            class_fav = "favorite-off";

        if(class_fav2 === null || class_fav2 === undefined )
            class_fav2 = "favorite2-off";

        for(var i=0;i<this.props.highlights.length;i++)
        {
            var highlightFor = this.props.highlights[i];
            switch(highlightFor)
            {
                case "subject01":
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#8e3ccb"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
                    case "subject02":
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#fe7e0f"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
                    case "subject03":
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#508b00"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
                    case "subject04":
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#213451"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
                    case "subject05":
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#7a4f56"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
                    default:
                        favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#508b00"}} size="2x" className={i==0?class_fav:class_fav2}/>   );
                        break;
            }            
            //favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#508b00"}} size="2x" className="favorite2"/>    );
        }
        
        if( favorites.length == 0)
            favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#213451"}} size="2x" className={class_fav}/>   );
        if( favorites.length == 1)
            favorites.push( <FontAwesomeIcon icon={faHeart} style={{color:"#213451"}} size="2x" className={class_fav2}/>   );
        
        return (
            <HOC>           
            <div className={classes.join(' ')}>
                {favorites}
                {restock}                
            </div> 
            </HOC>
        );
    }

}

export default Bin;