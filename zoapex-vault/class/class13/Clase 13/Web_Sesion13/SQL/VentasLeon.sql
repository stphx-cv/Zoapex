/* ============================================================
   Base de datos VentasLeon  -  Desarrollo de Aplicaciones Basico (Sesion 12)
   Ejecute este script una sola vez en SQL Server (SSMS o Azure Data Studio)
   ANTES de correr el proyecto. Crea la BD, las tablas, datos de ejemplo
   y los procedimientos almacenados que usa el Ejemplo 2.
   ============================================================ */

IF DB_ID('VentasLeon') IS NULL
    CREATE DATABASE VentasLeon;
GO
USE VentasLeon;
GO

/* ---- Limpieza por si se vuelve a ejecutar ---- */
IF OBJECT_ID('usp_ListarFacturasClienteFechas') IS NOT NULL DROP PROCEDURE usp_ListarFacturasClienteFechas;
IF OBJECT_ID('usp_DeudaCliente') IS NOT NULL DROP PROCEDURE usp_DeudaCliente;
IF OBJECT_ID('Tb_Detalle_Factura') IS NOT NULL DROP TABLE Tb_Detalle_Factura;
IF OBJECT_ID('Tb_Factura') IS NOT NULL DROP TABLE Tb_Factura;
IF OBJECT_ID('Tb_Cliente') IS NOT NULL DROP TABLE Tb_Cliente;
IF OBJECT_ID('Tb_Vendedor') IS NOT NULL DROP TABLE Tb_Vendedor;
GO

/* ============================ TABLAS ============================ */
CREATE TABLE Tb_Vendedor (
    Cod_ven   VARCHAR(5)   NOT NULL PRIMARY KEY,
    Nom_ven   VARCHAR(60)  NOT NULL,
    Ape_ven   VARCHAR(60)  NOT NULL
);

CREATE TABLE Tb_Cliente (
    Cod_cli     VARCHAR(5)    NOT NULL PRIMARY KEY,
    Raz_soc_cli VARCHAR(120)  NOT NULL,
    Ruc_cli     VARCHAR(11)   NULL,
    Dir_cli     VARCHAR(150)  NULL,
    Tel_cli     VARCHAR(20)   NULL,
    Est_cli     INT           NOT NULL DEFAULT 1   -- 1 activo, 0 inactivo
);

CREATE TABLE Tb_Factura (
    Num_fac   VARCHAR(10)  NOT NULL PRIMARY KEY,
    Fec_fac   DATETIME     NOT NULL,
    Fec_can   DATETIME     NULL,
    Est_fac   INT          NOT NULL,            -- 1 pendiente, 2 cancelada, 3 anulada
    Cod_cli   VARCHAR(5)   NOT NULL REFERENCES Tb_Cliente(Cod_cli),
    Cod_ven   VARCHAR(5)   NOT NULL REFERENCES Tb_Vendedor(Cod_ven),
    Por_igv   DECIMAL(5,2) NOT NULL DEFAULT 18.00
);

CREATE TABLE Tb_Detalle_Factura (
    Num_fac   VARCHAR(10)   NOT NULL REFERENCES Tb_Factura(Num_fac),
    Cod_pro   VARCHAR(10)   NOT NULL,
    Can_ven   INT           NOT NULL,
    Pre_ven   DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Detalle PRIMARY KEY (Num_fac, Cod_pro)
);
GO

/* ============================ DATOS ============================ */
INSERT INTO Tb_Vendedor (Cod_ven, Nom_ven, Ape_ven) VALUES
('V001','Pedro','Leon Soto'),
('V002','Ana','Quispe Rojas');

INSERT INTO Tb_Cliente (Cod_cli, Raz_soc_cli, Ruc_cli, Dir_cli, Tel_cli, Est_cli) VALUES
('C001','Comercial Andina SAC','20123456789','Av. Arequipa 1234, Lima','014567890', 1),
('C002','Tecnologia Global EIRL','20987654321','Jr. Union 250, Lima','017654321', 1),
('C003','Distribuidora del Sur SAC','20456789123','Av. El Sol 500, Cusco','084221100', 1);

INSERT INTO Tb_Factura (Num_fac, Fec_fac, Fec_can, Est_fac, Cod_cli, Cod_ven, Por_igv) VALUES
('F001','2025-01-15',NULL,        1,'C001','V001',18.00),  -- pendiente
('F002','2025-02-03','2025-02-20',2,'C001','V001',18.00),  -- cancelada
('F003','2025-02-18',NULL,        1,'C001','V002',18.00),  -- pendiente
('F004','2025-03-05',NULL,        1,'C002','V002',18.00),  -- pendiente
('F005','2025-03-22','2025-04-01',2,'C002','V001',18.00);  -- cancelada

INSERT INTO Tb_Detalle_Factura (Num_fac, Cod_pro, Can_ven, Pre_ven) VALUES
('F001','P010',2, 1200.00),
('F001','P020',1,  350.00),
('F002','P010',1, 1200.00),
('F003','P030',3,  150.00),
('F003','P020',2,  350.00),
('F004','P010',1, 1200.00),
('F005','P030',5,  150.00);
GO
/* ===================== EN CASO FALLE POR TEMA DE IDIOMA AL INSERTAR ===================== */
/* 
INSERT INTO Tb_Factura (Num_fac, Fec_fac, Fec_can, Est_fac, Cod_cli, Cod_ven, Por_igv) VALUES
('F001','20250115',NULL,      1,'C001','V001',18.00),
('F002','20250203','20250220',2,'C001','V001',18.00),
('F003','20250218',NULL,      1,'C001','V002',18.00),
('F004','20250305',NULL,      1,'C002','V002',18.00),
('F005','20250322','20250401',2,'C002','V001',18.00);

INSERT INTO Tb_Detalle_Factura (Num_fac, Cod_pro, Can_ven, Pre_ven) VALUES
('F001','P010',2, 1200.00),
('F001','P020',1,  350.00),
('F002','P010',1, 1200.00),
('F003','P030',3,  150.00),
('F003','P020',2,  350.00),
('F004','P010',1, 1200.00),
('F005','P030',5,  150.00);
*/
/* ===================== PROCEDIMIENTOS ALMACENADOS ===================== */

-- Lista las facturas de un cliente entre dos fechas, con su total calculado.
-- Lo consume el Ejemplo 2 con FromSql (mapeado a una entidad sin clave).
CREATE PROCEDURE usp_ListarFacturasClienteFechas
    @codigo  VARCHAR(5),
    @fecini  DATETIME,
    @fecfin  DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    SELECT  f.Num_fac,
            f.Fec_fac,
            CASE f.Est_fac WHEN 1 THEN 'Pendiente'
                           WHEN 2 THEN 'Cancelada'
                           ELSE 'Anulada' END AS Estado,
            SUM(d.Can_ven * d.Pre_ven) AS Total
    FROM    Tb_Factura f
            INNER JOIN Tb_Detalle_Factura d ON f.Num_fac = d.Num_fac
    WHERE   f.Cod_cli = @codigo
            AND f.Fec_fac BETWEEN @fecini AND @fecfin
    GROUP BY f.Num_fac, f.Fec_fac, f.Est_fac
    ORDER BY f.Fec_fac;
END
GO

-- Devuelve, por parametro de SALIDA, la deuda de un cliente (facturas pendientes).
-- Lo consume el Ejemplo 2 con ExecuteSqlRaw y un parametro de salida.
CREATE PROCEDURE usp_DeudaCliente
    @codigo  VARCHAR(5),
    @vdeuda  DECIMAL(12,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @vdeuda = ISNULL(SUM(d.Can_ven * d.Pre_ven), 0)
    FROM   Tb_Factura f
           INNER JOIN Tb_Detalle_Factura d ON f.Num_fac = d.Num_fac
    WHERE  f.Cod_cli = @codigo AND f.Est_fac = 1;
END
GO

PRINT 'Base de datos VentasLeon creada correctamente.';
