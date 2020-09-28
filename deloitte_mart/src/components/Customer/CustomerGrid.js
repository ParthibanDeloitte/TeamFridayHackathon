import React, { Component } from 'react';
import { Button,Modal } from 'react-bootstrap';
import 'react-bootstrap-table-next/dist/react-bootstrap-table2.min.css';
import BootstrapTable from 'react-bootstrap-table-next';
import './CustomerGrid.css'

function imageFormatter(cell, row, rowIndex, formatExtraData) {
    var classes_img = ["photo"];
    classes_img.push(cell);
  return (
    <img  className={classes_img.join(' ')} />
  );
}

const columns = [{
  dataField: 'id',
  text: 'ID'
}, {
  dataField: 'name',
  text: 'Name'
},
{
  dataField: 'photo',
  text: 'Photo',
  formatter: imageFormatter
}
];

const selectRow = {
  mode: 'radio',
  clickToSelect: true,
  bgColor: '#00BFFF'
};



class CustomerGrid extends Component
{
    selectedSubject = null;
    rowEvents = {
        onClick: (e, row, rowIndex) => {
          this.selectedSubject = row.name;
        }
      };

    // onSubmit(func)
    // {
    //     debugger;
    //     () => {return func(this.selectedSubject) };
    // }

    render(){
       return (
        <Modal show={this.props.showCustomerModal} onHide={this.props.close} size="lg" className="text-left" centered>
        <Modal.Header closeButton>
          <Modal.Title>Registered Customers</Modal.Title>
        </Modal.Header>
            <Modal.Body>
                <BootstrapTable
                        keyField='id'
                        data={ this.props.customers }
                        columns={ columns }
                        selectRow={ selectRow }
                        rowEvents={ this.rowEvents }
                    />
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

       );
    }
}

export default CustomerGrid;