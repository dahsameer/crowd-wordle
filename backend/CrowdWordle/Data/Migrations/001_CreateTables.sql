CREATE TABLE IF NOT EXISTS GameRecords (
    Id INTEGER PRIMARY KEY,
    EndedTime TEXT NOT NULL,
    SelectedWord INTEGER NOT NULL,
    Round INTEGER NOT NULL,
    Won INTEGER NOT NULL,
    TotalUsers INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS RoundRecords (
    GameId INTEGER NOT NULL,
    Round INTEGER NOT NULL,
    PlayedWord INTEGER NOT NULL,
    TotalVotes INTEGER NOT NULL,
    PRIMARY KEY (GameId, Round)
);

CREATE TABLE IF NOT EXISTS VoteRecords (
    GameId INTEGER NOT NULL,
    Round INTEGER NOT NULL,
    VoteIndex INTEGER NOT NULL,
    VotedWord INTEGER NOT NULL,
    Votes INTEGER NOT NULL,
    PRIMARY KEY (GameId, Round, VoteIndex)
);

CREATE TABLE IF NOT EXISTS SystemRecords (
    UserIdIndex INTEGER NOT NULL,
    HighestUserCount INTEGER NOT NULL
);

INSERT INTO SystemRecords (UserIdIndex, HighestUserCount)
SELECT 0, 0
WHERE NOT EXISTS (SELECT 1 FROM SystemRecords);