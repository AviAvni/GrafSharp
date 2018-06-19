// https://neo4j.com/graphgist/winter-network#listing_category=sports-and-recreation
CREATE (:Database {name:'Neo4j'})-[:SUPPORTS]->(:Language {name:'Cypher'})

CREATE (FuentesDeInvierno:Station {name:'fuentes de invierno', numberOfSlopes:15, numremontes:5})
CREATE (Abedules:Slope {name:'Abedules', difficulty:'Red'})
CREATE (Brezales:Slope {name:'Brezales', difficulty:'Blue'})
CREATE (PequenioLaurel:Slope {name:'Pequeño laurel', difficulty:'Green'})
CREATE (Toneos:Slope {name:'Toneo', difficulty:'Green'})
CREATE (LaHolla:Slope {name:'La Hoya', difficulty:'Red'})
CREATE (Rebecos:Slope {name:'Rebecos', difficulty:'Black'})

CREATE (SanIsidro:Station {name:'San Isidro', numberOfSlopes:31, numremontes:15})
CREATE (LosPiornos:Slope {name:'Los Piornos', difficulty:'Blue'})
CREATE (LaPerdiz:Slope {name:'La Perdiz', difficulty:'Red'})
CREATE (LaTortuga:Slope {name:'La tortuga', difficulty:'Blue'})
CREATE (GranCanion:Slope {name:'Gran Cañón', difficulty:'Red'})
CREATE (LaCollada:Slope {name:'La Collada', difficulty:'Black'})
CREATE (laTravesia:Slope {name:'La Travesía', difficulty:'Blue'})
CREATE (laSolana:Slope {name:'La Solana', difficulty:'Black'})

CREATE (Manzaneda:Station {name:'Manzaneda', numberOfSlopes:25, numremontes:5})
CREATE (eixo:Slope {name:'Xeixo', difficulty:'Green'})
CREATE (Baqueira:Station {name:'Baqueira Beret', numberOfSlopes:100, numremontes:55})
CREATE (Cerler:Station {name:'Cerler', numberOfSlopes:68, numremontes:19})
CREATE (Formigal:Station {name:'Formigal', numberOfSlopes:147, numremontes:37})

CREATE (Cantabrica:Place {name:'Cordilera Cantábrica'})
CREATE (Catalan:Place {name:'Pirineo Catalán'})
CREATE (Aragones:Place {name:'Pirineo Aragonés'})

CREATE
  (FuentesDeInvierno)-[:LOCATED_IN]->(Cantabrica),
  (SanIsidro)-[:LOCATED_IN]->(Cantabrica),
  (Manzaneda)-[:LOCATED_IN]->(Cantabrica),
  (Baqueira)-[:LOCATED_IN]->(Catalan),
  (Cerler)-[:LOCATED_IN]->(Aragones),
  (Formigal)-[:LOCATED_IN]->(Aragones),

  (Abedules)-[:BELONGS_TO]->(FuentesDeInvierno),
  (Brezales)-[:BELONGS_TO]->(FuentesDeInvierno),
  (PequenioLaurel)-[:BELONGS_TO]->(FuentesDeInvierno),
  (Toneos)-[:BELONGS_TO]->(FuentesDeInvierno),
  (LaHolla)-[:BELONGS_TO]->(FuentesDeInvierno),
  (Rebecos)-[:BELONGS_TO]->(FuentesDeInvierno),
  (LosPiornos)-[:BELONGS_TO]->(SanIsidro),
  (LaPerdiz)-[:BELONGS_TO]->(SanIsidro),
  (LaTortuga)-[:BELONGS_TO]->(SanIsidro),
  (GranCanion)-[:BELONGS_TO]->(SanIsidro),
  (LaCollada)-[:BELONGS_TO]->(SanIsidro),
  (laTravesia)-[:BELONGS_TO]->(SanIsidro),
  (laSolana)-[:BELONGS_TO]->(SanIsidro),
  (eixo)-[:BELONGS_TO]->(Manzaneda)
  
CREATE (Marilyn:Person {name:"Norma Jean Baker Mortenson", born:1926})
CREATE (Kennedy:Person {name:'John Fitzgerald Kennedy', born:1917})
CREATE (Bobby:Person {name:'Robert Francis Kennedy', born:1925})
CREATE (Tesla:Person {name:'Nikola Tesla', born:1856})
CREATE (Chaplin:Person {name:'Charles Spencer Chaplin', born:1889})
CREATE (SteveJobs:Person {name:'Steven Paul Jobs', born:1955})
CREATE (BillGates:Person {name:'William Henry Gates', born:1955})
CREATE (Dalí:Person {name:'Salvador Felipe Jacinto Dalí', born:1904})
CREATE (Faraday:Person {name:'Michael Faraday', born:1791})
CREATE (Turing:Person {name:'Alan Mathison Turing', born:1912})
CREATE (Edwar:Person {name:'Edward Snowden', born:1983})

CREATE
  //(Marilyn)-[:SKIED_IN {modalidad:'Snow', rating:10, coment:'this slope is the best!!'}]->(laSolana),
  (Marilyn)-[:SKIED_IN {modalidad:'Snow'}]->(laSolana),
  (Marilyn)-[:SKIED_IN {modalidad:'Esquí'}]->(Formigal),
  (Kennedy)-[:SKIED_IN {modalidad:'Esquí'}]->(eixo),
  (Tesla)-[:SKIED_IN {modalidad:'Esquí'}]->(GranCanion),
  (Tesla)-[:SKIED_IN {modalidad:'Esquí'}]->(LaCollada),
  (Tesla)-[:SKIED_IN {modalidad:'Esquí'}]->(laSolana),
  (Tesla)-[:SKIED_IN {modalidad:'Snow'}]->(Cerler),
  (SteveJobs)-[:SKIED_IN {modalidad:'Esquí'}]->(Abedules),
  (Edwar)-[:SKIED_IN {modalidad:'Esquí'}]->(LaTortuga),
  (SteveJobs)-[:SKIED_IN {modalidad:'Esquí'}]->(Baqueira),
  (BillGates)-[:SKIED_IN {modalidad:'Esquí'}]->(Baqueira),
  (Faraday)-[:SKIED_IN {modalidad:'Esquí'}]->(Manzaneda),
  (Turing)-[:SKIED_IN {modalidad:'Snow'}]->(Rebecos)

 CREATE (LosAngeles:Place {name:'Los Ángeles'})
 CREATE (California:Place {name:'California'})
 CREATE (EstadosUnidos:Place {name:'United States'})
 CREATE (Brookline:Place {name:'Brookline'})
 CREATE (Massachussets:Place {name:'Massachussets'})
 CREATE (Croacia:Place {name:'Croacia'})
 CREATE (Londres:Place {name:'Londres'})
 CREATE (Inglaterra:Place {name:'Inglaterra'})
 CREATE (SanFrancisco:Place {name:'San Francisco'})
 CREATE (Asturias:Place {name:'Asturias'})
 CREATE (Leon:Place {name:'Castilla León'})
 CREATE (Catalunia:Place {name:'Catalunya'})
 CREATE (Espania:Place {name:'España'})
 CREATE (Seattle:Place {name:'Seattle'})
 CREATE (Washington:Place {name:'Washington'})

CREATE
  (Marilyn)-[:BORN_IN]->(LosAngeles),
  (Kennedy)-[:BORN_IN]->(Brookline),
  (Bobby)-[:BORN_IN]->(Croacia),
  (Tesla)-[:BORN_IN]->(Londres),
  (Chaplin)-[:BORN_IN]->(SanFrancisco),
  (SteveJobs)-[:BORN_IN]->(Catalunia),
  (BillGates)-[:BORN_IN]->(Seattle),
  (Faraday)-[:BORN_IN]->(Londres),
  (Turing)-[:BORN_IN]->(Londres),
  (Edwar)-[:BORN_IN]->(EstadosUnidos)

CREATE
  (Asturias)-[:LOCATED_IN]->(Espania),
  (Leon)-[:LOCATED_IN]->(Espania),
  (LosAngeles)-[:LOCATED_IN]->(California),
  (California)-[:LOCATED_IN]->(EstadosUnidos),
  (Brookline)-[:LOCATED_IN]->(Massachussets),
  (Massachussets)-[:LOCATED_IN]->(EstadosUnidos),
  (Londres)-[:LOCATED_IN]->(Inglaterra),
  (SanFrancisco)-[:LOCATED_IN]->(California),
  (Catalunia)-[:LOCATED_IN]->(Espania),
  (Cantabrica)-[:LOCATED_IN]->(Asturias),
  (Cantabrica)-[:LOCATED_IN]->(Leon),
  (Catalan)-[:LOCATED_IN]->(Catalunia),
  (Aragones)-[:LOCATED_IN]->(Espania)

CREATE
  (Marilyn)-[:KNOWN]->(Kennedy),
  (Marilyn)-[:KNOWN]->(Bobby),
  (Kennedy)-[:KNOWN]->(Marilyn),
  (Bobby)-[:KNOWN]->(Marilyn),
  (Bobby)-[:KNOWN]->(Edwar),
  (Faraday)-[:KNOWN]->(Edwar),
  (BillGates)-[:KNOWN]->(Edwar),
  (Tesla)-[:KNOWN]->(Edwar),
  (Marilyn)-[:LIKES]->(Kennedy),
  (Bobby)-[:DISLIKES]->(Marilyn),
  (Tesla)-[:LIKES]->( Marilyn),
  (Chaplin)-[:LIKES]->( Marilyn),
  (Faraday)-[:LIKES]->( Marilyn)

RETURN FuentesDeInvierno
;