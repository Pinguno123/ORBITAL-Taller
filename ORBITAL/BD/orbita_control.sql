/*
    ORBITA Control
    Base de datos unificada para SQL Server

    Compatible con el modelo actual del proyecto ORBITAL / Entity Framework Database-First.

    IMPORTANTE:
    - Este script reconstruye las tablas de ORBITA si ya existen para dejar un esquema limpio.
    - No elimina la base de datos, pero sí elimina y vuelve a crear las tablas/vistas del proyecto.
    - Los valores de los enums coinciden con C# y comienzan en 0.

    RolUsuario:       0 Administrador, 1 Coordinador, 2 Auditor
    EstadoUsuario:    0 Activo, 1 Inactivo
    EstadoRecurso:    0 Disponible, 1 Asignado, 2 Mantenimiento
    PrioridadMision:  0 Baja, 1 Media, 2 Alta
    EstadoMision:     0 Planificada, 1 EnEjecucion, 2 Finalizada, 3 Cancelada
*/

--BASE DE DATOS
IF DB_ID('orbita_control') IS NULL
BEGIN
    EXEC('CREATE DATABASE orbita_control');
END;
GO

USE orbita_control;
GO

/* =====================================================
   LIMPIEZA DEL ESQUEMA PARA UNA INSTALACION REPRODUCIBLE
   ===================================================== */

IF OBJECT_ID('dbo.vw_asignaciones_activas', 'V') IS NOT NULL
    DROP VIEW dbo.vw_asignaciones_activas;
GO

IF OBJECT_ID('dbo.vw_recurso_detalle', 'V') IS NOT NULL
    DROP VIEW dbo.vw_recurso_detalle;
GO

IF OBJECT_ID('dbo.asignacion_recurso', 'U') IS NOT NULL
    DROP TABLE dbo.asignacion_recurso;
GO

IF OBJECT_ID('dbo.dron', 'U') IS NOT NULL
    DROP TABLE dbo.dron;
GO

IF OBJECT_ID('dbo.rover_terrestre', 'U') IS NOT NULL
    DROP TABLE dbo.rover_terrestre;
GO

IF OBJECT_ID('dbo.estacion_sensores', 'U') IS NOT NULL
    DROP TABLE dbo.estacion_sensores;
GO

IF OBJECT_ID('dbo.mision', 'U') IS NOT NULL
    DROP TABLE dbo.mision;
GO

IF OBJECT_ID('dbo.recurso_exploracion', 'U') IS NOT NULL
    DROP TABLE dbo.recurso_exploracion;
GO

IF OBJECT_ID('dbo.usuario', 'U') IS NOT NULL
    DROP TABLE dbo.usuario;
GO

/* =====================================================
   TABLAS
   ===================================================== */

--TABLA: usuario
CREATE TABLE dbo.usuario
(
    id INT IDENTITY(1,1) NOT NULL,
    nombre_usuario NVARCHAR(50) NOT NULL,
    contrasena NVARCHAR(255) NOT NULL,
    rol TINYINT NOT NULL,
    estado TINYINT NOT NULL
        CONSTRAINT df_usuario_estado DEFAULT (0),

    CONSTRAINT pk_usuario PRIMARY KEY (id),
    CONSTRAINT uq_usuario_nombre_usuario UNIQUE (nombre_usuario),

    -- RolUsuario: 0 Administrador, 1 Coordinador, 2 Auditor
    CONSTRAINT ck_usuario_rol
        CHECK (rol IN (0, 1, 2)),

    -- EstadoUsuario: 0 Activo, 1 Inactivo
    CONSTRAINT ck_usuario_estado
        CHECK (estado IN (0, 1)),

    CONSTRAINT ck_usuario_nombre_no_vacio
        CHECK (LEN(LTRIM(RTRIM(nombre_usuario))) > 0),

    CONSTRAINT ck_usuario_contrasena_no_vacia
        CHECK (LEN(LTRIM(RTRIM(contrasena))) > 0)
);
GO

--TABLA: recurso_exploracion
CREATE TABLE dbo.recurso_exploracion
(
    id INT IDENTITY(1,1) NOT NULL,
    codigo VARCHAR(20) NOT NULL,
    modelo NVARCHAR(100) NOT NULL,
    tipo VARCHAR(25) NOT NULL,
    estado TINYINT NOT NULL
        CONSTRAINT df_recurso_exploracion_estado DEFAULT (0),

    CONSTRAINT pk_recurso_exploracion PRIMARY KEY (id),
    CONSTRAINT uq_recurso_exploracion_codigo UNIQUE (codigo),

    -- Discriminador utilizado por RecursoRepositorio para reconstruir la subclase correcta
    CONSTRAINT ck_recurso_exploracion_tipo
        CHECK (tipo IN ('Dron', 'RoverTerrestre', 'EstacionSensores')),

    -- EstadoRecurso: 0 Disponible, 1 Asignado, 2 Mantenimiento
    CONSTRAINT ck_recurso_exploracion_estado
        CHECK (estado IN (0, 1, 2)),

    CONSTRAINT ck_recurso_exploracion_codigo_no_vacio
        CHECK (LEN(LTRIM(RTRIM(codigo))) > 0),

    CONSTRAINT ck_recurso_exploracion_modelo_no_vacio
        CHECK (LEN(LTRIM(RTRIM(modelo))) > 0)
);
GO

--TABLA: dron
CREATE TABLE dbo.dron
(
    recurso_id INT NOT NULL,
    autonomia_vuelo DECIMAL(10,2) NOT NULL,
    alcance DECIMAL(10,2) NOT NULL,
    costo_por_hora DECIMAL(12,2) NOT NULL,

    CONSTRAINT pk_dron PRIMARY KEY (recurso_id),

    CONSTRAINT fk_dron_recurso_exploracion
        FOREIGN KEY (recurso_id)
        REFERENCES dbo.recurso_exploracion(id)
        ON DELETE CASCADE,

    CONSTRAINT ck_dron_autonomia_vuelo
        CHECK (autonomia_vuelo >= 0),

    CONSTRAINT ck_dron_alcance
        CHECK (alcance >= 0),

    CONSTRAINT ck_dron_costo_por_hora
        CHECK (costo_por_hora >= 0)
);
GO

--TABLA: rover_terrestre
CREATE TABLE dbo.rover_terrestre
(
    recurso_id INT NOT NULL,
    autonomia DECIMAL(10,2) NOT NULL,
    capacidad_carga DECIMAL(10,2) NOT NULL,
    costo_por_kilometro DECIMAL(12,2) NOT NULL,

    CONSTRAINT pk_rover_terrestre PRIMARY KEY (recurso_id),

    CONSTRAINT fk_rover_terrestre_recurso_exploracion
        FOREIGN KEY (recurso_id)
        REFERENCES dbo.recurso_exploracion(id)
        ON DELETE CASCADE,

    CONSTRAINT ck_rover_terrestre_autonomia
        CHECK (autonomia >= 0),

    CONSTRAINT ck_rover_terrestre_capacidad_carga
        CHECK (capacidad_carga >= 0),

    CONSTRAINT ck_rover_terrestre_costo_por_kilometro
        CHECK (costo_por_kilometro >= 0)
);
GO

--TABLA: estacion_sensores
CREATE TABLE dbo.estacion_sensores
(
    recurso_id INT NOT NULL,
    cantidad_sensores INT NOT NULL,
    consumo_energetico DECIMAL(10,2) NOT NULL,
    costo_diario DECIMAL(12,2) NOT NULL,

    CONSTRAINT pk_estacion_sensores PRIMARY KEY (recurso_id),

    CONSTRAINT fk_estacion_sensores_recurso_exploracion
        FOREIGN KEY (recurso_id)
        REFERENCES dbo.recurso_exploracion(id)
        ON DELETE CASCADE,

    CONSTRAINT ck_estacion_sensores_cantidad
        CHECK (cantidad_sensores > 0),

    CONSTRAINT ck_estacion_sensores_consumo
        CHECK (consumo_energetico >= 0),

    CONSTRAINT ck_estacion_sensores_costo_diario
        CHECK (costo_diario >= 0)
);
GO

--TABLA: mision
CREATE TABLE dbo.mision
(
    id INT IDENTITY(1,1) NOT NULL,
    codigo VARCHAR(20) NOT NULL,
    nombre NVARCHAR(100) NOT NULL,
    descripcion NVARCHAR(500) NOT NULL,
    fecha_inicio DATETIME2(0) NOT NULL,
    fecha_fin_estimada DATETIME2(0) NOT NULL,
    prioridad TINYINT NOT NULL,
    estado TINYINT NOT NULL
        CONSTRAINT df_mision_estado DEFAULT (0),
    responsable_id INT NOT NULL,

    CONSTRAINT pk_mision PRIMARY KEY (id),
    CONSTRAINT uq_mision_codigo UNIQUE (codigo),

    CONSTRAINT fk_mision_responsable
        FOREIGN KEY (responsable_id)
        REFERENCES dbo.usuario(id),

    -- PrioridadMision: 0 Baja, 1 Media, 2 Alta
    CONSTRAINT ck_mision_prioridad
        CHECK (prioridad IN (0, 1, 2)),

    -- EstadoMision: 0 Planificada, 1 EnEjecucion, 2 Finalizada, 3 Cancelada
    CONSTRAINT ck_mision_estado
        CHECK (estado IN (0, 1, 2, 3)),

    CONSTRAINT ck_mision_codigo_no_vacio
        CHECK (LEN(LTRIM(RTRIM(codigo))) > 0),

    CONSTRAINT ck_mision_nombre_no_vacio
        CHECK (LEN(LTRIM(RTRIM(nombre))) > 0),

    CONSTRAINT ck_mision_descripcion_no_vacia
        CHECK (LEN(LTRIM(RTRIM(descripcion))) > 0),

    -- El taller permite que la fecha final sea igual a la inicial; solo prohíbe que sea anterior
    CONSTRAINT ck_mision_fechas
        CHECK (fecha_fin_estimada >= fecha_inicio)
);
GO

--TABLA: asignacion_recurso
CREATE TABLE dbo.asignacion_recurso
(
    id INT IDENTITY(1,1) NOT NULL,
    mision_id INT NOT NULL,
    recurso_id INT NOT NULL,
    fecha_asignacion DATETIME2(0) NOT NULL
        CONSTRAINT df_asignacion_recurso_fecha_asignacion DEFAULT (SYSDATETIME()),
    fecha_liberacion DATETIME2(0) NULL,
    activa BIT NOT NULL
        CONSTRAINT df_asignacion_recurso_activa DEFAULT (1),

    -- Dron = horas, Rover = kilometros, Estacion = dias
    cantidad_operacion DECIMAL(10,2) NOT NULL,

    CONSTRAINT pk_asignacion_recurso PRIMARY KEY (id),

    CONSTRAINT fk_asignacion_recurso_mision
        FOREIGN KEY (mision_id)
        REFERENCES dbo.mision(id),

    CONSTRAINT fk_asignacion_recurso_recurso
        FOREIGN KEY (recurso_id)
        REFERENCES dbo.recurso_exploracion(id),

    CONSTRAINT ck_asignacion_recurso_cantidad_operacion
        CHECK (cantidad_operacion > 0),

    CONSTRAINT ck_asignacion_recurso_fechas
        CHECK
        (
            fecha_liberacion IS NULL
            OR fecha_liberacion >= fecha_asignacion
        ),

    -- Una asignacion activa no tiene fecha de liberacion; una liberada si debe tenerla
    CONSTRAINT ck_asignacion_recurso_estado
        CHECK
        (
            (activa = 1 AND fecha_liberacion IS NULL)
            OR
            (activa = 0 AND fecha_liberacion IS NOT NULL)
        )
);
GO

/* =====================================================
   INDICES
   ===================================================== */

-- Un recurso solo puede tener una asignacion activa al mismo tiempo
CREATE UNIQUE INDEX uq_asignacion_recurso_recurso_activo
ON dbo.asignacion_recurso(recurso_id)
WHERE activa = 1;
GO

CREATE INDEX ix_mision_estado
ON dbo.mision(estado);
GO

CREATE INDEX ix_mision_responsable
ON dbo.mision(responsable_id);
GO

CREATE INDEX ix_recurso_exploracion_estado
ON dbo.recurso_exploracion(estado);
GO

CREATE INDEX ix_recurso_exploracion_tipo
ON dbo.recurso_exploracion(tipo);
GO

CREATE INDEX ix_asignacion_recurso_mision
ON dbo.asignacion_recurso(mision_id);
GO

/* =====================================================
   VISTAS UTILIZADAS POR ENTITY FRAMEWORK
   ===================================================== */

--VISTA: recursos con atributos de la subclase
CREATE VIEW dbo.vw_recurso_detalle
AS
SELECT
    r.id,
    r.codigo,
    r.modelo,
    r.tipo,
    r.estado,
    d.autonomia_vuelo,
    d.alcance,
    d.costo_por_hora,
    rt.autonomia AS autonomia_rover,
    rt.capacidad_carga,
    rt.costo_por_kilometro,
    es.cantidad_sensores,
    es.consumo_energetico,
    es.costo_diario
FROM dbo.recurso_exploracion r
LEFT JOIN dbo.dron d
    ON d.recurso_id = r.id
LEFT JOIN dbo.rover_terrestre rt
    ON rt.recurso_id = r.id
LEFT JOIN dbo.estacion_sensores es
    ON es.recurso_id = r.id;
GO

--VISTA: asignaciones actualmente activas por mision
CREATE VIEW dbo.vw_asignaciones_activas
AS
SELECT
    ar.id AS asignacion_id,
    ar.mision_id,
    m.codigo AS codigo_mision,
    m.nombre AS nombre_mision,
    ar.recurso_id,
    r.codigo AS codigo_recurso,
    r.modelo AS modelo_recurso,
    r.tipo AS tipo_recurso,
    ar.fecha_asignacion
FROM dbo.asignacion_recurso ar
INNER JOIN dbo.mision m
    ON m.id = ar.mision_id
INNER JOIN dbo.recurso_exploracion r
    ON r.id = ar.recurso_id
WHERE ar.activa = 1;
GO

/* =====================================================
   DATOS DE PRUEBA OBLIGATORIOS DEL TALLER
   ===================================================== */

-- 3 usuarios con roles diferentes; todos activos para las credenciales de prueba
INSERT INTO dbo.usuario (nombre_usuario, contrasena, rol, estado)
VALUES
(N'admin_orbita', N'Admin123', 0, 0),
(N'coordinador_orbita', N'Coord123', 1, 0),
(N'auditor_orbita', N'Audit123', 2, 0);
GO

-- 3 drones
INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
VALUES
('DRN-001', N'Falcon X1', 'Dron', 1),
('DRN-002', N'Falcon X2', 'Dron', 0),
('DRN-003', N'SkyEye Pro', 'Dron', 0);
GO

INSERT INTO dbo.dron (recurso_id, autonomia_vuelo, alcance, costo_por_hora)
SELECT id, 4.50, 20.00, 35.00 FROM dbo.recurso_exploracion WHERE codigo = 'DRN-001'
UNION ALL
SELECT id, 6.00, 30.00, 45.00 FROM dbo.recurso_exploracion WHERE codigo = 'DRN-002'
UNION ALL
SELECT id, 3.75, 18.00, 28.50 FROM dbo.recurso_exploracion WHERE codigo = 'DRN-003';
GO

-- 3 rovers
INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
VALUES
('ROV-001', N'Terra One', 'RoverTerrestre', 1),
('ROV-002', N'Atlas R2', 'RoverTerrestre', 0),
('ROV-003', N'Pathfinder Mini', 'RoverTerrestre', 0);
GO

INSERT INTO dbo.rover_terrestre (recurso_id, autonomia, capacidad_carga, costo_por_kilometro)
SELECT id, 10.00, 120.00, 12.50 FROM dbo.recurso_exploracion WHERE codigo = 'ROV-001'
UNION ALL
SELECT id, 14.00, 180.00, 16.00 FROM dbo.recurso_exploracion WHERE codigo = 'ROV-002'
UNION ALL
SELECT id, 8.00, 75.00, 9.25 FROM dbo.recurso_exploracion WHERE codigo = 'ROV-003';
GO

-- 3 estaciones de sensores; EST-003 permanece en mantenimiento como exige la prueba
INSERT INTO dbo.recurso_exploracion (codigo, modelo, tipo, estado)
VALUES
('EST-001', N'SensorHub A1', 'EstacionSensores', 0),
('EST-002', N'GeoSense 360', 'EstacionSensores', 0),
('EST-003', N'ClimateNode X', 'EstacionSensores', 2);
GO

INSERT INTO dbo.estacion_sensores (recurso_id, cantidad_sensores, consumo_energetico, costo_diario)
SELECT id, 8, 2.40, 40.00 FROM dbo.recurso_exploracion WHERE codigo = 'EST-001'
UNION ALL
SELECT id, 12, 3.75, 55.00 FROM dbo.recurso_exploracion WHERE codigo = 'EST-002'
UNION ALL
SELECT id, 10, 3.10, 48.00 FROM dbo.recurso_exploracion WHERE codigo = 'EST-003';
GO

-- 4 misiones: en ejecucion, planificada, finalizada y cancelada
INSERT INTO dbo.mision
(
    codigo,
    nombre,
    descripcion,
    fecha_inicio,
    fecha_fin_estimada,
    prioridad,
    estado,
    responsable_id
)
VALUES
(
    'MIS-001',
    N'Exploracion del Valle Norte',
    N'Recoleccion de datos topograficos y ambientales.',
    '2026-09-27 08:00:00',
    '2026-09-30 17:00:00',
    2,
    1,
    (SELECT id FROM dbo.usuario WHERE nombre_usuario = N'coordinador_orbita')
),
(
    'MIS-002',
    N'Monitoreo de Suelo',
    N'Monitoreo de humedad y composicion del suelo.',
    '2026-10-10 07:00:00',
    '2026-10-12 16:00:00',
    1,
    0,
    (SELECT id FROM dbo.usuario WHERE nombre_usuario = N'coordinador_orbita')
),
(
    'MIS-003',
    N'Inspeccion Ambiental',
    N'Registro historico de una mision ya completada.',
    '2026-09-10 08:00:00',
    '2026-09-12 18:00:00',
    1,
    2,
    (SELECT id FROM dbo.usuario WHERE nombre_usuario = N'admin_orbita')
),
(
    'MIS-004',
    N'Prueba de Ruta Sur',
    N'Mision cancelada antes de su ejecucion.',
    '2026-10-20 09:00:00',
    '2026-10-21 15:00:00',
    0,
    3,
    (SELECT id FROM dbo.usuario WHERE nombre_usuario = N'coordinador_orbita')
);
GO

-- MIS-001 en ejecucion: DRN-001 utiliza 5 horas y ROV-001 recorre 10 kilometros
INSERT INTO dbo.asignacion_recurso
(
    mision_id,
    recurso_id,
    fecha_asignacion,
    fecha_liberacion,
    activa,
    cantidad_operacion
)
VALUES
(
    (SELECT id FROM dbo.mision WHERE codigo = 'MIS-001'),
    (SELECT id FROM dbo.recurso_exploracion WHERE codigo = 'DRN-001'),
    '2026-09-27 08:00:00',
    NULL,
    1,
    5.00
),
(
    (SELECT id FROM dbo.mision WHERE codigo = 'MIS-001'),
    (SELECT id FROM dbo.recurso_exploracion WHERE codigo = 'ROV-001'),
    '2026-09-27 08:00:00',
    NULL,
    1,
    10.00
);
GO

-- MIS-003 finalizada: historial de EST-001 por 3 dias; el recurso queda actualmente disponible
INSERT INTO dbo.asignacion_recurso
(
    mision_id,
    recurso_id,
    fecha_asignacion,
    fecha_liberacion,
    activa,
    cantidad_operacion
)
VALUES
(
    (SELECT id FROM dbo.mision WHERE codigo = 'MIS-003'),
    (SELECT id FROM dbo.recurso_exploracion WHERE codigo = 'EST-001'),
    '2026-09-10 08:00:00',
    '2026-09-12 18:00:00',
    0,
    3.00
);
GO

/* =====================================================
   PRUEBA NEGATIVA OPCIONAL
   =====================================================

   DRN-001 ya tiene una asignacion activa. Al descomentar
   este bloque, el indice uq_asignacion_recurso_recurso_activo
   debe rechazar una segunda asignacion activa del mismo recurso.

INSERT INTO dbo.asignacion_recurso
(
    mision_id,
    recurso_id,
    activa,
    cantidad_operacion
)
VALUES
(
    (SELECT id FROM dbo.mision WHERE codigo = 'MIS-002'),
    (SELECT id FROM dbo.recurso_exploracion WHERE codigo = 'DRN-001'),
    1,
    2.00
);
*/

/* =====================================================
   CONSULTAS DE VERIFICACION
   ===================================================== */

SELECT * FROM dbo.usuario ORDER BY id;
SELECT * FROM dbo.mision ORDER BY id;
SELECT * FROM dbo.vw_recurso_detalle ORDER BY codigo;
SELECT * FROM dbo.asignacion_recurso ORDER BY id;
SELECT * FROM dbo.vw_asignaciones_activas ORDER BY codigo_mision, codigo_recurso;
GO
