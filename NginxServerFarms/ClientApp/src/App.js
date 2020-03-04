import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { About } from './components/About';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { HubConnectionBuilder } from '@microsoft/signalr';
import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  constructor(props) {
    super(props);

    this.state = {
      hubConnection: null,
      upstreams: null,
    };

    this.refreshConfigs = this.refreshConfigs.bind(this);
  }

  refreshConfigs(e) {
    debugger
    this.setState({
      upstreams: e.upstreams
    })
  }

  componentDidMount = () => {
    const hubConnection = new HubConnectionBuilder()
      .withUrl("/nginxHub")
      .withAutomaticReconnect()
      .build();

    hubConnection.on('RefreshConfigs', this.refreshConfigs);

    hubConnection.start()
      .then(() => {
        hubConnection.invoke('GetUpstreams').then(function (result) {
          debugger
          this.setState({
            upstreams: result
          })
        }).catch(function (err) {
          console.log(`GetConfig failed: ${err}`)
        })
      })
      .catch(err => console.log(`nginxHub error while connecting: ${err}`))

    this.setState({
      hubConnection: hubConnection
    })
  }

  render() {
    return (
      <Layout>
        <Route exact path='/' component={About} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data' component={FetchData} />
        <Route path='/about' component={About} />
      </Layout>
    );
  }
}
