import React, { Component } from 'react';
import Bin from './Bin'

class Shelf extends Component
{
    render(){

        let bins = this.props.bins.map( 
            (bin, index) => {
                
              return <Bin
                      id ={bin.id}
                      key={bin.id}
                      highlights={bin.highlights}
                      restock={bin.restock}

                      // click={() => this.binClicked(index)}
                      />
            }
          )
      ;
        return (
            <div class="card mb-4 pt-3">
            {bins}
          </div>
        );
    }

}

export default Shelf;