// https://neo4j.com/graphgist/medicine-amp-drugs-classification-for-the-central-hospital-of-asturias#listing_category=health-care-and-science
// Create ATC - Anatomic-Therapeutic-Quimic national classifier
CREATE (atc:NationalClassifier {name:'ATC', description:'Spanish classifier for Anatomic-Therapeutic-Quimic medicines'})

// Create Anatomic-Functional Areas
CREATE
    (a:AnatomicFunctionalArea {code:'A', name:'Digestive system and metabolism'}),
    (b:AnatomicFunctionalArea {code:'B', name:'Blood and blood-forming organs'}),
    (c:AnatomicFunctionalArea {code:'C', name:'Cardiovascular system'})

// Create their relationships with the ATC national classifier
CREATE
    (atc)-[:HAS_ANATOMIC_FUNCTIONAL_AREA]->(a),
    (atc)-[:HAS_ANATOMIC_FUNCTIONAL_AREA]->(b),
    (atc)-[:HAS_ANATOMIC_FUNCTIONAL_AREA]->(c)

// Create mechanism of action for each anatomic-functional area and their relationships
// AFA - A
CREATE
(a02ba:MechanismOfAction {code:'A02BA', name:'Anti ulcer:H2 receptor antagonists'}),
    (a)-[:MECHANISM_OF_ACTION]->(a02ba)

// AFA - B
CREATE
    (b01ab:MechanismOfAction {code:'B01AB', name:'Antithrombotic:Heparin and derivatives'}),
    (b)-[:MECHANISM_OF_ACTION]->(b01ab)

// AFA - C
CREATE
    (c01bd:MechanismOfAction {code:'C01BD', name:'Antiarrhythmics. Class III'}),
    (c)-[:MECHANISM_OF_ACTION]->(c01bd)

// 	Create active ingredients and their relationships
// A02BA active ingredients
CREATE
// Famotidine
    (famo:ActiveIngredient {name:'Famotidine'}),
    (a02ba)-[:HAS_ACTIVE_INGREDIENT]->(famo),
// Ranitidine
    (rani:ActiveIngredient {name:'Ranitidine'}),
    (a02ba)-[:HAS_ACTIVE_INGREDIENT]->(rani)

// B01AB active ingredients
CREATE
// Bemiparina
    (bemi:ActiveIngredient {name:'Bemiparina'}),
    (b01ab)-[:HAS_ACTIVE_INGREDIENT]->(bemi),
// Dalteparin
    (dalte:ActiveIngredient {name:'Dalteparin'}),
    (b01ab)-[:HAS_ACTIVE_INGREDIENT]->(dalte)

// C01BD active ingredients
CREATE
// Amiodarone
    (amio:ActiveIngredient {name:'Amiodarone'}),
    (c01bd)-[:HAS_ACTIVE_INGREDIENT]->(amio),
// Dronedarone
    (drone:ActiveIngredient {name:'Dronedarone'}),
    (c01bd)-[:HAS_ACTIVE_INGREDIENT]->(drone)

// Create differents posologies for the active ingredients
// Famotidine posologies
CREATE
    (famo10mg:Posology {code:'A01486', name:'Famotidine 10 mg'}),
    (famo)-[:HAS_POSOLOGY]->(famo10mg),
    (famo20mg:Posology {code:'A01488', name:'Famotidine 20 mg'}),
    (famo)-[:HAS_POSOLOGY]->(famo20mg)

// Ranitidine posologies
CREATE
    (rani150mg:Posology {code:'A03462', name:'Ranitidine 150 mg tablets'}),
    (rani)-[:HAS_POSOLOGY]->(rani150mg),
    (rani300mg:Posology {code:'A03464', name:'Ranitidine 300 mg tablets'}),
    (rani)-[:HAS_POSOLOGY]->(rani300mg)

// Bemiparina posologies
CREATE
    (bemi3500:Posology {code:'A00336', name:'Bemiparina 3500 UI injectable SC'}),
    (bemi)-[:HAS_POSOLOGY]->(bemi3500),
    (bemi5000:Posology {code:'A00337', name:'Bemiparina 5000 UI injectable SC'}),
    (bemi)-[:HAS_POSOLOGY]->(bemi5000)

// Dalterparin posologies
CREATE
    (dalte2500:Posology {code:'A00938', name:'Dalterparin 2500 UI injectable SC'}),
    (dalte)-[:HAS_POSOLOGY]->(dalte2500),
    (dalte5000:Posology {code:'A00939', name:'Dalterparin 5000 UI injectable SC'}),
    (dalte)-[:HAS_POSOLOGY]->(dalte5000)

// Amiodarone posologies
CREATE
    (amio150mg:Posology {code:'A00164', name:'Amiodarone 150 mg injectable IV'}),
    (amio)-[:HAS_POSOLOGY]->(amio150mg),
    (amio200mg:Posology {code:'A00165', name:'Amiodarone 200 mg tablets'}),
    (amio)-[:HAS_POSOLOGY]->(amio200mg)

// Dronedarone posologies
CREATE
    (drone400mg:Posology {code:'A01201', name:'Dronedarone 400 mg tablets'}),
    (drone)-[:HAS_POSOLOGY]->(drone400mg)

// Create differents pharmaceutical specialities and their relationships
// Famotidine 10 mg
CREATE
    (pepcid:PharmaceuticalSpeciality {name:'Pepcid 12 tablets', code:'6601426', price:'3'}),
    (eviantrina:PharmaceuticalSpeciality {name:'Eviantrina 12 tablets', code:'8182374', price:'2'}),
    (gastenin:PharmaceuticalSpeciality {name:'Gastenin 14 tablets', code:'7068877', price:'4'}),
    (famogenom:PharmaceuticalSpeciality {name:'Famogenom 14 tablets', code:'7419839', price:'4'}),

    (famo10mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(pepcid),
    (famo10mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(eviantrina),
    (famo10mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(gastenin),
    (famo10mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(famogenom)

// Famotidine 20 mg
CREATE
    (bexal:PharmaceuticalSpeciality {name:'Bexal 20 tablets', code:'7881254', price:'5'}),
    (ranbaxy:PharmaceuticalSpeciality {name:'Ranbaxy EFG 20 tablets', code:'8266449', price:'5'}),
    (esteve:PharmaceuticalSpeciality {name:'Esteve 28 tablets', code:'8675159', price:'6'}),
    (geminis:PharmaceuticalSpeciality {name:'Geminis 28 tablets', code:'7419839', price:'5'}),

    (famo20mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(bexal),
    (famo20mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(ranbaxy),
    (famo20mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(esteve),
    (famo20mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(geminis)

// Ranitidine 150 mg tablets
CREATE
    (zantac:PharmaceuticalSpeciality {name:'Zantac 28 tablets', code:'6540206', price:'5'}),
    (ranidin:PharmaceuticalSpeciality {name:'Ranidin 28 tablets', code:'6549131', price:'6'}),

    (rani150mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(zantac),
    (rani150mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(ranidin)

// Ranitidine 300 mg tablets
CREATE
    (terposen:PharmaceuticalSpeciality {name:'Terposen 28 tablets', code:'6541869', price:'4'}),
    (ranix:PharmaceuticalSpeciality {name:'Ranix 28 tablets', code:'6548301', price:'8'}),

    (rani300mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(terposen),
    (rani300mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(ranix)

// Bemiparina 3500 UI
CREATE
    (hibor3500:PharmaceuticalSpeciality {name:'Hibor 3500 UI 30 syringes precharged 0.2 ml', code:'6632086', price:'160'}),
    (afatinal3500:PharmaceuticalSpeciality {name:'Afatinal 3500 UI 30 syringes precharged 0.2 ml', code:'6584446', price:'170'}),

    (bemi3500)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(hibor3500),
    (bemi3500)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(afatinal3500)

// Bemiparina 5000 UI
CREATE
    (hibor5000:PharmaceuticalSpeciality {name:'Hibor 5000 UI 30 syringes precharged 0.2 ml', code:'7779872', price:'180'}),
    (afatinal5000:PharmaceuticalSpeciality {name:'Afatinal 5000 UI 30 syringes precharged 0.2 ml', code:'6584477', price:'190'}),

    (bemi5000)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(hibor5000),
    (bemi5000)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(afatinal5000)

// Dalteparin 2500 UI
CREATE
    (fragmin2500:PharmaceuticalSpeciality {name:'Fragmin 2500 UI 100 syringes precharged 0.2 ml', code:'6402191', price:'650'}),
    (boxol2500:PharmaceuticalSpeciality {name:'Boxol 2500 UI 100 syringes 0.2 ml', code:'6393024', price:'610'}),

    (dalte2500)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(fragmin2500),
    (dalte2500)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(boxol2500)

// Dalteparin 5000 UI
CREATE
    (fragmin5000:PharmaceuticalSpeciality {name:'Fragmin 5000 UI 100 syringes precharged 0.2 ml', code:'6402276', price:'695'}),
    (boxol5000:PharmaceuticalSpeciality {name:'Boxol 5000 UI 100 syringes 0.2 ml', code:'6393109', price:'605'}),

    (dalte5000)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(fragmin5000),
    (dalte5000)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(boxol5000)

// Amiodarone 150 mg tablets
CREATE
    (trangorex:PharmaceuticalSpeciality {name:'Trangorex 30 tablets', code:'6711569', price:'5'}),
    (amiodarone:PharmaceuticalSpeciality {name:'Amiodarone 30 tablets', code:'A001655', price:'4'}),

    (amio150mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(trangorex),
    (amio150mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(amiodarone)

// Dronedarone 400 mg tablets
CREATE
    (multaq:PharmaceuticalSpeciality {name:'Multaq 60 tablets', code:'6643433', price:'105'}),
    (dronedarone:PharmaceuticalSpeciality {name:'Dronedarone 60 tablets', code:'360600', price:'100'}),

    (drone400mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(multaq),
    (drone400mg)-[:HAS_PHARMACEUTICAL_SPECIALITY]->(dronedarone)