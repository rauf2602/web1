CREATE DATABASE IF NOT EXISTS store_manager_db
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE store_manager_db;

CREATE TABLE IF NOT EXISTS inventory_items (
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(150) NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    Date DATETIME NOT NULL,
    PRIMARY KEY (Id)
);
