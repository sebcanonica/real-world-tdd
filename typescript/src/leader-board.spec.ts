import {assert, expect} from 'chai';
import request from "supertest-as-promised"

import {FootballService, HttpFootballService, createLeaderBoardApp} from "./leader-board"

describe('LeaderBoard', () => {

    it('should display a leaderboard with one game started', async () => {
        const onlyOneGameStart = new MockedFootballService([
            { type: 'game-start', gameId: 'uriage-meylan' }
        ]);
        const app = createLeaderBoardApp(onlyOneGameStart);

        let {status, body} = await request(app).get('/leaderboard')

        expect(status).eq(200)
        expect(body).to.deep.eq([{ 
            home: "Uriage", 
            visitor: "Meylan", 
            score: [0, 0], 
            state: "in progress"
        }]);
    });

});

function assertValidEvents(events) {
    expect(events).to.be.an('array');
    events.forEach(event => {
        expect(event.type).to.be.oneOf(['game-start', 'goal', 'game-end']);
        const teamNames = event.gameId.split('-');
        expect(teamNames).to.be.of.length(2);
        expect(teamNames[0]).to.eq(teamNames[0].toLowerCase());
        expect(teamNames[1]).to.eq(teamNames[1].toLowerCase());
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

    xit('is what it is and i want to capture it', async () => {        
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


import got from "got"

xdescribe('System test', function() {
    it("should return the leaderboard with today's live data", async () => {
        const leaderBoard = (await got('http://localhost:5020/leaderboard', {json: true})).body;
        
        expect(leaderBoard).to.be.an('array');
        leaderBoard.forEach( game => {
            expect(game).to.have.keys( 'home', 'visitor', 'score', 'state' );
            expect(game.score).to.be.an('array');
            assertPositiveInteger(game.score[0]);
            assertPositiveInteger(game.score[1]);
            expect(game.state).to.be.oneOf(['in progress', 'finished']);
        });        
    }).timeout(6000);
});

function assertPositiveInteger(value) {
    expect(value).to.be.a('number');
    expect(value % 1).to.equals(0);
    expect(value).to.be.at.least(0);
}