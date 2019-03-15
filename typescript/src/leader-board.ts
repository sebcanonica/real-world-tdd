import got from "got"
import express from "express"

export function createLeaderBoardApp(footballService:FootballService) {
    const app = express();
    app.get('/leaderboard', async (req, res) => {
        const events = await footballService.getEvents();
        const event = events[0];
        const teams = event.gameId.split('-');
        res.send([{ 
            home: capitalize(teams[0]), 
            visitor: capitalize(teams[1]), 
            score: [0, 0], 
            state: "in progress"
        }])
    })
    return app;
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
