# OPERACIÓN ORBITA – Centro de Control de Misiones Científicas

**Asignatura:** Programación Orientada a Objetos – POO104  
**Evaluación:** Taller en Clase 2 – Acumulado 20 %  
**Entorno:** Visual Studio / .NET Framework 4.8 / SQL Server / Entity Framework 6  
**Tipo de aplicación:** Aplicación de consola en C#

---

## 1. Integrantes del equipo

- **Alexandra Elizabeth Alvarado Bautista** — AB260167
- **Douglas Emmanuel Sánchez Rivera** — SR260165
- **Karla Angie Arias Pérez** — AP260403

---

## 2. Descripción general

ORBITA Control es una aplicación de consola desarrollada en C# para administrar misiones científicas y los recursos tecnológicos utilizados durante su ejecución.

El sistema permite gestionar usuarios, misiones, drones, rovers terrestres y estaciones de sensores, así como asignar y liberar recursos, controlar estados de disponibilidad, validar las condiciones necesarias para iniciar una misión y calcular los costos estimados de operación.

La aplicación utiliza SQL Server para la persistencia de la información y Entity Framework 6 mediante un modelo Database First para interactuar con la base de datos.

---

## 3. Instrucciones de ejecución

### Prerrequisitos

Para ejecutar el proyecto se requiere:

1. Visual Studio 2019 o 2022 con soporte para .NET Framework.
2. .NET Framework 4.8.
3. SQL Server o SQL Server Express.
4. SQL Server Management Studio, recomendado para ejecutar el script de creación de la base de datos.
5. Restaurar los paquetes NuGet utilizados por el proyecto.

### Creación de la base de datos

1. Abrir SQL Server Management Studio.
2. Conectarse a la instancia local de SQL Server.
3. Abrir el archivo:

`BD/orbita_control.sql`

4. Ejecutar el script completo.

El script crea la base de datos:

`orbita_control`

y genera las tablas, relaciones, restricciones de integridad, vistas y datos de prueba requeridos para ejecutar la aplicación.

La base contempla persistencia para:

- usuarios;
- misiones;
- recursos de exploración;
- drones;
- rovers terrestres;
- estaciones de sensores;
- asignaciones de recursos.

La tabla `asignacion_recurso` conserva además la `cantidad_operacion`, utilizada para almacenar las horas, kilómetros o días estimados según el tipo de recurso.

### Configuración de la conexión

La cadena de conexión se encuentra en:

`App.config`

y utiliza el nombre:

`orbita_controlEntities`

Antes de ejecutar el proyecto debe verificarse que `data source` corresponda con la instancia de SQL Server disponible en la computadora.

Ejemplos:

Para una instancia predeterminada:

`data source=localhost`

Para SQL Server Express:

`data source=.\SQLEXPRESS`

No es necesario modificar el código C# para cambiar de computadora; únicamente debe ajustarse la cadena de conexión cuando el nombre de la instancia sea diferente.

### Compilación y ejecución

1. Abrir `ORBITAL.slnx` en Visual Studio.
2. Si la versión de Visual Studio no reconoce `.slnx`, abrir directamente `ORBITAL.csproj`.
3. Restaurar los paquetes NuGet si Visual Studio lo solicita.
4. Compilar mediante `Ctrl + Shift + B`.
5. Ejecutar mediante `F5` o `Ctrl + F5`.

---

## 4. Credenciales de prueba

| Rol | Usuario | Contraseña | Alcance |
|---|---|---|---|
| **Administrador** | `admin_orbita` | `Admin123` | Gestión de usuarios, recursos, misiones y Panel de Control ORBITA |
| **Coordinador** | `coordinador_orbita` | `Coord123` | Crear y modificar misiones, asignar y retirar recursos, iniciar y finalizar misiones y consultar información |
| **Auditor** | `auditor_orbita` | `Audit123` | Operaciones de consulta sobre misiones, recursos y Panel de Control ORBITA |

---

## 5. Organización general de la solución

La solución utiliza una separación lógica de responsabilidades mediante las áreas `Dominio`, `Datos` y `App`, además de la interfaz de consola.

### Dominio

Contiene las entidades y reglas principales del negocio.

**Entidades principales:**

- `Usuario`
- `Mision`
- `AsignacionRecurso`
- `RecursoExploracion`
- `Dron`
- `RoverTerrestre`
- `EstacionSensores`

`RecursoExploracion` es una clase abstracta de la que derivan los tres tipos concretos de recursos.

**Interfaces:**

- `IAsignable`

Define las operaciones relacionadas con asignación y disponibilidad:

- `Asignar(...)`
- `Liberar()`
- `EstaDisponible()`

**Enumeraciones:**

- `RolUsuario`
- `EstadoUsuario`
- `EstadoMision`
- `PrioridadMision`
- `EstadoRecurso`

**Excepciones personalizadas:**

- `RecursoNoDisponibleException`
- `MisionInvalidaException`

### Datos

Contiene la persistencia y el acceso a SQL Server.

**Entity Framework:**

El modelo `Model1.edmx` utiliza Entity Framework 6 Database First para representar las tablas y relaciones existentes en `orbita_control`.

**Repositorios principales:**

- `UsuarioRepositorio`
- `RecursoRepositorio`
- `MisionRepositorio`
- `AsignacionRepositorio`

Los repositorios centralizan las operaciones de persistencia para evitar que la interfaz o las entidades de dominio realicen consultas SQL directamente.

### App y sesión

`SesionUsuario` mantiene la información del usuario autenticado durante la ejecución y permite consultar sus permisos.

El sistema maneja los roles:

- Administrador
- Coordinador
- Auditor

Las operaciones sensibles vuelven a comprobar los permisos antes de realizar modificaciones, por lo que la autorización no depende únicamente de mostrar u ocultar opciones del menú.

### Presentación

`Program.cs` contiene la interacción principal mediante consola, incluyendo:

- inicio y cierre de sesión;
- menús según el rol;
- gestión de usuarios;
- gestión de recursos;
- gestión de misiones;
- asignación y retiro de recursos;
- Panel de Control ORBITA;
- captura y validación de entradas;
- manejo de excepciones mediante `try-catch`.

---

## 6. Jerarquía de recursos y polimorfismo

Todos los recursos derivan de:

`RecursoExploracion`

La clase base define el método abstracto:

`CalcularCostoOperacion(decimal cantidad)`

Cada clase concreta sobrescribe este método según su propia unidad de operación.

**Dron**

`Horas de vuelo × Costo por hora`

**RoverTerrestre**

`Kilómetros recorridos × Costo por kilómetro`

**EstacionSensores**

`Días utilizados × Costo diario`

La entidad `AsignacionRecurso` conserva una propiedad `CantidadOperacion`.

El significado de esta cantidad depende del recurso:

- Dron → horas.
- RoverTerrestre → kilómetros.
- EstacionSensores → días.

La cantidad también se persiste en:

`asignacion_recurso.cantidad_operacion`

Esto permite reconstruir posteriormente el costo de una misión aunque la aplicación se cierre y vuelva a ejecutarse.

El cálculo general no necesita comprobar mediante cadenas de `if` o `switch` cuál es el tipo concreto del recurso. Se utiliza el comportamiento polimórfico definido en `CalcularCostoOperacion(...)`.

---

## 7. Protocolo de Seguridad ORBITA

Antes de iniciar una misión, el sistema comprueba las siguientes condiciones:

1. La misión debe contener al menos un recurso.
2. Ningún recurso asignado puede encontrarse en mantenimiento.
3. Ningún recurso puede encontrarse asignado simultáneamente a otra misión activa.
4. La fecha de finalización estimada no puede ser anterior a la fecha de inicio.
5. El responsable debe encontrarse activo.
6. Todos los campos obligatorios deben encontrarse completos.

Las reglas propias de la misión son comprobadas en el dominio, mientras que aquellas que requieren conocer otras asignaciones almacenadas utilizan también la capa de persistencia.

Cuando una condición no se cumple, el sistema impide iniciar la misión y muestra un mensaje comprensible al usuario.

---

## 8. Principios SOLID identificados

### 8.1. Responsabilidad Única — SRP

**Dónde se identifica:**

La aplicación separa las responsabilidades entre dominio, persistencia y presentación.

Por ejemplo:

- `Mision` administra reglas y comportamiento relacionados con una misión.
- `MisionRepositorio` administra su persistencia.
- `Program.cs` presenta información y captura las decisiones del usuario.

De esta manera, una clase de dominio no necesita conocer Entity Framework ni realizar operaciones de consola.

**Problema que evita:**

Evita concentrar presentación, acceso a datos y lógica del negocio dentro de clases monolíticas. Esto facilita el mantenimiento y permite modificar una responsabilidad sin afectar innecesariamente las demás.

### 8.2. Abierto/Cerrado — OCP

**Dónde se identifica:**

La jerarquía:

`RecursoExploracion`

con:

- `Dron`
- `RoverTerrestre`
- `EstacionSensores`

permite extender el comportamiento mediante nuevas clases derivadas.

Cada tipo implementa su propia versión de:

`CalcularCostoOperacion(...)`

sin modificar el algoritmo general utilizado por `Mision` para obtener el costo.

Por ejemplo, un nuevo recurso podría implementar su propia fórmula manteniendo sin modificaciones la lógica polimórfica de cálculo de la misión.

La incorporación de un nuevo tipo sí podría requerir ampliar la persistencia para almacenar sus atributos específicos.

**Problema que evita:**

Reduce la necesidad de modificar grandes bloques de condiciones cada vez que se incorpora un comportamiento nuevo.

### 8.3. Sustitución de Liskov — LSP

**Dónde se identifica:**

`Dron`, `RoverTerrestre` y `EstacionSensores` pueden utilizarse mediante referencias de tipo `RecursoExploracion`.

Por ejemplo, una colección:

`List<RecursoExploracion>`

puede contener cualquiera de los tres tipos concretos.

El sistema puede recorrer esta colección e invocar el comportamiento común sin conocer anticipadamente el tipo específico.

**Problema que evita:**

Evita dependencias innecesarias hacia las clases concretas y permite trabajar de forma uniforme con diferentes recursos de exploración.

### 8.4. Segregación de Interfaces — ISP

**Dónde se identifica:**

La interfaz `IAsignable` contiene únicamente las operaciones relacionadas con asignación y disponibilidad de recursos.

No incluye métodos relacionados con autenticación, persistencia, usuarios o estadísticas.

**Problema que evita:**

Evita contratos excesivamente grandes que obliguen a las clases a implementar comportamientos que no necesitan.

---

## 9. Patrón de diseño propuesto para una versión futura

### Factory Method

**Clasificación:** Patrón creacional.

Actualmente la creación de recursos concretos requiere determinar qué clase debe instanciarse según el tipo seleccionado.

Una evolución futura podría utilizar Factory Method para separar esta creación de la interfaz y de otras partes de la aplicación.

### Participantes propuestos

**Producto abstracto:**

`RecursoExploracion`

**Productos concretos:**

- `Dron`
- `RoverTerrestre`
- `EstacionSensores`

**Creador o fábrica:**

`RecursoFactory`

Podría definir una operación como:

`CrearRecurso(...)`

**Fábricas concretas propuestas:**

- `DronFactory`
- `RoverFactory`
- `EstacionSensoresFactory`

### Problema que resolvería

Factory Method permitiría centralizar la creación de recursos especializados y reduciría la lógica condicional utilizada para determinar qué clase concreta debe construirse.

Si en una versión futura se incorpora un nuevo recurso, como `VehiculoLunar`, podría añadirse la nueva entidad y su fábrica correspondiente sin concentrar más lógica de creación en la interfaz de usuario.

La implementación de Factory Method se propone para una versión futura; no es necesaria para el funcionamiento de la versión actual.

---

## 10. Panel de Control ORBITA

El Panel de Control obtiene información dinámicamente desde los objetos y registros almacenados.

Presenta:

- cantidad total de misiones;
- misiones planificadas;
- misiones en ejecución;
- misiones finalizadas;
- recursos disponibles;
- recursos asignados;
- recursos en mantenimiento;
- costo estimado de una misión seleccionada.

El costo utiliza las asignaciones registradas y su `CantidadOperacion`, delegando el cálculo específico a cada recurso mediante polimorfismo.

Las asignaciones liberadas se conservan como historial, permitiendo conocer los recursos utilizados anteriormente por una misión.

---

## 11. Validaciones y manejo de errores

La aplicación realiza validaciones en diferentes niveles.

### Consola

Se validan:

- campos obligatorios;
- longitudes máximas;
- opciones de menú;
- números enteros;
- números decimales positivos;
- fechas;
- prioridades;
- roles;
- confirmaciones.

Cuando se espera un valor numérico y el usuario escribe una entrada inválida, el programa vuelve a solicitar el dato en lugar de convertirlo silenciosamente en cero.

Las fechas utilizan un formato controlado:

`yyyy-MM-dd`

para mantener un comportamiento consistente entre diferentes computadoras.

### Dominio

Las reglas relacionadas con estados, disponibilidad y ejecución de misiones se validan en las entidades correspondientes.

### Persistencia

SQL Server incorpora mecanismos adicionales de integridad mediante:

- claves primarias;
- claves foráneas;
- restricciones `UNIQUE`;
- restricciones `CHECK`;
- campos `NOT NULL`;
- control de asignaciones activas.

---

## 12. Comprobaciones manuales recomendadas

### Credenciales incorrectas

Ingresar un usuario o contraseña incorrectos.

**Resultado esperado:** el sistema rechaza el inicio de sesión sin finalizar la aplicación.

### Misión sin recursos

Crear o seleccionar una misión planificada sin recursos e intentar iniciarla.

**Resultado esperado:** la misión no inicia y se informa que necesita al menos un recurso asignado.

### Recurso en mantenimiento

Intentar utilizar un recurso que se encuentre en estado `Mantenimiento`.

**Resultado esperado:** el sistema rechaza la operación debido a la falta de disponibilidad.

### Recurso con asignación activa

Intentar asignar a otra misión un recurso que ya se encuentre vinculado a una asignación activa.

**Resultado esperado:** la operación es rechazada y se mantiene una sola asignación activa para el recurso.

### Fecha final anterior a la inicial

Intentar crear o modificar una misión utilizando una fecha final anterior a la fecha de inicio.

**Resultado esperado:** el sistema rechaza el dato y solicita una fecha válida.

### Entrada numérica incorrecta

Ingresar letras o valores negativos cuando el sistema solicite costos, autonomía, alcance, kilómetros, horas o días.

**Resultado esperado:** el dato no es aceptado y se solicita nuevamente.

### Consulta de costo histórico

Asignar recursos a una misión indicando sus cantidades de operación, finalizar la misión y volver a consultarla.

**Resultado esperado:** las asignaciones históricas permanecen disponibles y el costo puede reconstruirse utilizando las cantidades almacenadas.

---

## 13. Observaciones sobre persistencia

La relación entre misiones y recursos se conserva mediante `asignacion_recurso`.

Una asignación mantiene:

- misión;
- recurso;
- fecha de asignación;
- fecha de liberación;
- estado activo o inactivo;
- cantidad de operación.

De esta manera se puede conocer tanto qué recursos se encuentran actualmente asignados como cuáles fueron utilizados históricamente.

La aplicación utiliza esta información para conservar la trazabilidad de las operaciones y reconstruir los costos de las misiones.

---

## 14. Entregables relacionados

El proyecto debe acompañarse de:

- solución/proyecto de Visual Studio;
- script `BD/orbita_control.sql`;
- diagrama UML de clases;
- código fuente;
- este archivo README;
- evidencias de las pruebas realizadas.
