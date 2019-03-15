import express from "express"
import {assert, expect} from 'chai';
import request from "supertest-as-promised"
import got from "got"

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

describe('Football events dependency', function () {

    it('is what it is and i want to capture it', async () => {        
        const actual = (await got('http://localhost:5010/events', {json: true})).body;
        expect(actual).to.deep.eq([ 
            { type: 'game-start', gameId: 'lyon-marseille' },
            { type: 'goal', gameId: 'lyon-marseille', team: 'lyon' },
            { type: 'goal', gameId: 'lyon-marseille', team: 'marseille' },
            { type: 'game-end', gameId: 'lyon-marseille' },
            { type: 'game-start', gameId: 'paris-monaco' } ]
        );
    }).timeout(6000);

});