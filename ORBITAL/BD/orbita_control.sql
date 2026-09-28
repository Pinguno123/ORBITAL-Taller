-- ============================================================================
-- SISTEMA ORBITA CONTROL - SCRIPT DE BASE DE DATOS SQL SERVER
-- ============================================================================

IF DB_ID('orbita_control') IS NULL
BEGIN
    CREATE DATABASE orbita_control;
END
GO

USE orbita_control;
GO

-- 1. TABLA: usuario
IF OBJECT_ID('dbo.usuario', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.usuario (
        id INT IDENTITY(1,1) PRIMARY KEY,
        nombre_usuario NVARCHAR(50) NOT NULL UNIQUE,
        contrasena NVARCHAR(255) NOT NULL,
        rol TINYINT NOT NULL,     -- 0: Administrador, 1: Coordinador, 2: Auditor
        estado TINYINT NOT NULL   -- 1: Activo, 0: Inactivo
    );
END
GO

-- 2. TABLA: recurso_exploracion
IF OBJECT_ID('dbo.recurso_exploracion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.recurso_exploracion (
        id INT IDENTITY(1,1) PRIMARY KEY,
        codigo VARCHAR(20) NOT NULL UNIQUE,
        modelo NVARCHAR(100) NOT NULL,
        tipo VARCHAR(25) NOT NULL, -- 'Dron', 'Rover', 'Estacion'
        estado TINYINT NOT NULL    -- 0: Disponible, 1: Asignado, 2: Mantenimiento
    );
END
GO

-- 3. TABLA: dron (Especialización)
IF OBJECT_ID('dbo.dron', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.dron (
        recurso_id INT PRIMARY KEY,
        autonomia_vuelo DECIMAL(10,2) NOT NULL,
        alcance DECIMAL(10,2) NOT NULL,
        costo_por_hora DECIMAL(12,2) NOT NULL,
        CONSTRAINT fk_dron_recurso FOREIGN KEY (recurso_id) 
            REFERENCES dbo.recurso_exploracion(id) ON DELETE CASCADE
    );
END
GO

-- 4. TABLA: rover_terrestre (Especialización)
IF OBJECT_ID('dbo.rover_terrestre', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.rover_terrestre (
        recurso_id INT PRIMARY KEY,
        autonomia DECIMAL(10,2) NOT NULL,
        capacidad_carga DECIMAL(10,2) NOT NULL,
        costo_por_kilometro DECIMAL(12,2) NOT NULL,
        CONSTRAINT fk_rover_recurso FOREIGN KEY (recurso_id) 
            REFERENCES dbo.recurso_exploracion(id) ON DELETE CASCADE
    );
END
GO

-- 5. TABLA: estacion_sensores (Especialización)
IF OBJECT_ID('dbo.estacion_sensores', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.estacion_sensores (
        recurso_id INT PRIMARY KEY,
        cantidad_sensores INT NOT NULL,
        consumo_energetico DECIMAL(10,2) NOT NULL,
        costo_diario DECIMAL(12,2) NOT NULL,
        CONSTRAINT fk_estacion_recurso FOREIGN KEY (recurso_id) 
            REFERENCES dbo.recurso_exploracion(id) ON DELETE CASCADE
    );
END
GO

-- 6. TABLA: mision
IF OBJECT_ID('dbo.mision', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.mision (
        id INT IDENTITY(1,1) PRIMARY KEY,
        codigo VARCHAR(20) NOT NULL UNIQUE,
        nombre NVARCHAR(100) NOT NULL,
        descripcion NVARCHAR(500) NOT NULL,
        fecha_inicio DATETIME2(0) NOT NULL,
        fecha_fin_estimada DATETIME2(0) NOT NULL,
        prioridad TINYINT NOT NULL, -- 0: Baja, 1: Media, 2: Alta
        estado TINYINT NOT NULL,    -- 0: Planificada, 1: EnEjecucion, 2: Finalizada, 3: Cancelada
        responsable_id INT NOT NULL,
        CONSTRAINT fk_mision_usuario FOREIGN KEY (responsable_id) 
            REFERENCES dbo.usuario(id)
    );
END
GO

-- 7. TABLA: asignacion_recurso
IF OBJECT_ID('dbo.asignacion_recurso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.asignacion_recurso (
        id INT IDENTITY(1,1) PRIMARY KEY,
        mision_id INT NOT NULL,
        recurso_id INT NOT NULL,
        fecha_asignacion DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
        fecha_liberacion DATETIME2(0) NULL,
        activa BIT NOT NULL DEFAULT 1,
        cantidad_operacion DECIMAL(10,2) NOT NULL DEFAULT 1, -- Horas de dron, Km de rover o Días de estación
        CONSTRAINT fk_asignacion_recurso_mision FOREIGN KEY (mision_id) 
            REFERENCES dbo.mision(id),
        CONSTRAINT fk_asignacion_recurso_recurso FOREIGN KEY (recurso_id) 
            REFERENCES dbo.recurso_exploracion(id)
    );
END
ELSE
BEGIN
    -- MIGRACIÓN SEGURA PARA BASES DE DATOS EXISTENTES
    IF COL_LENGTH('dbo.asignacion_recurso', 'cantidad_operacion') IS NULL
    BEGIN
        ALTER TABLE dbo.asignacion_recurso 
        ADD cantidad_operacion DECIMAL(10,2) NOT NULL CONSTRAINT DF_asig_cantidad_operacion DEFAULT 1;
    END
END
GO

-- 8. VISTA: vw_asignaciones_activas
IF OBJECT_ID('dbo.vw_asignaciones_activas', 'V') IS NOT NULL
    DROP VIEW dbo.vw_asignaciones_activas;
GO
CREATE VIEW dbo.vw_asignaciones_activas AS
SELECT 
    a.id AS asignacion_id,
    m.id AS mision_id,
    m.codigo AS codigo_mision,
    m.nombre AS nombre_mision,
    r.id AS recurso_id,
    r.codigo AS codigo_recurso,
    r.modelo AS modelo_recurso,
    r.tipo AS tipo_recurso,
    a.fecha_asignacion
FROM dbo.asignacion_recurso a
INNER JOIN dbo.mision m ON a.mision_id = m.id
INNER JOIN dbo.recurso_exploracion r ON a.recurso_id = r.id
WHERE a.activa = 1;
GO

-- 9. VISTA: vw_recurso_detalle
IF OBJECT_ID('dbo.vw_recurso_detalle', 'V') IS NOT NULL
    DROP VIEW dbo.vw_recurso_detalle;
GO
CREATE VIEW dbo.vw_recurso_detalle AS
SELECT 
    r.id,
    r.codigo,
    r.modelo,
    r.tipo,
    r.estado,
    d.autonomia_vuelo,
    d.alcance,
    d.costo_por_hora,
    rov.autonomia AS autonomia_rover,
    rov.capacidad_carga,
    rov.costo_por_kilometro,
    e.cantidad_sensores,
    e.consumo_energetico,
    e.costo_diario
FROM dbo.recurso_exploracion r
LEFT JOIN dbo.dron d ON r.id = d.recurso_id
LEFT JOIN dbo.rover_terrestre rov ON r.id = rov.recurso_id
LEFT JOIN dbo.estacion_sensores e ON r.id = e.recurso_id;
GO

-- 10. DATOS INICIALES OBLIGATORIOS (SEED)
IF NOT EXISTS (SELECT 1 FROM dbo.usuario)
BEGIN
    INSERT INTO dbo.usuario (nombre_usuario, contrasena, rol, estado) VALUES
    ('admin_orbita', 'Admin123', 0, 1),
    ('coordinador_orbita', 'Coord123', 1, 1),
    ('auditor_orbita', 'Audit123', 2, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.recurso_exploracion)
BEGIN
    -- 1. Dron Falcon X1
    INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
    VALUES ('DRN-001', 'Falcon X1', 'Dron', 1);
    DECLARE @idDron INT = SCOPE_IDENTITY();
    INSERT INTO dbo.dron (recurso_id, autonomia_vuelo, alcance, costo_por_hora)
    VALUES (@idDron, 5.10, 21.00, 40.00);

    -- 2. Rover Terra One
    INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
    VALUES ('ROV-001', 'Terra One', 'Rover', 1);
    DECLARE @idRover INT = SCOPE_IDENTITY();
    INSERT INTO dbo.rover_terrestre (recurso_id, autonomia, capacidad_carga, costo_por_kilometro)
    VALUES (@idRover, 10.00, 120.00, 12.50);

    -- 3. Estación de Sensores Alpha (En Mantenimiento)
    INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
    VALUES ('EST-003', 'Sensor Hub Alpha', 'Estacion', 2);
    DECLARE @idEstacion INT = SCOPE_IDENTITY();
    INSERT INTO dbo.estacion_sensores (recurso_id, cantidad_sensores, consumo_energetico, costo_diario)
    VALUES (@idEstacion, 6, 1.80, 25.00);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.mision)
BEGIN
    DECLARE @idCoord INT = (SELECT TOP 1 id FROM dbo.usuario WHERE nombre_usuario = 'coordinador_orbita');
    DECLARE @idDron1 INT = (SELECT TOP 1 id FROM dbo.recurso_exploracion WHERE codigo = 'DRN-001');
    DECLARE @idRover1 INT = (SELECT TOP 1 id FROM dbo.recurso_exploracion WHERE codigo = 'ROV-001');

    INSERT INTO dbo.mision (codigo, nombre, descripcion, fecha_inicio, fecha_fin_estimada, prioridad, estado, responsable_id)
    VALUES ('MIS-001', 'Exploración del Valle Norte II', 'Recolección de datos topográficos y ambientales.', '2026-09-26', '2026-09-27', 1, 1, @idCoord);

    DECLARE @idMision INT = SCOPE_IDENTITY();

    -- Asignación con cantidad de operación: 5 horas para el dron y 10 km para el rover
    INSERT INTO dbo.asignacion_recurso (mision_id, recurso_id, fecha_asignacion, activa, cantidad_operacion)
    VALUES (@idMision, @idDron1, '2026-09-26', 1, 5.00);

    INSERT INTO dbo.asignacion_recurso (mision_id, recurso_id, fecha_asignacion, activa, cantidad_operacion)
    VALUES (@idMision, @idRover1, '2026-09-26', 1, 10.00);
END
GO
