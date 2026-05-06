
use mealmind;

#inserts voor alle tabellen
INSERT into categorie (CategorieNaam) VALUE ("Vlees"),("Vis"),("Vegetarisch"),("vegan"),("Kip"),("rund"),("varkensvlees"),("Snel klaar"),("lang"),("makkelijk"),("gemiddeld"),("moeilijk"),("gezond"),("Koolhydraatarm"),("Eiwitrijk"),("Glutenvrij"),("Lactosevrij");
INSERT into gebruiker (GebruikerUsername, GebruikerEmail,GebruikerPasswordHash) value ("Mats","Mats@gmail.com","1234");
INSERT into gebruiker (GebruikerUsername, GebruikerEmail,GebruikerPasswordHash) value ("Pieter","Parker@gmail.com","1234");
INSERT into gebruiker (GebruikerUsername, GebruikerEmail,GebruikerPasswordHash) value ("Diogo","Machado@gmail.com","1234");
INSERT into gebruiker (GebruikerUsername, GebruikerEmail,GebruikerPasswordHash) value ("owen","OffTheStreets@gmail.com","1234");
INSERT INTO ingredient (IngredientNaam) VALUE ("Tomaat"),("Ui"),("Knoflook"),("Paprika"),("Wortel"),("Aardappel"),("Rijst"),("Pasta"),("Kipfilet"),("Rundergehakt"),("Varkensvlees"),("Zalm"),("Ei"),("Melk"),("Kaas"),("Room"),("Olijfolie"),("Boter"),("Bloem"),("Suiker");

INSERT INTO recept (ReceptNaam, ReceptBeshrijving) VALUE
("Spaghetti bolognese","Klassieke pasta met tomatensaus en rund"),
("Kip curry","Pittige curry met kip en rijst"),
("Zalm uit de oven","Zalmfilet met kruiden en groenten"),
("Vegetarische lasagne","Lasagne met groenten en tomatensaus"),
("Omelet natuur","Snelle omelet met eieren"),
("Stoofvlees","Traditioneel Belgisch stoofvlees"),
("Pasta pesto","Snelle pasta met pesto en kaas"),
("Salade kip","Frisse salade met gegrilde kip");

INSERT INTO recept_has_ingredient (FkRecept, FkIngredient, HoeveelheidIngredient) VALUE
(1,1,"400 g"),
(1,2,"150 g"),
(1,4,"400 g"),
(1,10,"800 g");

INSERT INTO categorie_has_recept (FkCategorie, FkRecept) VALUE
(5, 1),  -- Rund
(8, 1);  -- Snel 

INSERT into gebruikerkiestrecept (FkRecept,FkGebruiker,Datum) value
(1, 1, '2026-01-16'),
(2, 1, '2026-01-17'),
(3, 1, '2026-01-18'),
(4, 1, '2026-01-19'),
(5, 1, '2026-01-20');








