-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';
	
-- -----------------------------------------------------
-- Schema MealMind
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema MealMind
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `MealMind` DEFAULT CHARACTER SET utf8 ;
USE `MealMind` ;

-- -----------------------------------------------------
-- Table `MealMind`.`Categorie`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Categorie` (
  `idCategorie` INT NOT NULL AUTO_INCREMENT,
  `CategorieNaam` MEDIUMTEXT NOT NULL,
  PRIMARY KEY (`idCategorie`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`Recept`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Recept` (
  `idRecept` INT NOT NULL AUTO_INCREMENT,
  `ReceptNaam` MEDIUMTEXT NOT NULL,
  `ReceptBeshrijving` VARCHAR(45) NOT NULL,
  PRIMARY KEY (`idRecept`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`Ingredient`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Ingredient` (
  `idIngredient` INT NOT NULL AUTO_INCREMENT,
  `IngredientNaam` MEDIUMTEXT NOT NULL,
  PRIMARY KEY (`idIngredient`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`Recept_has_Ingredient`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Recept_has_Ingredient` (
  `FkRecept` INT NOT NULL,
  `FkIngredient` INT NOT NULL,
  `HoeveelheidIngredient` MEDIUMTEXT NOT NULL,
  PRIMARY KEY (`FkRecept`, `FkIngredient`),
  INDEX `fk_Recept_has_Ingredient_Ingredient1_idx` (`FkIngredient` ASC) VISIBLE,
  INDEX `fk_Recept_has_Ingredient_Recept1_idx` (`FkRecept` ASC) VISIBLE,
  CONSTRAINT `fk_Recept_has_Ingredient_Recept1`
    FOREIGN KEY (`FkRecept`)
    REFERENCES `MealMind`.`Recept` (`idRecept`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Recept_has_Ingredient_Ingredient1`
    FOREIGN KEY (`FkIngredient`)
    REFERENCES `MealMind`.`Ingredient` (`idIngredient`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`Gebruiker`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Gebruiker` (
  `idGebruiker` INT NOT NULL AUTO_INCREMENT,
  `GebruikerUsername` MEDIUMTEXT NOT NULL,
  `GebruikerEmail` MEDIUMTEXT NOT NULL,
  `GebruikerPasswordHash` MEDIUMTEXT NOT NULL,
  
  
  PRIMARY KEY (`idGebruiker`))
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`GebruikerKiestRecept`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`GebruikerKiestRecept` (
  `FkRecept` INT NOT NULL,
  `FkGebruiker` INT NOT NULL,
  `Datum` DATE NULL,
  PRIMARY KEY (`FkRecept`, `FkGebruiker`),
  INDEX `fk_Recept_has_Gebruiker_Gebruiker1_idx` (`FkGebruiker` ASC) VISIBLE,
  INDEX `fk_Recept_has_Gebruiker_Recept1_idx` (`FkRecept` ASC) VISIBLE,
  CONSTRAINT `fk_Recept_has_Gebruiker_Recept1`
    FOREIGN KEY (`FkRecept`)
    REFERENCES `MealMind`.`Recept` (`idRecept`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Recept_has_Gebruiker_Gebruiker1`
    FOREIGN KEY (`FkGebruiker`)
    REFERENCES `MealMind`.`Gebruiker` (`idGebruiker`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


-- -----------------------------------------------------
-- Table `MealMind`.`Categorie_has_Recept`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `MealMind`.`Categorie_has_Recept` (
  `FkCategorie` INT NOT NULL,
  `FkRecept` INT NOT NULL,
  PRIMARY KEY (`FkCategorie`, `FkRecept`),
  INDEX `fk_Categorie_has_Recept_Recept1_idx` (`FkRecept` ASC) VISIBLE,
  INDEX `fk_Categorie_has_Recept_Categorie1_idx` (`FkCategorie` ASC) VISIBLE,
  CONSTRAINT `fk_Categorie_has_Recept_Categorie1`
    FOREIGN KEY (`FkCategorie`)
    REFERENCES `MealMind`.`Categorie` (`idCategorie`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION,
  CONSTRAINT `fk_Categorie_has_Recept_Recept1`
    FOREIGN KEY (`FkRecept`)
    REFERENCES `MealMind`.`Recept` (`idRecept`)
    ON DELETE NO ACTION
    ON UPDATE NO ACTION)
ENGINE = InnoDB;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
