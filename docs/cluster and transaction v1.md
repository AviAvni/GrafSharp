---------------------------------------------------------
|                                                       |
| GraphActor - Clustering and Transactions              |
|                                                       |
---------------------------------------------------------

// Clustering

1. only one node is writeable(leader) and optionally many read-only nodes(followers)
2. election of leader
3. heartbeat for leader


// Transaction

The problem start with write transaction so 

1. all nodes in the cluster track query number like [Graph version].[Node id].[Transaction id]
2. Graph version increment only when write happend and is sync in all nodes and transaction id reset to 0
3. Node id is constant for each node
4. Trnsaction id  increment on each query
5. Resolve query conflict
    a. if the transaction [Graph version] is lower use the lower version to complete the query
	b. if the transaction [Graph version] is higher wait the write to be complete