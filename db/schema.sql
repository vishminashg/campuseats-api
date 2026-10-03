-- CampusEats database schema + seed data (PostgreSQL)
-- Run this in the Neon SQL Editor (the API also creates it automatically on startup).
CREATE TABLE IF NOT EXISTS menu_items (
    id       SERIAL PRIMARY KEY,
    name     TEXT NOT NULL,
    price    NUMERIC(10,2) NOT NULL,
    category TEXT NOT NULL
);

INSERT INTO menu_items (name, price, category)
SELECT * FROM (VALUES
    ('Chicken Kottu',        850.00, 'Mains'),
    ('Vegetable Fried Rice', 650.00, 'Mains'),
    ('Egg Hopper',           120.00, 'Snacks'),
    ('Fish Cutlet',           90.00, 'Snacks'),
    ('Iced Milo',            300.00, 'Drinks')
) AS seed(name, price, category)
WHERE NOT EXISTS (SELECT 1 FROM menu_items);

SELECT * FROM menu_items ORDER BY id;
