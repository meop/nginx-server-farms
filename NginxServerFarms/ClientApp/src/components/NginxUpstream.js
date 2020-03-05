import React, { Component } from 'react';
import { ListGroup, ListGroupItem } from 'reactstrap';

export class NginxUpstream extends Component {
    static displayName = NginxUpstream.name;

    render() {
        const { upstream } = this.props;
        return (
            <>
                <h1>{upstream.name}</h1>
                <ListGroup>
                    {
                        upstream.servers.map((x, i) => {
                            return (
                                <ListGroupItem
                                    key={i}
                                    active={x.enabled}
                                    disabled={x.enabled}
                                    action
                                    onClick={() => this.setActiveServer(x)}
                                >
                                    {x.entry}
                                </ListGroupItem>
                            );
                        })
                    }
                </ListGroup>
            </>
        )
    }

    setActiveServer = async (upstreamServer) => {
        const { upstream } = this.props
        const changedUpstream = {
            name: upstream.name,
            servers: []
        }
        upstream.servers.forEach((s) => {
            if (s.entry === upstreamServer.entry) {
                changedUpstream.servers.push({
                    enabled: true,
                    entry: s.entry,
                })
            }
            else if (s.enabled) {
                changedUpstream.servers.push({
                    enabled: false,
                    entry: s.entry,
                })
            }
        });

        await this.setUpstream(changedUpstream)
    }

    setUpstream = async (changedUpstream) => {
        // right now i dont care about the response.. assume it worked :)
        // i can pop up a green check or red cross or something
        // if we make a feedback loop for events when restarting service
        await fetch('nginx/upstreams', {
            method: 'post',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify([changedUpstream])
        });
    }
}