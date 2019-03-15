const { createLeaderBoardApp, HttpFootballService } = require('./src/leader-board');
createLeaderBoardApp(new HttpFootballService()).listen(5020)

console.log("LeaderBoard Api started on 5020, you can open http://localhost:5020/leaderboard in a navigator" +
    " and you'll get the leaderboard for the day.") 