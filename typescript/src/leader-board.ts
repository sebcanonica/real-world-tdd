import got from "got"


export interface FootballService {
    getEvents(): Promise<any>;
}

export class HttpFootballService implements FootballService {
    async getEvents() {
        return (await got('http://localhost:5010/events', {json: true})).body;
    };
}