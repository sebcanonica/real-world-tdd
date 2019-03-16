import got from "got"
import express from "express"

export function createLeaderBoardApp(footballService:FootballService) {
    const app = express();
    app.get('/leaderboard', async (req, res) => {
        const events = await footballService.getEvents();
        const gamesById = {};
        const leaderboard = events
            .filter(isType('game-start'))
            .map(event => gamesById[event.gameId] = createGameFromEvent(event) );
        events.filter(isType('game-end'))
            .forEach( event => gamesById[event.gameId].state = 'finished' );
        events.filter(isType('goal'))
            .forEach( event => updateScore(event, gamesById[event.gameId].score) );                    
        res.send(leaderboard);
    })
    return app;
}

function isType(type) {
    return (event) => event.type === type;
}

function createGameFromEvent(event) {
    const teams = event.gameId.split('-');
    return { 
        home: capitalize(teams[0]), 
        visitor: capitalize(teams[1]), 
        score: [0, 0], 
        state: "in progress"
    };
}

function updateScore(event, score) {
    if (event.gameId.startsWith(event.team)) {
        score[0]++;
    } else {
        score[1]++;
    }
}

export interface FootballService {
    getEvents(): Promise<any>;
}

export class HttpFootballService implements FootballService {
    async getEvents() {
        return (await got('http://localhost:5010/events', {json: true})).body;
    };
}

function capitalize(string) {
    return string.charAt(0).toUpperCase() + string.slice(1);
}
