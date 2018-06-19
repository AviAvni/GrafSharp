---------------------------------------------------------
|                                                       |
| gRAF# - (graph-sharp, R-Ricarrdo, A-Avi, F#-F# :) )   |
|                                                       |
---------------------------------------------------------

// Syntax

query := 
    | matchClause
	| createClause
    | mergeClause
    | setClause
    | returnClause

matchClause := "MATCH" pattern query

createClause := "CREATE" pattern query?

mergeClause := "MERGE" pattern query?

setClause := "SET" varExpr "." varExpr "=" expr query?

returnClause := "RETURN" expr

pattern :=
    | nodePattern
    | relationPatten

expr :=
    | literalExpr
    | varExpr
    | functionExpr
	| nameExpr
    | complexExpr
	| patternExpr

literalExpr : =
    | number
    | string
    | datetime
	| bool
    | guid
    | null
	| list
	| object

list := "[" literalExpr "]"

object := "{" (varExpr ":" expr)^"," "}"

varExpr := letter+

functionExpr := varExpr "(" expr ")"

nodePattern : "(" varExpr? (":" labelPattern)* object? ")"

relationPattern := 
    | nodePattern "<-[" varExpr? (":" labelPattern)* object? "]-" nodePattern // in
    | nodePattern "-[" varExpr? (":" labelPattern)* object? "]->" nodePattern // out
    | nodePattern "-[" varExpr? (":" labelPattern)* object? "]-" nodePattern // in or out

nameExpr := pattern "AS" varExpr

complexExpr :=
    | expr "AND" expr
    | expr "OR" expr
    | "NOT" expr
    | "(" expr ")"

patternExpr := pattern

// Semantics

MATCH (a)
RETURN (a)
--------
[a]

MATCH (a)-[b]-(c)
RETURN a?, b?, c?
--------
[a?, b?, c?]

MATCH (a)
MATCH (b)
RETURN a?, b?
-----------
[a]?, [b]?

MATCH (a)-[b]-(c)
MATCH (d)
RETURN a?, b?, c?, d?
-----------
[a?, b?, c?], [d]?

MATCH (a)
MATCH (b)-[c]-(d)
RETURN a?, b?, c?, d?
-----------
[a]?, [b?, c?, d]?

MATCH (a)-[b]-(c)
MATCH (d)-[e]-(f)
RETURN a?, b?, c?, d?, e?, f?
------------
[a?, b?, c?], [d?, e?, f?]

MATCH (a)-[b]-(c)
MATCH (a)-[d]-(e)
RETURN a?, b?, c?, d?, e?
------------
[a?, [b?, c?], [d?, e?]]


MATCH (a)-[b]-(c)
MATCH (a)-[d]-(c)
RETURN a?, b?, c?, d?, e?
------------
[a?, c?, [b]?, [d]?]

​