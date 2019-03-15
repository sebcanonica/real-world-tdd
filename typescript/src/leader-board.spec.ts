import express from "express"
import {assert, expect} from 'chai';
import request from "supertest-as-promised"

import {FootballService, HttpFootballService} from "./leader-board"

describe('LeaderBoard', () => {

    xit('should display a  leaderboard with  the state of all games', async () => {
        const app = express() // this is the start of the actual production code
        // You'll have to change this route and do something sensible in order
        // for the test to pass. Eventually you'll have to move this into "Production" code
        app.get('/foo', (req, res) => {
            res.send('bar')
        })

        let {status, body} = await request(app).get('/leaderboard')

        expect(status).eq(200)
        // then assert something on the response body

    }).timeout(6000);


});

function assertValidEvents(events) {
    expect(events).to.be.an('array');
    events.forEach(event => {
        expect(event.type).to.be.oneOf(['game-start', 'goal', 'game-end']);
        expect(event.gameId.split('-')).to.be.of.length(2);
        if (event.type === 'goal') {
            expect(event.gameId).to.include(event.team);
        }
    });
}

class MockedFootballService implements FootballService {
    _events;

    constructor(events) {
        this._events = events;
    }

    async getEvents() {
        assertValidEvents(this._events);
        return this._events;
    };
}

describe('Football events dependency', function () {

    it('is what it is and i want to capture it', async () => {        
        const actual = await( new HttpFootballService().getEvents());
        assertValidEvents(actual);
    }).timeout(6000);

    describe('Mocked dependency', () => {
        it('should be able to return the same content', async () => {
            const actual = await (new MockedFootballService([ 
                { type: 'game-start', gameId: 'lyon-marseille' },
                { type: 'goal', gameId: 'lyon-marseille', team: 'lyon' },
                { type: 'goal', gameId: 'lyon-marseille', team: 'marseille' },
                { type: 'game-end', gameId: 'lyon-marseille' }
            ]).getEvents());
            assertValidEvents(actual);
        });

    });
});