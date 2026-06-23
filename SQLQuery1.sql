/*
    Script_MDF_Limpio.sql
    Proyecto: Flota DLD Web

    IMPORTANTE:
    Este script deja el MDF solo con las tablas que usa el proyecto web:
    Usuarios, Conductores, Vehiculos y Viajes.

    Ojo: borra y vuelve a crear estas tablas, así que si tenían datos antiguos se pierden.
    Para una entrega de prueba está bien, porque queda limpio y ordenado.
*/

USE [FlotaLuchito_bd];
GO

/* Primero botamos las relaciones, porque si hay FK antiguas SQL Server no deja borrar tablas. */
DECLARE @dropFK NVARCHAR(MAX) = N'';

SELECT @dropFK = @dropFK +
    N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.' + QUOTENAME(OBJECT_NAME(parent_object_id)) +
    N' DROP CONSTRAINT ' + QUOTENAME(name) + N';' + CHAR(13)
FROM sys.foreign_keys;

IF LEN(@dropFK) > 0
BEGIN
    EXEC sp_executesql @dropFK;
END;
GO

/* Borramos todas las tablas que no corresponden o que vamos a rehacer limpias. */
DROP TABLE IF EXISTS dbo.CargasCombustible;
DROP TABLE IF EXISTS dbo.Inspecciones;
DROP TABLE IF EXISTS dbo.Mantenimientos;
DROP TABLE IF EXISTS dbo.Multas;
DROP TABLE IF EXISTS dbo.Talleres;
DROP TABLE IF EXISTS dbo.TiposMantenimiento;
DROP TABLE IF EXISTS dbo.TiposVehiculo;

DROP TABLE IF EXISTS dbo.Viajes;
DROP TABLE IF EXISTS dbo.Vehiculos;
DROP TABLE IF EXISTS dbo.Conductores;
DROP TABLE IF EXISTS dbo.Usuarios;
GO

/* Tabla para el login. */
CREATE TABLE dbo.Usuarios
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioLogin NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL
);
GO

/* Usuario pedido para entrar al sistema. */
INSERT INTO dbo.Usuarios (UsuarioLogin, Password)
VALUES ('luchito', '1234');
GO

/* Tabla del formulario Conductores.cshtml. */
CREATE TABLE dbo.Conductores
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(60) NOT NULL,
    Apellido NVARCHAR(60) NOT NULL,
    Rut NVARCHAR(15) NOT NULL UNIQUE,
    Telefono NVARCHAR(20) NOT NULL,
    Correo NVARCHAR(100) NOT NULL,
    NumeroLicencia NVARCHAR(30) NOT NULL,
    TipoLicencia NVARCHAR(10) NOT NULL,
    FechaVencimientoLicencia DATE NOT NULL,
    Estado NVARCHAR(30) NOT NULL
);
GO

/* Tabla del formulario Vehiculos.cshtml. */
CREATE TABLE dbo.Vehiculos
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TipoVehiculo NVARCHAR(50) NOT NULL,
    Conductor NVARCHAR(160) NOT NULL,
    Patente NVARCHAR(12) NOT NULL UNIQUE,
    Marca NVARCHAR(60) NOT NULL,
    Modelo NVARCHAR(60) NOT NULL,
    Anio INT NOT NULL,
    Color NVARCHAR(40) NOT NULL,
    Combustible NVARCHAR(30) NOT NULL,
    Kilometraje DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(40) NOT NULL,
    TieneMultas BIT NOT NULL DEFAULT(0),
    VaAMantencion BIT NOT NULL DEFAULT(0)
);
GO

/* Tabla del formulario Viajes.cshtml. */
CREATE TABLE dbo.Viajes
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Vehiculo NVARCHAR(160) NOT NULL,
    Conductor NVARCHAR(160) NOT NULL,
    Origen NVARCHAR(80) NOT NULL,
    Destino NVARCHAR(80) NOT NULL,
    FechaSalida DATE NOT NULL,
    FechaLlegada DATE NOT NULL,
    Distancia DECIMAL(18,2) NOT NULL,
    Estado NVARCHAR(30) NOT NULL,
    LatitudOrigen FLOAT NULL,
    LongitudOrigen FLOAT NULL,
    LatitudDestino FLOAT NULL,
    LongitudDestino FLOAT NULL,
    TiempoEstimado INT NULL
);
GO

/* Datos de ejemplo chicos para que el profe vea algo al tiro. Los puedes borrar si quieres. */
INSERT INTO dbo.Conductores
(Nombre, Apellido, Rut, Telefono, Correo, NumeroLicencia, TipoLicencia, FechaVencimientoLicencia, Estado)
VALUES
('Juan', 'Perez', '12.345.678-9', '987654321', 'juan@correo.cl', 'LIC123', 'B', '2027-05-10', 'Activo');
GO

INSERT INTO dbo.Vehiculos
(TipoVehiculo, Conductor, Patente, Marca, Modelo, Anio, Color, Combustible, Kilometraje, Estado, TieneMultas, VaAMantencion)
VALUES
('Camioneta', 'Juan Perez (12.345.678-9)', 'ABCD12', 'Toyota', 'Hilux', 2020, 'Blanco', 'Diesel', 85000, 'Disponible', 0, 0);
GO

INSERT INTO dbo.Viajes
(Vehiculo, Conductor, Origen, Destino, FechaSalida, FechaLlegada, Distancia, Estado)
VALUES
('ABCD12 - Toyota Hilux', 'Juan Perez (12.345.678-9)', 'Rancagua', 'Santiago', '2026-06-01', '2026-06-01', 90, 'Finalizado');
GO
