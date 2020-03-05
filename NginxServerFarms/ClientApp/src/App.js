import React, { Component } from 'react';
import { Route } from 'react-router';
import { About } from './components/About';
import { Layout } from './components/Layout';
import { NginxUpstreams } from './components/NginxUpstreams';
import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
        <Layout>
            <Route exact path='/' component={NginxUpstreams} />
            <Route path='/about' component={About} />
        </Layout>
    );
  }
}
