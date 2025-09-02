CREATE TABLE IF NOT EXISTS GameRecords (
    Id INTEGER PRIMARY KEY,
    StartedTime TEXT NOT NULL,
    SelectedWord INTEGER NOT NULL,
    Round INTEGER NOT NULL,
    PlayedWord1 INTEGER NOT NULL,
    PlayedWord2 INTEGER NOT NULL,
    PlayedWord3 INTEGER NOT NULL,
    PlayedWord4 INTEGER NOT NULL,
    PlayedWord5 INTEGER NOT NULL,
    PlayedWord6 INTEGER NOT NULL,
    Won INTEGER NOT NULL,
    EndedTime TEXT NOT NULL,
    TotalUsers INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS SystemRecords (
    UserIdIndex INTEGER NOT NULL,
    HighestUserCount INTEGER NOT NULL
);

INSERT INTO SystemRecords (UserIdIndex, HighestUserCount)
SELECT 0, 0
WHERE NOT EXISTS (SELECT 1 FROM SystemRecords);