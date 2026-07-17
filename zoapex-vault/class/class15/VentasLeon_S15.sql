/* ============================================================================
   VentasLeon_S15.sql  -  Sesion 15: seguridad del sitio web (roles y usuarios)
   Se ejecuta UNA sola vez sobre la base VentasLeon (ya ampliada por la Sesion 14).
   Agrega el modelo de seguridad propio del sitio: Tb_Rol, Tb_Usuario y la tabla
   puente Tb_Usuario_Rol, mas una vista de apoyo para leer los roles de un usuario.

   NOTA IMPORTANTE (equivalencia con el material antiguo):
   En ASP.NET Web Forms la seguridad se creaba con el utilitario aspnet_regsql.exe,
   que generaba las tablas aspnet_Membership, aspnet_Roles, aspnet_Users, etc.
   Ese proveedor (System.Web.Security) NO existe en ASP.NET Core. Aqui usamos un
   esquema propio y sencillo (rol, usuario, usuario-rol) con la clave guardada como
   hash. La autenticacion se resuelve con cookies y la autorizacion con roles y
   politicas configuradas en Program.cs (no en web.config).
   ============================================================================ */
USE VentasLeon;
GO

/* ------------------------------------------------------------------- ROL */
/* Cada rol agrupa permisos. El caso de uso maneja 2: Administrador y Operador. */
CREATE TABLE Tb_Rol (
    Cod_rol   INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Nom_rol   VARCHAR(30)  NOT NULL UNIQUE
);
GO

/* --------------------------------------------------------------- USUARIO */
/* La clave NUNCA se guarda en texto plano: se almacena su hash (lo genera la
   aplicacion con PasswordHasher de ASP.NET Core). Est_usu: 1 activo, 0 inactivo. */
CREATE TABLE Tb_Usuario (
    Cod_usu       INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Login         VARCHAR(30)   NOT NULL UNIQUE,
    Clave         VARCHAR(200)  NOT NULL,
    Nombre        VARCHAR(80)   NOT NULL,
    Email         VARCHAR(120)  NULL,
    Est_usu       INT           NOT NULL DEFAULT 1,
    Fec_Registro  DATETIME      NOT NULL DEFAULT GETDATE()
);
GO

/* --------------------------------------------------------- USUARIO-ROL */
/* Relacion muchos a muchos: un usuario puede tener uno o varios roles. */
CREATE TABLE Tb_Usuario_Rol (
    Cod_usu   INT  NOT NULL REFERENCES Tb_Usuario(Cod_usu),
    Cod_rol   INT  NOT NULL REFERENCES Tb_Rol(Cod_rol),
    CONSTRAINT PK_Usuario_Rol PRIMARY KEY (Cod_usu, Cod_rol)
);
GO

/* Roles base del caso de uso. Los usuarios los crea la aplicacion (hash de clave):
   al iniciar el sitio por primera vez se siembran 'admin' y 'operador' si la tabla
   de usuarios esta vacia (ver SecuritySeeder en la capa de datos). Tambien se pueden
   crear mas roles y usuarios desde la pagina Seguridad (solo Administrador). */
INSERT INTO Tb_Rol (Nom_rol) VALUES ('Administrador'), ('Operador');
GO

/* Vista de apoyo: usuario con la lista de sus roles (se usa al autenticar). */
CREATE VIEW vw_Usuario_Roles AS
    SELECT u.Cod_usu, u.Login, u.Nombre, u.Email, u.Est_usu, r.Nom_rol
    FROM   Tb_Usuario u
    JOIN   Tb_Usuario_Rol ur ON ur.Cod_usu = u.Cod_usu
    JOIN   Tb_Rol r          ON r.Cod_rol  = ur.Cod_rol;
GO

PRINT 'VentasLeon_S15 aplicado correctamente (Tb_Rol, Tb_Usuario, Tb_Usuario_Rol y vista de roles).';
GO
