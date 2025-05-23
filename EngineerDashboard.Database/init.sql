create database telemetry;

use telemetry;

CREATE TABLE Ranks (
    id int PRIMARY KEY auto_increment,
    name VARCHAR(20) not null,
    icon TEXT not null,
    pointsMin int not null,
    pointsMax int not null
);

CREATE TABLE Drivers (
    id BIGINT PRIMARY KEY auto_increment,
    username VARCHAR(50) not null,
    ELO int not null default 1000,
    rankId int not null,
    FOREIGN KEY (rankId) REFERENCES Ranks(id)
);

CREATE TABLE Tracks (
    id int PRIMARY KEY auto_increment,
    name VARCHAR(50) not null
);

CREATE TABLE Races (
    id BIGINT PRIMARY KEY auto_increment,
    date DATE not null,
    trackId int not null,
    aiDifficulty int not null,
    raceLength int not null,
    FOREIGN KEY (trackId) REFERENCES Tracks(id)
);

CREATE TABLE RaceResults (
    id BIGINT PRIMARY KEY auto_increment,
    raceId BIGINT not null,
    driverId BIGINT not null,
    startPos int not null,
    finishPos int not null,
    hasFastestLap BOOLEAN not null,
    penalties int not null,
    dnf BOOLEAN not null,
    sessionTime BIGINT not null,
    points int not null,
    carDamage int not null,
    FOREIGN KEY (raceId) REFERENCES Races(id),
    FOREIGN KEY (driverId) REFERENCES Drivers(id)
);

CREATE TABLE Laps (
    id BIGINT PRIMARY KEY auto_increment,
    raceResultId BIGINT not null,
    position int not null,
    tyreCompound enum('W','I','H','M','S') not null ,
    deltaToLeader int not null,
    deltaToCarInFront int not null,
    tyreWear int not null,
    lapTime int not null,
    FOREIGN KEY (raceResultId) REFERENCES RaceResults(id)
);

CREATE TABLE Stints (
    id BIGINT PRIMARY KEY auto_increment,  
    raceResultId BIGINT not null,
    tyreCompound enum('W','I','H','M','S') not null,
    startLap int not null,
    endLap int not null,
    pitStopTime int not null,
    FOREIGN KEY (raceResultId) REFERENCES RaceResults(id)
);
