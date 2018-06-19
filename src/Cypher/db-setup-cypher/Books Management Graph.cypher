// https://neo4j.com/graphgist/books-management-graph#listing_category=master-data-management
// Book

CREATE (harry1:Book{title:'Harry Potter and the Philosophers Stone'}),
       (bookThief:Book{title:'The Book Thief'}),
       (tom:Book{title:'The Adventures of Tom Sawyer'})

// Book Series
CREATE (harryPotter:BookSeries{title:'Harry Potter'})

// Keyword
CREATE (magic:Keyword{name:'magic'}),
       (students:Keyword{name:'students'}),
       (death:Keyword{name:'death'}),
       (judaism:Keyword{name:'judaism'}),
       (boy:Keyword{name:'boy'}),
       (change:Keyword{name:'change'}),
       (psychology:Keyword{name:'psychology'})

// Genre
CREATE (fantasy:Genre{name:'fantasy'}),
       (mystery:Genre{name:'mystery'}),
       (novel:Genre{name:'novel'}),
       (historical:Genre{name:'historical'}),
       (fiction:Genre{name:'fiction'}),
       (Bildungsroman:Genre{name:'Bildungsroman'}),
       (childrensNovel:Genre{name:'children\'s novel'})


//Person
CREATE (rowling:Person{name:'J. K.', surname:'Rowling'}),
       (menard:Person{name:'Jean-François', surname:'Ménard'}),
       (zusak:Person{name:'Markus', surname:'Zusak'}),
       (twain:Person{name:'Mark', surname:'Twain'})


//Translation
CREATE (harry1French:Translation{title:'Harry Potter Ã  lecole des sorciers'}),
       (harry1Polish:Translation{title:'Harry Potter i Kamień Filozoficzny'}),
       (harry1Spanish:Translation{title:'Harry Potter y la piedra filosofal '})


//Language
CREATE (english:Language{name:'English'}),
       (french:Language{name:'French'}),
       (polish:Language{name:'Polish'}),
       (spanish:Language{name:'Spanish'})


//Movie
CREATE (harry1Movie:Movie{title:'Harry Potter and the Philosophers Stone'}),
       (bookThiefMovie:Movie{title:'The Book Thief'})

//Time
CREATE (year1997:Time{year:'1997'}),
       (year2005:Time{year:'2005'}),
       (year1876:Time{year:'1876'}),
       (centuryXIX:Time{century:'XIX'}),
       (centuryXX:Time{century:'XX'})

//Place
CREATE (US:Place{country:'US'}),
        (UK:Place{country:'UK'}),
        (London:Place{cityName:'London'}),
        (Australia:Place{country:'Australia'}),
        (Germany:Place{country:'Germany'})

//Publishing House
CREATE (bloomsbury:PublishingHouse{name:'Bloomsbury Publishing'}),
       (levine:PublishingHouse{name:'Arthur A. Levine Books'}),
       (picador:PublishingHouse{name:'Picador'}),
       (apc:PublishingHouse{name:'American Publishing Company'})

//IS_PART_OF
CREATE	 (harry1)-[:IS_PART_OF]->(harryPotter)

//DESCRIBED_BY
CREATE	 (harry1)-[:DESCRIBED_BY]->(magic),
         (harry1)-[:DESCRIBED_BY]->(students),
         (bookThief)-[:DESCRIBED_BY]->(students),
         (bookThief)-[:DESCRIBED_BY]->(death),
         (bookThief)-[:DESCRIBED_BY]->(judaism),
         (tom)-[:DESCRIBED_BY]->(boy),
         (tom)-[:DESCRIBED_BY]->(psychology),
         (tom)-[:DESCRIBED_BY]->(change)

//OF_TYPE
CREATE	 (harry1)-[:OF_TYPE]->(fantasy),
         (harry1)-[:OF_TYPE]->(mystery),
         (harry1)-[:OF_TYPE]->(fiction),
         (bookThief)-[:OF_TYPE]->(novel),
         (bookThief)-[:OF_TYPE]->(historical),
         (bookThief)-[:OF_TYPE]->(fiction),
         (tom)-[:OF_TYPE]->(novel),
         (tom)-[:OF_TYPE]->(Bildungsroman),
         (tom)-[:OF_TYPE]->(childrensNovel)

//WRITTEN_BY
CREATE	 (harry1)-[:WRITTEN_BY]->(rowling),
         (bookThief)-[:WRITTEN_BY]->(zusak),
         (tom)-[:WRITTEN_BY]->(twain)

//TRANSLATED_TO
CREATE	 (harry1)-[:TRANSLATED_TO]->(harry1French),
         (harry1)-[:TRANSLATED_TO]->(harry1Polish),
         (harry1)-[:TRANSLATED_TO]->(harry1Spanish)

//HAS_ORIGINAL_LANGUAGE
CREATE	 (harry1)-[:HAS_ORIGINAL_LANGUAGE]->(english),
         (bookThief)-[:HAS_ORIGINAL_LANGUAGE]->(english),
         (tom)-[:HAS_ORIGINAL_LANGUAGE]->(english)

//OF_LANGUAGE
CREATE	 (harry1French)-[:OF_LANGUAGE]->(french),
         (harry1Polish)-[:OF_LANGUAGE]->(polish),
         (harry1Spanish)-[:OF_LANGUAGE]->(spanish)

//MADE_BY
CREATE	 (harry1French)-[:MADE_BY]->(menard)

//MADE_INTO
CREATE	 (harry1)-[:MADE_INTO]->(harry1Movie),
         (bookThief)-[:MADE_INTO]->(bookThiefMovie)

//WHEN_ACTION
CREATE	 (harry1)-[:WHEN_ACTION]->(centuryXX),
         (bookThief)-[:WHEN_ACTION]->(centuryXX),
         (tom)-[:WHEN_ACTION]->(centuryXIX)

//WHEN_PUBLISHED
CREATE	 (harry1)-[:WHEN_PUBLISHED]->(year1997),
         (bookThief)-[:WHEN_PUBLISHED]->(year2005),
         (tom)-[:WHEN_PUBLISHED]->(year1876)

//WHERE_ACTION
CREATE	 (harry1)-[:WHERE_ACTION]->(London),
         (bookThief)-[:WHERE_ACTION]->(Germany),
         (tom)-[:WHERE_ACTION]->(US)

//WHERE_PUBLISHED
CREATE	 (harry1)-[:WHERE_PUBLISHED]->(UK),
         (bookThief)-[:WHERE_PUBLISHED]->(Australia),
         (tom)-[:WHERE_PUBLISHED]->(US)

//PUBLISHED_BY
CREATE	 (harry1)-[:PUBLISHED_BY]->(bloomsbury),
         (bookThief)-[:PUBLISHED_BY]->(picador),
         (tom)-[:PUBLISHED_BY]->(apc)