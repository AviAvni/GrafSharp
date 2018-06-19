// https://neo4j.com/graphgist/2dcbbe7f-a2e4-4a2b-ad1e-6c0a26efd6fd#listing_category=web-amp-social
// Create Alice
CREATE (alice:TorHost {
name:"alice-pc.onion",
isTorNode: True
})

// Create Dave
CREATE (dave:Server {
name:"dave-server.onion",
type: "Directory",
isTorNode: True
})

// Create Node1
CREATE (node1:Host {
name:"node1-router.onion",
isTorNode: False
})


// Create Node2
CREATE (node2:TorHost {
name:"node2-pc.onion",
isTorNode: True
})

// Create Node3
CREATE (node3:Host {
name:"node3-pc.onion",
isTorNode: False
})


// Create Node4
CREATE (node4:Host {
name:"node4-pc.onion",
isTorNode: True
})


// Create Node5
CREATE (node5:Host {
name:"node5-pc.onion",
isTorNode: False
})

// Create Bob
CREATE (bob:TorHost {
name:"bob-mac.onion",
isTorNode: True
})

// Create Bob
CREATE (chuck:TorHost {
name:"chuck-ubuntu.onion",
isTorNode: True
})

// Create Hidden service
CREATE (webServer:HiddenService {
name:"Web Server",
publicKey:"3048 0241 ...",
port: 9999
})

// Create Hidden service
CREATE (arm:Application {
name:"Anonymizing Relay Monitor"
})

// Connect Alice to Directory Server Dave
CREATE (alice)-[:DEPENDS_ON]->(dave)

// Connect Alice to Node-1
CREATE (alice)-[:CONNECTS]->(node1)

// Connect Node-1 to Node-2
CREATE (node1)-[:CONNECTS]->(node2)

// Connect Node-2 to Node-3
CREATE (node2)-[:CONNECTS]->(node3)

// Connect Node-3 to Bob
CREATE (node3)-[:CONNECTS]->(bob)

// Connect Chuck to Node-3
CREATE (chuck)-[:CONNECTS]->(node3)


// Connect Node-5 to Node-2
CREATE (node5)-[:CONNECTS]->(node2)

// Connect Node-4 to Node-5
CREATE (node4)-[:CONNECTS]->(node5)

// Connect Node-1 to Node-4
CREATE (node1)-[:CONNECTS]->(node4)

// Connect Node-3 to Node-4
CREATE (node3)-[:CONNECTS]->(node4)

// Connect Chuck to Node-3
CREATE (chuck)-[:CONNECTS]->(node3)

// Connect Chuck to ARM
CREATE (chuck)-[:RUNS]->(arm)

// Connect Bob to WebServer
CREATE (bob)-[:RUNS]->(webServer)