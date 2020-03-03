import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';

import signalR from '@microsoft/signalr';

import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  constructor(props) {
    super(props);

    this.state = {
      hubConnection : null,
    };
  }

  componentDidMount = () => {
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("/nginxHub")
      .build();

    hubConnection.start()
      .then(() => console.log('nginxHub started'))
      .catch(err => console.log(`nginxHub error while connecting: ${err}`))

    this.setState({
      hubConnection
    })
  }

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data' component={FetchData} />
      </Layout>
    );
  }
}
