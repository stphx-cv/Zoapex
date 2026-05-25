-- =============================================================
-- Zoapex — Script de base de datos para Supabase / PostgreSQL
-- Ejecutar completo en el SQL Editor de Supabase
-- =============================================================

-- ---------------------------------------------------------------
-- TABLAS
-- ---------------------------------------------------------------

CREATE TABLE IF NOT EXISTS category (
    category_id SERIAL PRIMARY KEY,
    name        VARCHAR(50)  NOT NULL,
    description VARCHAR(150),
    status      SMALLINT     NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS product (
    product_id  SERIAL PRIMARY KEY,
    code        VARCHAR(10)    UNIQUE,
    name        VARCHAR(80)    NOT NULL,
    description VARCHAR(200),
    price       NUMERIC(10,2)  NOT NULL CHECK (price > 0),
    stock       INT            NOT NULL DEFAULT 0 CHECK (stock >= 0),
    min_stock   INT            NOT NULL DEFAULT 0 CHECK (min_stock >= 0),
    category_id INT            NOT NULL REFERENCES category(category_id),
    image_url   VARCHAR(300),
    status      SMALLINT       NOT NULL DEFAULT 1,
    registered_at TIMESTAMP    NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS customer (
    customer_id SERIAL PRIMARY KEY,
    first_name  VARCHAR(80)  NOT NULL,
    last_name   VARCHAR(80)  NOT NULL,
    email       VARCHAR(120) UNIQUE,
    phone       VARCHAR(15),
    address     VARCHAR(200),
    status      SMALLINT     NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS "order" (
    order_id    SERIAL PRIMARY KEY,
    code        VARCHAR(12)   UNIQUE,
    customer_id INT           REFERENCES customer(customer_id),
    order_date  TIMESTAMP     NOT NULL DEFAULT NOW(),
    subtotal    NUMERIC(10,2) NOT NULL DEFAULT 0,
    tax         NUMERIC(10,2) NOT NULL DEFAULT 0,
    total       NUMERIC(10,2) NOT NULL DEFAULT 0,
    status      SMALLINT      NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS order_detail (
    detail_id   SERIAL PRIMARY KEY,
    order_id    INT           NOT NULL REFERENCES "order"(order_id),
    product_id  INT           NOT NULL REFERENCES product(product_id),
    quantity    INT           NOT NULL CHECK (quantity > 0),
    unit_price  NUMERIC(10,2) NOT NULL,
    subtotal    NUMERIC(10,2) NOT NULL
);

-- ---------------------------------------------------------------
-- FUNCIONES (equivalente a Stored Procedures)
-- ---------------------------------------------------------------

-- fn_list_categories: Lista categorías activas para combos desplegables
CREATE OR REPLACE FUNCTION fn_list_categories()
RETURNS TABLE (
    category_id  INT,
    name         VARCHAR(50),
    description  VARCHAR(150),
    status       SMALLINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.category_id, c.name, c.description, c.status
    FROM category c
    WHERE c.status = 1
    ORDER BY c.name;
END;
$$ LANGUAGE plpgsql;

-- fn_list_products: Lista todos los productos activos con nombre de categoría
CREATE OR REPLACE FUNCTION fn_list_products()
RETURNS TABLE (
    product_id    INT,
    code          VARCHAR(10),
    name          VARCHAR(80),
    description   VARCHAR(200),
    price         NUMERIC(10,2),
    stock         INT,
    min_stock     INT,
    category_id   INT,
    category_name VARCHAR(50),
    image_url     VARCHAR(300),
    status        SMALLINT,
    registered_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.product_id, p.code, p.name, p.description,
        p.price, p.stock, p.min_stock,
        p.category_id, c.name AS category_name,
        p.image_url, p.status, p.registered_at
    FROM product p
    INNER JOIN category c ON p.category_id = c.category_id
    WHERE p.status = 1
    ORDER BY p.code;
END;
$$ LANGUAGE plpgsql;

-- fn_get_product: Devuelve un producto específico por su ID
CREATE OR REPLACE FUNCTION fn_get_product(p_id INT)
RETURNS TABLE (
    product_id    INT,
    code          VARCHAR(10),
    name          VARCHAR(80),
    description   VARCHAR(200),
    price         NUMERIC(10,2),
    stock         INT,
    min_stock     INT,
    category_id   INT,
    category_name VARCHAR(50),
    image_url     VARCHAR(300),
    status        SMALLINT
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.product_id, p.code, p.name, p.description,
        p.price, p.stock, p.min_stock,
        p.category_id, c.name AS category_name,
        p.image_url, p.status
    FROM product p
    INNER JOIN category c ON p.category_id = c.category_id
    WHERE p.product_id = p_id;
END;
$$ LANGUAGE plpgsql;

-- fn_insert_product: Inserta un producto y autogenera el código P001, P002...
CREATE OR REPLACE FUNCTION fn_insert_product(
    p_name        VARCHAR(80),
    p_description VARCHAR(200),
    p_price       NUMERIC(10,2),
    p_stock       INT,
    p_min_stock   INT,
    p_category_id INT,
    p_image_url   VARCHAR(300)
)
RETURNS VOID AS $$
DECLARE
    v_next_id INT;
    v_code    VARCHAR(10);
BEGIN
    SELECT COALESCE(MAX(product_id), 0) + 1 INTO v_next_id FROM product;
    v_code := 'P' || LPAD(v_next_id::TEXT, 3, '0');

    INSERT INTO product (code, name, description, price, stock, min_stock, category_id, image_url, status)
    VALUES (v_code, p_name, p_description, p_price, p_stock, p_min_stock, p_category_id, p_image_url, 1);
END;
$$ LANGUAGE plpgsql;

-- fn_update_product: Actualiza los datos de un producto existente
CREATE OR REPLACE FUNCTION fn_update_product(
    p_id          INT,
    p_name        VARCHAR(80),
    p_description VARCHAR(200),
    p_price       NUMERIC(10,2),
    p_stock       INT,
    p_min_stock   INT,
    p_category_id INT,
    p_image_url   VARCHAR(300)
)
RETURNS VOID AS $$
BEGIN
    UPDATE product
    SET name        = p_name,
        description = p_description,
        price       = p_price,
        stock       = p_stock,
        min_stock   = p_min_stock,
        category_id = p_category_id,
        image_url   = p_image_url
    WHERE product_id = p_id;
END;
$$ LANGUAGE plpgsql;

-- fn_delete_product: Eliminación lógica (status = 0)
CREATE OR REPLACE FUNCTION fn_delete_product(p_id INT)
RETURNS VOID AS $$
BEGIN
    UPDATE product SET status = 0 WHERE product_id = p_id;
END;
$$ LANGUAGE plpgsql;

-- fn_register_order: Registra pedido + detalle en transacción y descuenta stock
CREATE OR REPLACE FUNCTION fn_register_order(
    p_customer_id INT,
    p_details     JSONB   -- [{product_id, quantity, unit_price}]
)
RETURNS INT AS $$
DECLARE
    v_order_id       INT;
    v_next_id        INT;
    v_code           VARCHAR(12);
    v_subtotal       NUMERIC(10,2) := 0;
    v_tax            NUMERIC(10,2);
    v_total          NUMERIC(10,2);
    v_detail         JSONB;
    v_qty            INT;
    v_price          NUMERIC(10,2);
    v_prod_id        INT;
    v_line_subtotal  NUMERIC(10,2);
BEGIN
    -- Generar código del pedido
    SELECT COALESCE(MAX(order_id), 0) + 1 INTO v_next_id FROM "order";
    v_code := 'ORD' || LPAD(v_next_id::TEXT, 3, '0');

    -- Calcular subtotal recorriendo el JSON
    FOR v_detail IN SELECT * FROM jsonb_array_elements(p_details)
    LOOP
        v_qty   := (v_detail->>'quantity')::INT;
        v_price := (v_detail->>'unit_price')::NUMERIC(10,2);
        v_subtotal := v_subtotal + (v_qty * v_price);
    END LOOP;

    v_tax   := ROUND(v_subtotal * 0.18, 2);
    v_total := v_subtotal + v_tax;

    -- Insertar la orden
    INSERT INTO "order" (code, customer_id, subtotal, tax, total, status)
    VALUES (v_code, p_customer_id, v_subtotal, v_tax, v_total, 1)
    RETURNING order_id INTO v_order_id;

    -- Insertar detalles y descontar stock
    FOR v_detail IN SELECT * FROM jsonb_array_elements(p_details)
    LOOP
        v_prod_id       := (v_detail->>'product_id')::INT;
        v_qty           := (v_detail->>'quantity')::INT;
        v_price         := (v_detail->>'unit_price')::NUMERIC(10,2);
        v_line_subtotal := v_qty * v_price;

        INSERT INTO order_detail (order_id, product_id, quantity, unit_price, subtotal)
        VALUES (v_order_id, v_prod_id, v_qty, v_price, v_line_subtotal);

        UPDATE product SET stock = stock - v_qty WHERE product_id = v_prod_id;
    END LOOP;

    RETURN v_order_id;
END;
$$ LANGUAGE plpgsql;

-- ---------------------------------------------------------------
-- DATOS DE PRUEBA
-- ---------------------------------------------------------------

INSERT INTO category (name, description, status) VALUES
    ('Food',        'Pet food and nutritional products',    1),
    ('Toys',        'Toys and entertainment for pets',      1),
    ('Accessories', 'Collars, leashes, beds and more',      1),
    ('Hygiene',     'Grooming and hygiene products',        1)
ON CONFLICT DO NOTHING;

INSERT INTO product (code, name, description, price, stock, min_stock, category_id, image_url, status) VALUES
    ('P001', 'Premium Dog Food 5kg',  'High-quality dry food for adult dogs',       45.90, 50, 10, 1, '', 1),
    ('P002', 'Cat Kibble Bag 2kg',    'Balanced nutrition kibble for cats',          28.50, 30,  5, 1, '', 1),
    ('P003', 'Rope Toy',              'Durable cotton rope toy for dogs',            12.99, 40,  8, 2, '', 1),
    ('P004', 'Feather Wand',          'Interactive feather wand for cats',            8.50, 25,  5, 2, '', 1),
    ('P005', 'Adjustable Collar',     'Nylon adjustable collar, size M',             15.00, 35,  7, 3, '', 1),
    ('P006', 'Retractable Leash 5m',  '5-meter retractable leash for dogs',          22.90, 20,  5, 3, '', 1),
    ('P007', 'Pet Shampoo 500ml',     'Gentle formula shampoo for dogs and cats',    18.50, 45, 10, 4, '', 1),
    ('P008', 'Nail Clipper',          'Stainless steel nail clipper for pets',        9.99, 30,  6, 4, '', 1)
ON CONFLICT DO NOTHING;

-- Verificación: debe retornar 8 productos
SELECT * FROM fn_list_products();
