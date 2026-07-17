/* ============================================================================
   VentasLeon_S14.sql  -  Sesion 14: escenarios transaccionales (orden de compra)
   Se ejecuta UNA sola vez sobre la base VentasLeon (ya ampliada por la Sesion 13).
   Agrega: Tb_Producto, Tb_Orden_Compra, Tb_Detalle_Compra, un tipo de tabla (TVP)
   y el procedimiento transaccional usp_RegistrarOrdenCompra.
   La orden de compra reutiliza los proveedores creados en la Sesion 13.
   ============================================================================ */
USE VentasLeon;
GO

/* ------------------------------------------------------------------ PRODUCTO */
/* Cada producto es abastecido por un proveedor (Cod_prv). Asi el formulario
   puede mostrar solo los productos del proveedor elegido. */
CREATE TABLE Tb_Producto (
    Cod_pro   VARCHAR(4)    NOT NULL PRIMARY KEY,
    Des_pro   VARCHAR(80)   NOT NULL,
    Pre_pro   DECIMAL(10,2) NOT NULL,
    Stk_act   INT           NOT NULL DEFAULT 0,
    Cod_prv   VARCHAR(5)    NOT NULL REFERENCES Tb_Proveedor(Cod_prv),
    Est_pro   INT           NOT NULL DEFAULT 1   -- 1 activo, 0 inactivo
);
GO

/* Productos por proveedor (usa proveedores activos de la Sesion 13: P001,P002,P004,P006,P007). */
INSERT INTO Tb_Producto (Cod_pro, Des_pro, Pre_pro, Stk_act, Cod_prv) VALUES
 ('A001','Laptop 14 pulgadas Core i5',        2450.00, 30, 'P001'),
 ('A002','Monitor LED 24 pulgadas',            620.00, 45, 'P001'),
 ('A003','Teclado mecanico retroiluminado',    180.00, 80, 'P001'),
 ('A004','Mouse inalambrico',                    75.00,120, 'P002'),
 ('A005','Audifonos con microfono',            150.00, 60, 'P002'),
 ('A006','Webcam Full HD',                      210.00, 40, 'P002'),
 ('A007','Impresora multifuncional',           780.00, 25, 'P004'),
 ('A008','Toner generico negro',                95.00,200, 'P004'),
 ('A009','Papel bond A4 (millar)',              28.00,300, 'P004'),
 ('A010','Disco solido SSD 500GB',             320.00, 55, 'P006'),
 ('A011','Memoria RAM 8GB',                     190.00, 70, 'P006'),
 ('A012','Estabilizador 1000W',                135.00, 90, 'P007'),
 ('A013','Cable HDMI 2m',                        22.00,250, 'P007'),
 ('A014','Regleta 6 tomas',                      45.00,150, 'P007');
GO

/* ------------------------------------------------------------ ORDEN DE COMPRA */
/* Cabecera de la orden. Est_oco: 1 = registrada, 2 = atendida, 3 = anulada. */
CREATE TABLE Tb_Orden_Compra (
    Num_oco       VARCHAR(6)   NOT NULL PRIMARY KEY,
    Fec_oco       DATETIME     NOT NULL,
    Cod_prv       VARCHAR(5)   NOT NULL REFERENCES Tb_Proveedor(Cod_prv),
    Fec_ate       DATETIME     NULL,
    Est_oco       INT          NOT NULL DEFAULT 1,
    Usu_Registro  VARCHAR(30)  NULL,
    Fec_Registro  DATETIME     NOT NULL DEFAULT GETDATE()
);
GO

/* Detalle de la orden (una fila por producto pedido). Clave compuesta. */
CREATE TABLE Tb_Detalle_Compra (
    Num_oco   VARCHAR(6)  NOT NULL REFERENCES Tb_Orden_Compra(Num_oco),
    Cod_pro   VARCHAR(4)  NOT NULL REFERENCES Tb_Producto(Cod_pro),
    Can_ped   INT         NOT NULL,
    CONSTRAINT PK_Detalle_Compra PRIMARY KEY (Num_oco, Cod_pro)
);
GO

/* Una orden de ejemplo, para que el reporte tenga datos desde el inicio. */
INSERT INTO Tb_Orden_Compra (Num_oco, Fec_oco, Cod_prv, Est_oco, Usu_Registro) VALUES
 ('OC0001', GETDATE(), 'P001', 1, 'sistema');
INSERT INTO Tb_Detalle_Compra (Num_oco, Cod_pro, Can_ped) VALUES
 ('OC0001','A001', 5),
 ('OC0001','A002',10),
 ('OC0001','A003',15);
GO

/* -------------------------------------------------- TVP + SP TRANSACCIONAL */
/* Tipo de tabla: permite enviar TODO el detalle en un solo parametro (TVP). */
CREATE TYPE TVP_DetalleCompra AS TABLE (
    Cod_pro   VARCHAR(4)  NOT NULL,
    Can_ped   INT         NOT NULL
);
GO

/* SP transaccional: graba cabecera + detalle en una sola unidad (todo o nada).
   Genera el correlativo Num_oco (OC0001, OC0002, ...) dentro de la transaccion. */
CREATE PROCEDURE usp_RegistrarOrdenCompra
    @Cod_prv    VARCHAR(5),
    @Fec_ate    DATETIME,
    @Usuario    VARCHAR(30),
    @Detalles   TVP_DetalleCompra READONLY,
    @Num_oco    VARCHAR(6) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- validacion: no se acepta una orden sin detalle
    IF NOT EXISTS (SELECT 1 FROM @Detalles)
    BEGIN
        RAISERROR('La orden de compra no tiene detalle.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRAN;

            -- correlativo: OC + numero de 4 digitos
            DECLARE @sig INT =
                ISNULL((SELECT MAX(CAST(SUBSTRING(Num_oco, 3, 4) AS INT))
                        FROM Tb_Orden_Compra), 0) + 1;
            SET @Num_oco = 'OC' + RIGHT('0000' + CAST(@sig AS VARCHAR(4)), 4);

            -- cabecera
            INSERT INTO Tb_Orden_Compra (Num_oco, Fec_oco, Cod_prv, Fec_ate, Est_oco, Usu_Registro)
            VALUES (@Num_oco, GETDATE(), @Cod_prv, @Fec_ate, 1, @Usuario);

            -- detalle (todas las filas del TVP de una vez)
            INSERT INTO Tb_Detalle_Compra (Num_oco, Cod_pro, Can_ped)
            SELECT @Num_oco, Cod_pro, Can_ped FROM @Detalles;

        COMMIT;          -- si todo salio bien, se confirma
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK;   -- si algo fallo, no queda nada grabado
        THROW;                         -- se propaga el error a la aplicacion
    END CATCH
END
GO

PRINT 'VentasLeon_S14 aplicado correctamente (Producto, Orden_Compra, Detalle_Compra, TVP y SP transaccional).';
GO
