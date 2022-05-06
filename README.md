# MiniProject-Yuvo
1- Create the Database tables:
CREATE TABLE TRANS_MW_ERC_PM_TN_RADIO_LINK_POWER 
(
    NETWORK_SID int,
    DATETIME_KEY DATETIME,
    Neld float,
    "Object" varchar(80),
    "Time" timestamp,
    "Interval" int,
    Direction varchar(80),
    NeAlias varchar(80),
    NeType varchar(80),
    RxLevelBelowTS1 int,
    RxLevelBelowTS2 int,
    MinRxLevel float,
    MaxRxLevel float,
    TxLevelAboveTS1 int,
    MinTxLevel float,
    MaxTxLevel float,
    FailureDescription varchar(80),
    LINK varchar,
    TID varchar,
    FARENDTID varchar,
    SLOT varchar,
    PORT varchar
);

create table parsed_files
(
FileName varchar,
Date datetime
);

CREATE TABLE loaded_files
(
 FileName varchar,
 DATE datetime
);

CREATE TABLE TRANS_MW_AGG_SLOT_HOURLY
(
"Time" timestamp,
LINK varchar,
SLOT varchar,
NeType varchar,
NeAlias varchar,
MAX_RX_LEVEL float,
MAX_TX_LEVEL float,
RSL_DEVIATION float
);

CREATE TABLE TRANS_MW_AGG_SLOT_DAILY
(
"Time" timestamp,
LINK varchar,
SLOT varchar,
NeType varchar,
NeAlias varchar,
MAX_RX_LEVEL float,
MAX_TX_LEVEL float,
RSL_DEVIATION float
);

Create table TRANS_MW_AGG_SLOT_AGGREGATED
(
AGGREGATED_DATES datetime,
Description varchar
);

2- The folders that should be created locally:
 - LoadedFiles
 - ParsedFiles
 - FilesToBeParsed
 - FilesToBeLoaded

3- In the "appsettings.json" file you should change the paths of the below parameters based on your local machine:
 - VerticaConnectionString
 - FilesToBeLoaded
 - FilesToBeParsed
 - ParsedFiles
 - LoadedFiles 
