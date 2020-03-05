import React, { Component } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';
import { NginxUpstream } from './NginxUpstream';

export class NginxUpstreams extends Component {
    static displayName = NginxUpstreams.name;

    constructor(props) {
        super(props);
        this.state = {
            hubConnection: null,
            upstreams: null
        };
    }

    componentDidMount() {
        this.getUpstreams();
        this.setupWebSocket();
    }

    async componentWillUnmount() {
        const { hubConnection } = this.state;
        await hubConnection.stop();
    }

    render() {
        const { upstreams } = this.state;
        return (
            <div>
                <h1>Nginx Upstreams</h1>
                <p>Changing these entries will update the local proxy service config and restart it.</p>
                {
                    upstreams &&
                    upstreams.map((x, i) => {
                        return (
                            <NginxUpstream
                                key={i}
                                upstream={x}
                            />
                        );
                    })
                }
            </div>
        );
    }

    refreshConfigs = (upstreams) => {
        this.setState({
            upstreams
        })
    }

    setupWebSocket = async () => {
        const hubConnection = new HubConnectionBuilder()
            .withUrl("/nginxHub")
            .withAutomaticReconnect()
            .build();

        hubConnection.on('RefreshConfigs', this.refreshConfigs);

        await hubConnection.start()
            .then()
            .catch(err => console.log(`nginxHub error connecting: ${err}`))

        this.setState({
            hubConnection: hubConnection
        })
    }

    getUpstreams = async () => {
        const response = await fetch('nginx/upstreams');
        this.setState({
            upstreams: await response.json()
        });
    }
}