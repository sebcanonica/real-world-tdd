# Football leaderboard kata 
This kata is about applying TDD in the presence of *untestable* dependencies.
Your task is to write a rest-api that uses another lower level rest-api. Do not 
mock this external API until you experience some difficulty with them 
(too slow, or flaky, ...). You definately want to *explore* the other api's behaviour 
with unit tests. Then once you have a few tests, you can refactor the tests to
something more in line with the testing pyramid. 

Start with a guiding system-level test. You'r priority is to get the integration
points right, i.e. calling the external api correctly and providing the right value 
on your http endpoint. Once you have that you can probably get the interal logic
right with traditional fast unit tests.

At the end I expect you'll have three types of tests, system-tests, focused 
integration-tests and unit-tests. Personally I separate those tests in my CI
so that I can run them without polluting the unit tests with "random" failures
but still get the valuable information that an external api is down/broken. 
  
## The external api
This is code out of your control. You can get it by installing 
a docker image or access the online version
worst case it is provided as a lib that you can start yourself  

    cd football-events-rest-api 
    npm install
    npm start
     
You now have a webserver running you can get todays 
events on http://localhost:5010/events

This api responds with a list of all game events, like goals etc.

## Task
Write a rest-api that serve data for a score-board with 
todays' football games. 

home team | visitor team | state | score
--- | --- | --- | ---  
Barcelona | Madrid | in progress | 0 -1  
Arsenal | Paris | finished | 4 - 3      

You don't have to worry about formatting, just return 
JSON of the form

    [
      { "home": "Barcelona", 
        "visitor": "Madrid", 
        "score": [0, 1], 
        "state": "in progress" }
    ]


## TODOs
* publish docker
* make sure there is some coordination between external ws so that they can't be
  that easily mocked out
