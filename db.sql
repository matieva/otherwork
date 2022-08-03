-- SQL3 Basic Query


-- Gjalt når vi tok SELECT DISTINCT
--SQL Server Hash Match Aggregate operator is used to process the large tables that are not sorted using an index.


-- common: SELECT, TOP, FROM, WHERE UNION
-- less common: OFFSET EXCEPT, INTERSECT
-- predicates and SARGability (finne for den måten den er lagd, uten å gi andre oppsett for å finne. richard like eksempel)
-- useful functions
-- joins
-- proter indenting








-- AGGREGATE FUNCTIONS, SAMLET FUNKSJONER
-- GROUP BY
-- WINDOWS FUNCTIONS
-- SUBQUERY
-- DERIVED TABLE
-- COMMON TABLE EXPRESSION (CTE)
-- VIEW


-- COMBINE THE BUILDING BLOCKS AND STORE THEM AS A VIEW


--BUILT IN FUNCTIONS
-- SUM(), MAX(), MIN(), AVG(), COUNT().
-- GETDATE(). IS A NONDETERMINISTIC FUNCTION. In computer programming, a nondeterministic algorithm
-- is an algorithm that, even for the same input, can exhibit different behaviors on different runs,
-- as opposed to a deterministic algorithm.
-- SUSER_SNAME() will give you login on the server. to map permissions.
-- WINDOW FUNCTIONS, will come later.








/*         AGGREGATE FUNCTION        */
/*        
DEFINITION: An aggregate function in SQL performs a calculation on multiple values and returns a single value.

EXAMPLE_1:
fra tabell:
LastFinishedDegreeDate
2020-01-30


SELECT
    MAX(ToDate) AS LastFinishedDegreeDate
        FROM LoggLogg

- gir 1 verdi


EXAPLE_2:
-
SELECT
    PersonID, ToDate
        FROM LoggLogg
        WHERE PersonID <= 10003  FÅR Å HA MINDRE RECORDS VIST FRAM
        ORDER BY PersonID, ToDate
    
    1 VERDI FRA BRUKER 1
    3 VERDIER FRA BRUKER 2
    2 VERDI FRA BRUKER 3

    FINNE UT LastFinishedDegreeDate FOR BRUKER 2 og 3

    WRITE A QUERY THAT WILL GIVE THE HIGHEST DATE FROM EACH USER(PersonID)


    HOW TO DO IT:
    SELECT
        PersonID, MAX(ToDate) AS LastFinishingDegreeDate
            FROM LoggLogg
            WHERE PersonID <= 10003
            GROUP BY PersonID


MORE BUILTIN FUNCTIONS


-- INTERNAL FUNCTIONS:
SELECT
    GETDATE() AS ,
    SUSER_SNAME() AS [Username],
    DB_NAME() AS [Database Name],
    @@SERVERNAME AS [Server Name],
    @@LANGUAGE AS [Language],
    @@VERSION AS [SQL Server Version],
    CHECKSUM('string) AS [CheckSum],
    NEWID() AS [NewID]                  if you want to have a unique identifier type, to generate a new one. 







AGGREGATES CONTINUE


















































MODIFDYING AND DEFINING
INSERT
UPDATE DELETE 
--USING OUTPUT




SELECT INTO - The SELECT INTO statement copies data from one table into a new table.
INSERT INTO - The INSERT INTO statement is used to insert new records in a table.
INSERT INTO SELECT - The INSERT INTO SELECT statement copies data from one table and inserts it into another table.



FOR UPDATING AND DELETING REMEMBER THE WHERE CLOSS









*/

