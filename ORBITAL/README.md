# OPERACIÓN ORBITA – Centro de Control de Misiones Científicas
**Asignatura:** Programación Orientada a Objetos – POO104  
**Evaluación:** Taller en Clase 2 (Acumulado 20%)  
**Entorno:** Visual Studio / .NET Framework 4.8 / SQL Server / Entity Framework 6  

---

## 1. Integrantes del Equipo
* [Nombre y Carnet del Estudiante 1]
* [Nombre y Carnet del Estudiante 2]
* [Nombre y Carnet del Estudiante 3]

---

## 2. Instrucciones de Ejecución

### Prerrequisitos
1. **SQL Server Local** o **SQL Server Express** en ejecución.
2. **Visual Studio 2019/2022** con soporte para desarrollo de escritorio .NET.

### Paso a Paso
1. **Creación y Carga de la Base de Datos:**
   - Abrir SQL Server Management Studio (SSMS).
   - Abrir y ejecutar el script ubicado en: `BD/orbita_control.sql`.
   - El script creará la base de datos `orbita_control`, todas las tablas con restricciones de integridad, vistas y los datos de prueba obligatorios.
2. **Configuración de la Cadena de Conexión:**
   - Verificar en `App.config` que la cadena `orbita_controlEntities` apunte a su instancia local de SQL Server (`data source=localhost` o `data source=.\SQLEXPRESS`).
3. **Compilación y Ejecución:**
   - Abrir la solución `ORBITAL.slnx` o el proyecto `ORBITAL.csproj` en Visual Studio.
   - Compilar la solución (Ctrl + Shift + B).
   - Iniciar la ejecución (F5 o Ctrl + F5).

---

## 3. Credenciales de Prueba

| Rol | Usuario | Contraseña | Permisos y Alcance |
| :--- | :--- | :--- | :--- |
| **Administrador** | `admin_orbita` | `Admin123` | Acceso total: gestión de usuarios, alta y edición de recursos, gestión completa de misiones y panel de control. |
| **Coordinador** | `coordinador_orbita` | `Coord123` | Gestión operativa: crear/modificar misiones, asignar/liberar recursos, iniciar y finalizar misiones, estimar costos. |
| **Auditor** | `auditor_orbita` | `Audit123` | Solo lectura: consultar misiones, recursos, historial y vista de asignaciones activas. |

---

## 4. Distribución General de Módulos y Capas

La solución sigue una arquitectura en capas desacoplada:

1. **`ORBITAL.Dominio` (Capa de Negocio y Reglas):**
   - **Entidades:** `Usuario`, `Mision`, `RecursoExploracion` (abstracta), `Dron`, `RoverTerrestre`, `EstacionSensores`, `AsignacionRecurso`.
   - **Interfaces:** `IAsignable` (implementada por `RecursoExploracion`).
   - **Enumeraciones:** `RolUsuario`, `EstadoUsuario`, `EstadoMision`, `PrioridadMision`, `EstadoRecurso`.
   - **Excepciones Propias:** `RecursoNoDisponibleException`, `MisionInvalidaException`.
   - **Reglas Principales:** Protocolo de Seguridad ORBITA (validación de 6 condiciones antes de iniciar misiones) y cálculo polimórfico de costos.

2. **`ORBITAL.Datos` (Capa de Persistencia y Acceso a Datos):**
   - **Contexto Entity Framework:** `orbita_controlEntities` generado mediante Database-First desde `BD/orbita_control.sql`.
   - **Repositorios:**
     - `UsuarioRepositorio`: Autenticación y consulta de usuarios.
     - `RecursoRepositorio`: CRUD polimórfico de recursos (Drones, Rovers, Estaciones) apoyado en la vista `vw_recurso_detalle`.
     - `MisionRepositorio`: Creación, actualización, cambio de estado y carga de relaciones.
     - `AsignacionRepositorio`: Gestión transaccional de asignación y liberación de recursos con vista `vw_asignaciones_activas`.

3. **`ORBITAL.App` (Capa de Aplicación y Sesión):**
   - `SesionUsuario`: Gestor de autenticación y autorización por roles en memoria con verificación de permisos en tiempo de ejecución.

4. **`ORBITAL` (Capa de Presentación - Consola):**
   - `Program.cs`: Interfaz interactiva de consola con menús contextuales según el rol autenticado, captura robusta de excepciones (`try-catch-finally`), panel de control y simulador de pruebas de fallo.

---

## 5. Principios SOLID Identificados y Explicados

### 1. Principio de Responsabilidad Única (Single Responsibility Principle - SRP)
* **En qué parte del código se identifica:**
  En la separación estricta entre las entidades de dominio (ej. `Mision.cs`), los repositorios (ej. `MisionRepositorio.cs`) y la sesión (`SesionUsuario.cs`).
  - `Mision.cs` es responsable únicamente de resguardar el estado y aplicar las reglas de negocio (como el Protocolo de Seguridad y el cálculo de costos). No sabe nada de SQL ni de Entity Framework.
  - `MisionRepositorio.cs` es responsable únicamente de interactuar con la base de datos y mapear los datos a objetos del dominio.
  - `Program.cs` es responsable únicamente de la presentación y lectura en consola.
* **Qué problema se pretende evitar:**
  Evita las clases "Dios" o monolíticas donde la lógica de interfaz, la lógica de negocio y las consultas SQL están entrelazadas, facilitando el mantenimiento y evitando que un cambio en la base de datos rompa la lógica del dominio.

### 2. Principio Abierto/Cerrado (Open/Closed Principle - OCP)
* **En qué parte del código se identifica:**
  En la clase abstracta `RecursoExploracion` y el método polimórfico `CalcularCostoOperacion(decimal cantidad)`.
  Las clases derivadas (`Dron`, `RoverTerrestre`, `EstacionSensores`) sobrescriben este comportamiento con su propia fórmula y unidad de operación:
  - Dron: $\text{horas de vuelo} \times \text{costo/hora}$
  - Rover: $\text{kilómetros de recorrido} \times \text{costo/km}$
  - Estación: $\text{días de monitoreo} \times \text{costo/día}$
  
  La entidad `AsignacionRecurso` conserva la `CantidadOperacion` persistida en base de datos (`asignacion_recurso.cantidad_operacion`), y el método `Mision.CalcularCostoEstimado()` itera sobre las asignaciones activas (o sobre el historial completo de asignaciones en misiones finalizadas/canceladas) delegando a `asig.CalcularCosto()` y a `recurso.CalcularCostoOperacion(...)` de forma polimórfica **sin usar `if` ni `switch` para comprobar el tipo concreto**.
* **Qué problema se pretende evitar:**
  Si en el futuro se incorpora un nuevo recurso (por ejemplo, `SateliteOrbital`), solo se crea la nueva clase que herede de `RecursoExploracion`. No es necesario modificar ni una sola línea de código existente en `Mision.cs` ni en la clase base ni en la lógica de cálculo.

### 3. Principio de Sustitución de Liskov (Liskov Substitution Principle - LSP)
* **En qué parte del código se identifica:**
  `Mision.Recursos` es una colección genérica `List<RecursoExploracion>`. Cualquier subclase (`Dron`, `RoverTerrestre`, `EstacionSensores`) puede asignarse y procesarse a través de la referencia base sin alterar el correcto funcionamiento del sistema.
* **Qué problema se pretende evitar:**
  Evita errores de incompatibilidad de tipos o comportamientos anómalos al manipular colecciones heterogéneas de recursos.

---

## 6. Patrón de Diseño Propuesto para Versión Futura

* **Nombre del Patrón:** *Factory Method* (Método de Fábrica).
* **Clasificación:** Creacional.
* **Problema del sistema que podría resolver:**
  Actualmente, la instanciación de recursos especializados según su tipo discriminador se resuelve dentro del repositorio o en la consola. Un patrón Factory Method desacoplaría totalmente la creación de recursos de exploración, centralizando la validación de parámetros específicos de cada subclase y facilitando la adición de nuevos tipos de tecnología espacial.
* **Clases que participarían:**
  - `RecursoExploracion` (Producto Abstracto).
  - `Dron`, `RoverTerrestre`, `EstacionSensores` (Productos Concretos).
  - `RecursoFactory` (Creador con método fábrica `CrearRecurso(string tipo, IDictionary<string, object> parametros)`).
* **Nota sobre Patrones ya Aplicados:**
  En la capa de aplicación se implementó el patrón **Singleton** en `SesionUsuario.Instancia` para garantizar un único punto central de control de la sesión del usuario en toda la ejecución.

---

## 7. Comprobación Manual de Excepciones y Reglas de Negocio

El sistema controla y gestiona rigurosamente las situaciones excepcionales mediante bloques `try-catch-finally`, permitiendo comprobar las reglas de forma manual directamente desde el menú:

1. **Intento de asignar un recurso no disponible (`RecursoNoDisponibleException`):**
   - Ir a la opción **"Asignar recurso a misión"**.
   - Seleccionar cualquier misión activa (ej. `MIS-002`).
   - Ingresar el código de un recurso en mantenimiento como `EST-003` o ya asignado como `DRN-001`.
   - **Resultado:** El dominio rechaza la operación y el sistema captura `RecursoNoDisponibleException`, mostrando un mensaje de advertencia claro en color rojo sin detener el programa.

2. **Intento de iniciar una misión violando el Protocolo de Seguridad (`MisionInvalidaException`):**
   - Crear una misión nueva sin recursos asignados (o seleccionar `MIS-004`).
   - Ir a la opción **"Iniciar misión"** e intentar arrancarla.
   - **Resultado:** La validación automática de las 6 condiciones del Protocolo de Seguridad ORBITA detecta la ausencia de recursos (o recurso en mantenimiento) y lanza `MisionInvalidaException`, informando con precisión el motivo del rechazo.

3. **Intento de crear una misión con fechas o datos inválidos (`MisionInvalidaException`):**
   - Ir a **"Crear nueva misión"** e ingresar una fecha de fin anterior a la fecha de inicio o dejar campos obligatorios en blanco.
   - **Resultado:** El sistema rechaza la solicitud capturando la excepción de dominio correspondiente.
