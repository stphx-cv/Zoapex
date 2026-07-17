/* ============================================================
   VentasLeon_S13  -  Desarrollo de Aplicaciones Basico (Sesion 13)
   Ejecute este script UNA sola vez, DESPUES de VentasLeon.sql.
   Agrega:
     - Ubigeo: Tb_Departamento, Tb_Provincia, Tb_Distrito (con datos)
     - Tb_Proveedor (con datos; incluye inactivos para verlos en rojo)
     - Mas facturas de ejemplo (varios anios) para el grafico y la paginacion
     - Vista vw_VistaFacturas y SP usp_ListarFacturas_Paginacion (OFFSET/FETCH)
   Fechas: se usan partes enteras (DATEFROMPARTS) para que no dependan del idioma.
   ============================================================
PARA ELIMINAR 

USE master;
GO

ALTER DATABASE VentasLeon
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE VentasLeon;
GO

 */

USE VentasLeon;
GO

/* ---- Limpieza por si se vuelve a ejecutar ---- */
IF OBJECT_ID('usp_ListarFacturas_Paginacion') IS NOT NULL DROP PROCEDURE usp_ListarFacturas_Paginacion;
IF OBJECT_ID('vw_VistaFacturas') IS NOT NULL DROP VIEW vw_VistaFacturas;
IF OBJECT_ID('Tb_Proveedor') IS NOT NULL DROP TABLE Tb_Proveedor;
IF OBJECT_ID('Tb_Distrito') IS NOT NULL DROP TABLE Tb_Distrito;
IF OBJECT_ID('Tb_Provincia') IS NOT NULL DROP TABLE Tb_Provincia;
IF OBJECT_ID('Tb_Departamento') IS NOT NULL DROP TABLE Tb_Departamento;
GO

/* ======================= UBIGEO ======================= */
CREATE TABLE Tb_Departamento (
    Cod_dep   VARCHAR(2)   NOT NULL PRIMARY KEY,
    Nom_dep   VARCHAR(60)  NOT NULL
);

CREATE TABLE Tb_Provincia (
    Cod_prov  VARCHAR(4)   NOT NULL PRIMARY KEY,
    Cod_dep   VARCHAR(2)   NOT NULL REFERENCES Tb_Departamento(Cod_dep),
    Nom_prov  VARCHAR(60)  NOT NULL
);

CREATE TABLE Tb_Distrito (
    Cod_dist  VARCHAR(6)   NOT NULL PRIMARY KEY,
    Cod_prov  VARCHAR(4)   NOT NULL REFERENCES Tb_Provincia(Cod_prov),
    Nom_dist  VARCHAR(60)  NOT NULL
);
GO

INSERT INTO Tb_Departamento (Cod_dep, Nom_dep) VALUES
('15','Lima'),
('08','Cusco'),
('04','Arequipa');

INSERT INTO Tb_Provincia (Cod_prov, Cod_dep, Nom_prov) VALUES
('1501','15','Lima'),
('1507','15','Huaral'),
('0801','08','Cusco'),
('0808','08','Urubamba'),
('0401','04','Arequipa'),
('0405','04','Camana');

INSERT INTO Tb_Distrito (Cod_dist, Cod_prov, Nom_dist) VALUES
('150101','1501','Lima (Cercado)'),
('150122','1501','Miraflores'),
('150130','1501','San Isidro'),
('150701','1507','Huaral'),
('150702','1507','Aucallama'),
('080101','0801','Cusco'),
('080107','0801','Wanchaq'),
('080801','0808','Urubamba'),
('080805','0808','Ollantaytambo'),
('040101','0401','Arequipa'),
('040129','0401','Yanahuara'),
('040501','0405','Camana'),
('040502','0405','Jose Maria Quimper');
GO

/* ======================= PROVEEDOR ======================= */
CREATE TABLE Tb_Proveedor (
    Cod_prv      VARCHAR(5)    NOT NULL PRIMARY KEY,
    Raz_soc_prv  VARCHAR(120)  NOT NULL,
    Ruc_prv      VARCHAR(11)   NULL,
    Dir_prv      VARCHAR(150)  NULL,
    Cod_dep      VARCHAR(2)    NULL REFERENCES Tb_Departamento(Cod_dep),
    Cod_prov     VARCHAR(4)    NULL REFERENCES Tb_Provincia(Cod_prov),
    Cod_dist     VARCHAR(6)    NULL REFERENCES Tb_Distrito(Cod_dist),
    Est_prv      INT           NOT NULL DEFAULT 1   -- 1 activo, 0 inactivo
);
GO

INSERT INTO Tb_Proveedor (Cod_prv, Raz_soc_prv, Ruc_prv, Dir_prv, Cod_dep, Cod_prov, Cod_dist, Est_prv) VALUES
('P001','Importaciones Andinas SAC',   '20481234567','Av. Colonial 100',   '15','1501','150122', 1),
('P002','Suministros del Cusco EIRL',  '20512345678','Calle Sol 200',      '08','0801','080101', 1),
('P003','Comercial Arequipa SRL',      '20456123789','Av. Ejercito 300',   '04','0401','040129', 0),  -- inactivo
('P004','Distribuidora Lima Norte SAC','20487654321','Av. Tupac Amaru 400', '15','1507','150701', 1),
('P005','Textiles del Sur SA',         '20533221100','Jr. Comercio 500',   '08','0808','080805', 0),  -- inactivo
('P006','Ferreteria Central EIRL',     '20441122334','Av. Grau 600',       '04','0405','040501', 1),
('P007','Servicios Generales Peru SAC','20499887766','Av. Brasil 700',     '15','1501','150130', 1);
GO

/* ============ MAS FACTURAS (varios anios) para grafico y paginacion ============ */
/* Genera F006..F040 repartidas en 2023, 2024 y 2025, con su detalle.            */
DECLARE @i INT = 6;
WHILE @i <= 40
BEGIN
    DECLARE @num  VARCHAR(10) = 'F' + RIGHT('000' + CAST(@i AS VARCHAR(3)), 3);
    DECLARE @anio INT = 2023 + (@i % 3);            -- 2023, 2024, 2025
    DECLARE @mes  INT = ((@i * 7) % 12) + 1;
    DECLARE @dia  INT = ((@i * 3) % 27) + 1;
    DECLARE @fec  DATE = DATEFROMPARTS(@anio, @mes, @dia);
    DECLARE @est  INT = CASE WHEN @i % 3 = 0 THEN 2 ELSE 1 END;   -- cancelada / pendiente
    DECLARE @cli  VARCHAR(5) = 'C00' + CAST(((@i % 3) + 1) AS VARCHAR(1));
    DECLARE @ven  VARCHAR(5) = 'V00' + CAST(((@i % 2) + 1) AS VARCHAR(1));

    INSERT INTO Tb_Factura (Num_fac, Fec_fac, Fec_can, Est_fac, Cod_cli, Cod_ven, Por_igv)
    VALUES (@num, @fec, CASE WHEN @est = 2 THEN DATEADD(DAY, 10, @fec) ELSE NULL END, @est, @cli, @ven, 18.00);

    INSERT INTO Tb_Detalle_Factura (Num_fac, Cod_pro, Can_ven, Pre_ven) VALUES
        (@num, 'P010', (@i % 3) + 1, 1200.00),
        (@num, 'P020', (@i % 2) + 1,  350.00);

    SET @i = @i + 1;
END
GO

/* ======================= VISTA + SP DE PAGINACION ======================= */
/* Vista: una fila por factura, con cliente, vendedor, estado y total calculado. */
GO
CREATE VIEW vw_VistaFacturas AS
SELECT  f.Num_fac,
        f.Fec_fac,
        c.Raz_soc_cli                     AS Cliente,
        v.Nom_ven + ' ' + v.Ape_ven       AS Vendedor,
        CASE f.Est_fac WHEN 1 THEN 'Pendiente'
                       WHEN 2 THEN 'Cancelada'
                       ELSE 'Anulada' END  AS Estado,
        SUM(d.Can_ven * d.Pre_ven)         AS Total
FROM    Tb_Factura f
        INNER JOIN Tb_Cliente        c ON f.Cod_cli = c.Cod_cli
        INNER JOIN Tb_Vendedor       v ON f.Cod_ven = v.Cod_ven
        INNER JOIN Tb_Detalle_Factura d ON f.Num_fac = d.Num_fac
GROUP BY f.Num_fac, f.Fec_fac, c.Raz_soc_cli, v.Nom_ven, v.Ape_ven, f.Est_fac;
GO

/* SP: devuelve SOLO la pagina pedida usando OFFSET / FETCH.                      */
CREATE PROCEDURE usp_ListarFacturas_Paginacion
    @pagina INT,
    @tam    INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  Num_fac, Fec_fac, Cliente, Vendedor, Estado, Total
    FROM    vw_VistaFacturas
    ORDER BY Num_fac
    OFFSET (@pagina - 1) * @tam ROWS      -- salta las paginas previas
    FETCH NEXT @tam ROWS ONLY;            -- devuelve solo esta pagina
END
GO

PRINT 'VentasLeon_S13 aplicado correctamente (Ubigeo, Proveedor, facturas extra, vista y SP de paginacion).';
GO
