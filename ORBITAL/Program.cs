using ORBITAL.App;
using ORBITAL.Datos.Repositorios;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using ORBITAL.Dominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ORBITAL
{
    internal class Program
    {
        private static readonly SesionUsuario sesion = SesionUsuario.Instancia;
        private static readonly UsuarioRepositorio usuarioRepo = new UsuarioRepositorio();
        private static readonly RecursoRepositorio recursoRepo = new RecursoRepositorio();
        private static readonly MisionRepositorio misionRepo = new MisionRepositorio();
        private static readonly AsignacionRepositorio asignacionRepo = new AsignacionRepositorio();

        static void Main(string[] args)
        {
            Console.Title = "ORBITA Control - Centro de Control de Misiones Científicas";

            while (true)
            {
                if (sesion.ObtenerUsuarioActual() == null)
                {
                    MostrarPantallaLogin();
                }
                else
                {
                    MostrarMenuPrincipal();
                }
            }
        }

        #region Autenticación y Login

        private static void MostrarPantallaLogin()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine("        SISTEMA ORBITA CONTROL - CENTRO DE CONTROL DE MISIONES                 ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
            Console.WriteLine("\nCredenciales de acceso predeterminadas:");
            Console.WriteLine("  * Administrador : admin_orbita       / Admin123");
            Console.WriteLine("  * Coordinador   : coordinador_orbita / Coord123");
            Console.WriteLine("  * Auditor       : auditor_orbita     / Audit123");
            Console.WriteLine("--------------------------------------------------------------------------------");

            Console.Write("\nIngrese nombre de usuario (o '0' para salir): ");
            string usuario = Console.ReadLine()?.Trim();

            if (usuario == "0")
            {
                Environment.Exit(0);
            }

            Console.Write("Ingrese contraseña: ");
            string contrasena = LeerContrasenaOculta();

            try
            {
                bool loginExitoso = sesion.IniciarSesion(usuario, contrasena);

                if (loginExitoso)
                {
                    Usuario actual = sesion.ObtenerUsuarioActual();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[ÉXITO] Bienvenido/a, {actual.NombreUsuario} ({actual.Rol}).");
                    Console.ResetColor();
                    Pausar();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[ERROR] Credenciales incorrectas o usuario inactivo.");
                    Console.ResetColor();
                    Pausar();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR DE ACCESO A DATOS] No se pudo conectar a la base de datos: {ex.Message}");
                Console.ResetColor();
                Pausar();
            }
        }

        private static string LeerContrasenaOculta()
        {
            string pass = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, pass.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return pass;
        }

        #endregion

        #region Menú Principal por Roles

        private static void MostrarMenuPrincipal()
        {
            Usuario actual = sesion.ObtenerUsuarioActual();

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine($"  ORBITA CONTROL | Usuario: {actual.NombreUsuario} | Rol: {actual.Rol} ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();

            switch (actual.Rol)
            {
                case RolUsuario.Administrador:
                    MenuAdministrador();
                    break;
                case RolUsuario.Coordinador:
                    MenuCoordinador();
                    break;
                case RolUsuario.Auditor:
                    MenuAuditor();
                    break;
            }
        }

        private static void MenuAdministrador()
        {
            Console.WriteLine("\n--- MÓDULO ADMINISTRADOR (ACCESO TOTAL) ---");
            Console.WriteLine("1.  Consultar todas las misiones");
            Console.WriteLine("2.  Crear nueva misión");
            Console.WriteLine("3.  Modificar datos de una misión");
            Console.WriteLine("4.  Asignar recurso a misión");
            Console.WriteLine("5.  Retirar recurso de misión");
            Console.WriteLine("6.  Iniciar misión (Protocolo de Seguridad ORBITA)");
            Console.WriteLine("7.  Finalizar misión");
            Console.WriteLine("8.  Cancelar misión");
            Console.WriteLine("9.  Calcular costo estimado de operación de una misión");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("10. Consultar todos los recursos");
            Console.WriteLine("11. Consultar recursos disponibles");
            Console.WriteLine("12. Registrar nuevo recurso (Dron, Rover, Estación)");
            Console.WriteLine("13. Modificar datos de un recurso");
            Console.WriteLine("14. Cambiar estado de un recurso");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("15. Gestión de usuarios (Consultar, Registrar, Cambiar Estado)");
            Console.WriteLine("16. Panel de Control ORBITA (Estadísticas)");
            Console.WriteLine("0.  Cerrar sesión");
            Console.Write("\nSeleccione una opción: ");

            string opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1": ListarMisiones(); break;
                case "2": CrearMision(); break;
                case "3": ModificarMision(); break;
                case "4": AsignarRecursoAMision(); break;
                case "5": RetirarRecursoDeMision(); break;
                case "6": IniciarMision(); break;
                case "7": FinalizarMision(); break;
                case "8": CancelarMision(); break;
                case "9": CalcularCostoMision(); break;
                case "10": ListarRecursos(soloDisponibles: false); break;
                case "11": ListarRecursos(soloDisponibles: true); break;
                case "12": RegistrarRecurso(); break;
                case "13": ModificarRecurso(); break;
                case "14": CambiarEstadoRecurso(); break;
                case "15": GestionUsuarios(); break;
                case "16": MostrarPanelDeControl(); break;
                case "0": sesion.CerrarSesion(); break;
                default:
                    Console.WriteLine("Opción no válida.");
                    Pausar();
                    break;
            }
        }

        private static void MenuCoordinador()
        {
            Console.WriteLine("\n--- MÓDULO COORDINADOR (GESTIÓN DE MISIONES Y RECURSOS) ---");
            Console.WriteLine("1.  Consultar misiones");
            Console.WriteLine("2.  Crear nueva misión");
            Console.WriteLine("3.  Modificar datos de una misión");
            Console.WriteLine("4.  Asignar recurso a misión");
            Console.WriteLine("5.  Retirar recurso de misión");
            Console.WriteLine("6.  Iniciar misión (Protocolo de Seguridad ORBITA)");
            Console.WriteLine("7.  Finalizar misión");
            Console.WriteLine("8.  Calcular costo estimado de operación de una misión");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("9.  Consultar recursos disponibles");
            Console.WriteLine("10. Panel de Control ORBITA");
            Console.WriteLine("0.  Cerrar sesión");
            Console.Write("\nSeleccione una opción: ");

            string opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1": ListarMisiones(); break;
                case "2": CrearMision(); break;
                case "3": ModificarMision(); break;
                case "4": AsignarRecursoAMision(); break;
                case "5": RetirarRecursoDeMision(); break;
                case "6": IniciarMision(); break;
                case "7": FinalizarMision(); break;
                case "8": CalcularCostoMision(); break;
                case "9": ListarRecursos(soloDisponibles: true); break;
                case "10": MostrarPanelDeControl(); break;
                case "0": sesion.CerrarSesion(); break;
                default:
                    Console.WriteLine("Opción no válida.");
                    Pausar();
                    break;
            }
        }

        private static void MenuAuditor()
        {
            Console.WriteLine("\n--- MÓDULO AUDITOR (SOLO LECTURA / CONSULTAS) ---");
            Console.WriteLine("1. Consultar todas las misiones");
            Console.WriteLine("2. Consultar todos los recursos");
            Console.WriteLine("3. Consultar recursos disponibles");
            Console.WriteLine("4. Consultar asignaciones activas (Vista auditoría)");
            Console.WriteLine("5. Panel de Control ORBITA");
            Console.WriteLine("0. Cerrar sesión");
            Console.Write("\nSeleccione una opción: ");

            string opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1": ListarMisiones(); break;
                case "2": ListarRecursos(soloDisponibles: false); break;
                case "3": ListarRecursos(soloDisponibles: true); break;
                case "4": ListarAsignacionesActivas(); break;
                case "5": MostrarPanelDeControl(); break;
                case "0": sesion.CerrarSesion(); break;
                default:
                    Console.WriteLine("Opción no válida.");
                    Pausar();
                    break;
            }
        }

        #endregion

        #region Métodos Auxiliares de Validación y Lectura Robusta

        private static string LeerTextoObligatorio(string prompt, int maxLongitud = 100)
        {
            while (true)
            {
                Console.Write(prompt);
                string entrada = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(entrada))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Este campo es obligatorio y no puede quedar vacío.");
                    Console.ResetColor();
                    continue;
                }

                if (entrada.Length > maxLongitud)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] La longitud no puede superar los {maxLongitud} caracteres (ingresó {entrada.Length}).");
                    Console.ResetColor();
                    continue;
                }

                return entrada;
            }
        }

        private static string LeerTextoOpcional(string prompt, string valorActual, int maxLongitud = 100)
        {
            while (true)
            {
                Console.Write($"{prompt} [Actual: {valorActual}] (Enter para conservar): ");
                string entrada = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(entrada))
                {
                    return valorActual;
                }

                if (entrada.Length > maxLongitud)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] La longitud no puede superar los {maxLongitud} caracteres (ingresó {entrada.Length}).");
                    Console.ResetColor();
                    continue;
                }

                return entrada;
            }
        }

        private static decimal LeerDecimalPositivo(string prompt, decimal min = 0, decimal max = decimal.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string entrada = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(entrada))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Debe ingresar un valor numérico.");
                    Console.ResetColor();
                    continue;
                }

                string entradaNormalizada = entrada.Replace(',', '.');

                if (decimal.TryParse(entradaNormalizada, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultado))
                {
                    if (resultado < min || resultado > max)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] El número debe estar entre {min} y {max}.");
                        Console.ResetColor();
                        continue;
                    }
                    return resultado;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Entrada inválida. Ingrese un número válido (ej. 15.5).");
                Console.ResetColor();
            }
        }

        private static int LeerEnteroPositivo(string prompt, int min = 1, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                string entrada = Console.ReadLine()?.Trim();

                if (int.TryParse(entrada, out int resultado))
                {
                    if (resultado < min || resultado > max)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] Debe ser un número entero entre {min} y {max}.");
                        Console.ResetColor();
                        continue;
                    }
                    return resultado;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Entrada inválida. Ingrese un número entero positivo.");
                Console.ResetColor();
            }
        }

        private static DateTime LeerFecha(string prompt, DateTime? fechaMinima = null)
        {
            while (true)
            {
                Console.Write($"{prompt} (YYYY-MM-DD): ");
                string entrada = Console.ReadLine()?.Trim();

                if (DateTime.TryParseExact(entrada, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    if (fechaMinima.HasValue && fecha <= fechaMinima.Value)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERROR] La fecha debe ser posterior a la fecha de inicio ({fechaMinima.Value:yyyy-MM-dd}).");
                        Console.ResetColor();
                        continue;
                    }
                    return fecha;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] Formato de fecha incorrecto. Debe usar estrictamente el formato YYYY-MM-DD (ejemplo: 2026-05-15).");
                Console.ResetColor();
            }
        }

        private static PrioridadMision LeerPrioridad()
        {
            while (true)
            {
                Console.WriteLine("Prioridad de la misión:");
                Console.WriteLine("  0. Baja");
                Console.WriteLine("  1. Media");
                Console.WriteLine("  2. Alta");
                Console.Write("Seleccione prioridad (0-2): ");

                string entrada = Console.ReadLine()?.Trim().ToLower();

                switch (entrada)
                {
                    case "0":
                    case "baja":
                        return PrioridadMision.Baja;
                    case "1":
                    case "media":
                        return PrioridadMision.Media;
                    case "2":
                    case "alta":
                        return PrioridadMision.Alta;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] Opción de prioridad inválida. Debe seleccionar 0 (Baja), 1 (Media) o 2 (Alta).");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private static RolUsuario LeerRolUsuario()
        {
            while (true)
            {
                Console.WriteLine("Roles disponibles:");
                Console.WriteLine("  0. Administrador");
                Console.WriteLine("  1. Coordinador");
                Console.WriteLine("  2. Auditor");
                Console.Write("Seleccione rol (0-2): ");

                string entrada = Console.ReadLine()?.Trim();
                switch (entrada)
                {
                    case "0": return RolUsuario.Administrador;
                    case "1": return RolUsuario.Coordinador;
                    case "2": return RolUsuario.Auditor;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] Opción de rol inválida. Ingrese 0, 1 o 2.");
                        Console.ResetColor();
                        break;
                }
            }
        }

        private static bool LeerConfirmacion(string prompt)
        {
            while (true)
            {
                Console.Write($"{prompt} (s/n): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();
                if (respuesta == "s" || respuesta == "si") return true;
                if (respuesta == "n" || respuesta == "no") return false;
                Console.WriteLine("Responda 's' para sí o 'n' para no.");
            }
        }

        #endregion

        #region Operaciones de Misiones

        private static void ListarMisiones()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== LISTADO DE MISIONES REGISTRADAS ===");
            Console.ResetColor();

            try
            {
                var misiones = misionRepo.ObtenerTodas();
                if (misiones.Count == 0)
                {
                    Console.WriteLine("No hay misiones registradas.");
                }
                else
                {
                    foreach (var m in misiones)
                    {
                        Console.WriteLine($"\n--------------------------------------------------");
                        Console.WriteLine($"[ID: {m.Id}] Código: {m.Codigo} | Nombre: {m.Nombre}");
                        Console.WriteLine($"Descripción: {m.Descripcion}");
                        Console.WriteLine($"Estado: {m.Estado} | Prioridad: {m.Prioridad}");
                        Console.WriteLine($"Fechas: {m.FechaInicio:yyyy-MM-dd} al {m.FechaFinEstimada:yyyy-MM-dd}");
                        Console.WriteLine($"Responsable: {(m.Responsable != null ? m.Responsable.NombreUsuario : "Sin asignar")}");
                        Console.WriteLine($"Recursos asignados ({m.Recursos.Count}):");
                        if (m.Recursos.Count == 0)
                        {
                            Console.WriteLine("   (Sin recursos asignados)");
                        }
                        else
                        {
                            foreach (var r in m.Recursos)
                            {
                                var asig = m.Asignaciones.FirstOrDefault(a => a.Recurso != null && a.Recurso.Id == r.Id && a.EstaActiva());
                            string unidad = r is Dron ? "horas de vuelo" : (r is RoverTerrestre ? "km" : "días");
                            string detalleUso = asig != null ? $" | Uso asignado: {asig.CantidadOperacion:N2} {unidad}" : "";
                            Console.WriteLine($"   * {r}{detalleUso}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al consultar misiones: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void CrearMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para crear misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CREAR NUEVA MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión (ej. MIS-005, máx 20 caracteres): ", 20);
                string nombre = LeerTextoObligatorio("Nombre de la misión (máx 100 caracteres): ", 100);
                string descripcion = LeerTextoObligatorio("Descripción de la misión (máx 500 caracteres): ", 500);

                DateTime fechaInicio = LeerFecha("Fecha de inicio");
                DateTime fechaFin = LeerFecha("Fecha fin estimada", fechaMinima: fechaInicio);

                PrioridadMision prioridad = LeerPrioridad();
                Usuario responsable = sesion.ObtenerUsuarioActual();

                var nuevaMision = new Mision(
                    codigo,
                    nombre,
                    descripcion,
                    fechaInicio,
                    fechaFin,
                    prioridad,
                    responsable
                );

                misionRepo.Crear(nuevaMision);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{nuevaMision.Codigo}' registrada correctamente en la base de datos.");
                Console.ResetColor();
            }
            catch (MisionInvalidaException mex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[VALIDACIÓN] {mex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] No se pudo crear la misión: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void ModificarMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para modificar misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== MODIFICAR DATOS DE UNA MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión a modificar: ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Misión no encontrada en el sistema.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                if (mision.Estado == EstadoMision.Finalizada || mision.Estado == EstadoMision.Cancelada)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[DENEGADO] No se pueden modificar misiones en estado {mision.Estado}.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nMisión actual: {mision.Nombre} [{mision.Codigo}]");
                Console.WriteLine($"Descripción: {mision.Descripcion}");
                Console.WriteLine($"Fechas: {mision.FechaInicio:yyyy-MM-dd} al {mision.FechaFinEstimada:yyyy-MM-dd}");
                Console.WriteLine($"Prioridad: {mision.Prioridad} | Estado: {mision.Estado}\n");

                mision.Nombre = LeerTextoOpcional("Nuevo nombre", mision.Nombre, 100);
                mision.Descripcion = LeerTextoOpcional("Nueva descripción", mision.Descripcion, 500);

                if (LeerConfirmacion("¿Desea modificar las fechas de la misión?"))
                {
                    DateTime nuevaInicio = LeerFecha("Nueva fecha de inicio");
                    DateTime nuevaFin = LeerFecha("Nueva fecha fin estimada", fechaMinima: nuevaInicio);
                    mision.FechaInicio = nuevaInicio;
                    mision.FechaFinEstimada = nuevaFin;
                }

                if (LeerConfirmacion("¿Desea cambiar la prioridad de la misión?"))
                {
                    mision.Prioridad = LeerPrioridad();
                }

                misionRepo.Actualizar(mision);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{mision.Codigo}' actualizada exitosamente.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] No se pudo modificar la misión: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void AsignarRecursoAMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para asignar recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== ASIGNAR RECURSO A MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigoMision = LeerTextoObligatorio("Código de la misión (ej. MIS-001): ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigoMision);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                if (mision.Estado == EstadoMision.Finalizada || mision.Estado == EstadoMision.Cancelada)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"No se pueden asignar recursos a una misión en estado {mision.Estado}.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nMisión seleccionada: {mision.Nombre} (Estado: {mision.Estado})");

                Console.WriteLine("\nRecursos actualmente disponibles:");
                var disponibles = recursoRepo.ObtenerDisponibles();
                if (disponibles.Count == 0)
                {
                    Console.WriteLine("No hay recursos disponibles en este momento.");
                    Pausar();
                    return;
                }

                foreach (var d in disponibles)
                {
                    Console.WriteLine($"  * [{d.Id}] {d}");
                }

                string codigoRecurso = LeerTextoObligatorio("\nIngrese el código del recurso a asignar (ej. DRN-002): ", 20);
                var recurso = recursoRepo.ObtenerPorCodigo(codigoRecurso);

                if (recurso == null)
                {
                    Console.WriteLine("Recurso no encontrado.");
                    Pausar();
                    return;
                }

                // Solicitar cantidad de operación polimórfica según el tipo de recurso
                decimal cantidadOperacion = 1;
                if (recurso is Dron dron)
                {
                    Console.WriteLine($"\n[TIPO: Dron] Costo por hora de vuelo: ${dron.CostoPorHora:N2}");
                    cantidadOperacion = LeerDecimalPositivo("Horas de vuelo estimadas para esta misión: ", min: 0.1m);
                }
                else if (recurso is RoverTerrestre rover)
                {
                    Console.WriteLine($"\n[TIPO: Rover Terrestre] Costo por kilómetro: ${rover.CostoPorKilometro:N2}");
                    cantidadOperacion = LeerDecimalPositivo("Kilómetros de recorrido estimados para esta misión: ", min: 0.1m);
                }
                else if (recurso is EstacionSensores estacion)
                {
                    Console.WriteLine($"\n[TIPO: Estación de Sensores] Costo diario de operación: ${estacion.CostoDiario:N2}");
                    cantidadOperacion = LeerDecimalPositivo("Días de operación estimados para esta misión: ", min: 0.1m);
                }

                // Asignar en entidad de dominio con cálculo de costo
                mision.AsignarRecurso(recurso, cantidadOperacion);

                // Persistir en base de datos con la cantidad de operación ingresada
                asignacionRepo.RegistrarAsignacion(mision.Id, recurso.Id, cantidadOperacion);

                decimal costoCalculado = recurso.CalcularCostoOperacion(cantidadOperacion);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{recurso.Codigo}' asignado a la misión '{mision.Codigo}'.");
                Console.WriteLine($"Costo de operación estimado para este recurso: ${costoCalculado:N2}");
                Console.ResetColor();
            }
            catch (RecursoNoDisponibleException rex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[RECURSO NO DISPONIBLE] {rex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void RetirarRecursoDeMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para retirar recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== RETIRAR RECURSO DE MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigoMision = LeerTextoObligatorio("Código de la misión: ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigoMision);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                if (mision.Recursos.Count == 0)
                {
                    Console.WriteLine("Esta misión no tiene recursos asignados actualmente.");
                    Pausar();
                    return;
                }

                Console.WriteLine("\nRecursos asignados a esta misión:");
                foreach (var r in mision.Recursos)
                {
                    Console.WriteLine($"  * [{r.Id}] {r}");
                }

                string codigoRecurso = LeerTextoObligatorio("\nIngrese el código del recurso a retirar: ", 20);
                var recurso = mision.Recursos.FirstOrDefault(r => r.Codigo.Equals(codigoRecurso, StringComparison.OrdinalIgnoreCase));

                if (recurso == null)
                {
                    Console.WriteLine("El recurso no pertenece a la misión seleccionada.");
                    Pausar();
                    return;
                }

                mision.RetirarRecurso(recurso);
                asignacionRepo.LiberarRecursoDeMision(mision.Id, recurso.Id);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{recurso.Codigo}' retirado de la misión y devuelto al estado Disponible.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void IniciarMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para iniciar misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== INICIAR MISIÓN - PROTOCOLO DE SEGURIDAD ORBITA ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión a iniciar (ej. MIS-002): ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nVerificando Protocolo de Seguridad ORBITA para la misión '{mision.Nombre}'...");

                // Valida datos obligatorios, fechas, responsable activo, al menos un recurso,
                // ningún recurso en mantenimiento y ningún recurso en otra misión activa
                misionRepo.CambiarEstado(mision.Id, EstadoMision.EnEjecucion);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Protocolo de Seguridad verificado al 100%. La misión '{mision.Codigo}' ha iniciado su ejecución.");
                Console.ResetColor();
            }
            catch (MisionInvalidaException mex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[VIOLACIÓN DE PROTOCOLO DE SEGURIDAD] {mex.Message}");
                Console.ResetColor();
            }
            catch (RecursoNoDisponibleException rex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[FALLA EN RECURSOS ASIGNADOS] {rex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void FinalizarMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Coordinador) && !sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Su rol no tiene permisos para finalizar misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== FINALIZAR MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión a finalizar: ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                // Regla estricta: sólo misiones en EnEjecucion pueden finalizarse
                if (mision.Estado != EstadoMision.EnEjecucion)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[REGLA DE NEGOCIO] Únicamente las misiones en estado 'EnEjecucion' pueden ser finalizadas. Estado actual de '{mision.Codigo}': {mision.Estado}.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                misionRepo.CambiarEstado(mision.Id, EstadoMision.Finalizada);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{mision.Codigo}' finalizada con éxito. Todos sus recursos asignados han sido liberados y están Disponibles.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void CancelarMision()
        {
            if (!sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Solo los Administradores tienen permisos para cancelar misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CANCELAR MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión a cancelar: ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                if (mision.Estado == EstadoMision.Finalizada)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("No se puede cancelar una misión que ya ha finalizado.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                if (!LeerConfirmacion($"¿Está seguro de que desea cancelar la misión '{mision.Codigo}'?"))
                {
                    Console.WriteLine("Operación cancelada por el usuario.");
                    Pausar();
                    return;
                }

                misionRepo.CambiarEstado(mision.Id, EstadoMision.Cancelada);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{mision.Codigo}' cancelada y sus recursos liberados.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void CalcularCostoMision()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CÁLCULO POLIMÓRFICO DE COSTO ESTIMADO DE MISIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código de la misión: ", 20);
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nMisión: {mision.Nombre} [{mision.Codigo}]");
                Console.WriteLine($"Fechas: {mision.FechaInicio:yyyy-MM-dd} al {mision.FechaFinEstimada:yyyy-MM-dd}");
                decimal dias = (decimal)Math.Max(1, Math.Ceiling((mision.FechaFinEstimada - mision.FechaInicio).TotalDays));
                Console.WriteLine($"Duración estimada base: {dias} día(s)");

                Console.WriteLine($"\nRecursos asignados ({mision.Recursos.Count}):");
                if (mision.Recursos.Count == 0)
                {
                    Console.WriteLine("   (No tiene recursos asignados)");
                }
                else
                {
                    foreach (var r in mision.Recursos)
                    {
                        var asig = mision.Asignaciones.FirstOrDefault(a => a.Recurso != null && a.Recurso.Id == r.Id && a.EstaActiva());
                        decimal cantidadUso = asig != null ? asig.CantidadOperacion : dias;
                        decimal costoIndividual = asig != null ? asig.CalcularCosto() : r.CalcularCostoOperacion(cantidadUso);
                        string unidad = r is Dron ? "horas de vuelo" : (r is RoverTerrestre ? "km" : "días");
                        Console.WriteLine($"  * {r.Codigo} ({r.Modelo}) [{r.GetType().Name}] -> Costo ({cantidadUso:N2} {unidad}): ${costoIndividual:N2}");
                    }
                }

                decimal total = mision.CalcularCostoEstimado();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n==================================================");
                Console.WriteLine($" COSTO TOTAL ESTIMADO DE LA MISIÓN: ${total:N2}");
                Console.WriteLine($"==================================================");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        #endregion

        #region Operaciones de Recursos

        private static void ListarRecursos(bool soloDisponibles)
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(soloDisponibles ? "=== RECURSOS DISPONIBLES ===" : "=== TODOS LOS RECURSOS DE EXPLORACIÓN ===");
            Console.ResetColor();

            try
            {
                var recursos = soloDisponibles ? recursoRepo.ObtenerDisponibles() : recursoRepo.ObtenerTodos();

                if (recursos.Count == 0)
                {
                    Console.WriteLine("No se encontraron recursos.");
                }
                else
                {
                    foreach (var r in recursos)
                    {
                        Console.WriteLine($"  * [ID: {r.Id}] {r}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al consultar recursos: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void RegistrarRecurso()
        {
            if (!sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Solo los Administradores pueden registrar nuevos recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== REGISTRAR NUEVO RECURSO DE EXPLORACIÓN ===");
            Console.ResetColor();

            try
            {
                string tipo = "";
                while (true)
                {
                    Console.WriteLine("Seleccione el tipo de recurso:");
                    Console.WriteLine("  1. Dron de Exploración");
                    Console.WriteLine("  2. Rover Terrestre");
                    Console.WriteLine("  3. Estación de Sensores");
                    Console.Write("Tipo (1-3): ");
                    tipo = Console.ReadLine()?.Trim();

                    if (tipo == "1" || tipo == "2" || tipo == "3")
                        break;

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Opción inválida. Debe seleccionar 1, 2 o 3.\n");
                    Console.ResetColor();
                }

                string codigo = LeerTextoObligatorio("Código único (ej. DRN-004, ROV-004, EST-004, máx 20 caracteres): ", 20);
                string modelo = LeerTextoObligatorio("Modelo del recurso (máx 100 caracteres): ", 100);

                RecursoExploracion nuevo = null;

                switch (tipo)
                {
                    case "1":
                        decimal autonomiaVuelo = LeerDecimalPositivo("Autonomía de vuelo en horas: ", min: 0.1m);
                        decimal alcance = LeerDecimalPositivo("Alcance en kilómetros: ", min: 0.1m);
                        decimal costoHora = LeerDecimalPositivo("Costo por hora ($): ", min: 0);

                        nuevo = new Dron(codigo, modelo, autonomiaVuelo, alcance, costoHora);
                        break;

                    case "2":
                        decimal autonomiaRover = LeerDecimalPositivo("Autonomía en kilómetros: ", min: 0.1m);
                        decimal carga = LeerDecimalPositivo("Capacidad de carga en kg: ", min: 0);
                        decimal costoKm = LeerDecimalPositivo("Costo por kilómetro ($): ", min: 0);

                        nuevo = new RoverTerrestre(codigo, modelo, autonomiaRover, carga, costoKm);
                        break;

                    case "3":
                        int sensores = LeerEnteroPositivo("Cantidad de sensores (entero positivo): ", min: 1);
                        decimal consumo = LeerDecimalPositivo("Consumo energético (kW): ", min: 0);
                        decimal costoDiario = LeerDecimalPositivo("Costo diario ($): ", min: 0);

                        nuevo = new EstacionSensores(codigo, modelo, sensores, consumo, costoDiario);
                        break;
                }

                recursoRepo.Registrar(nuevo);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{nuevo.Codigo}' ({nuevo.Modelo}) registrado exitosamente en la base de datos.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void ModificarRecurso()
        {
            if (!sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Solo los Administradores pueden modificar datos de recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== MODIFICAR DATOS DE UN RECURSO DE EXPLORACIÓN ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código del recurso a modificar: ", 20);
                var recurso = recursoRepo.ObtenerPorCodigo(codigo);

                if (recurso == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Recurso no encontrado en el sistema.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nRecurso actual: {recurso}");

                recurso.Modelo = LeerTextoOpcional("Nuevo modelo", recurso.Modelo, 100);

                if (recurso is Dron dron)
                {
                    Console.WriteLine($"\nParámetros del Dron:");
                    dron.AutonomiaVuelo = LeerDecimalPositivo($"Autonomía de vuelo ({dron.AutonomiaVuelo} hrs) -> Nueva: ", min: 0.1m);
                    dron.Alcance = LeerDecimalPositivo($"Alcance ({dron.Alcance} km) -> Nuevo: ", min: 0.1m);
                    dron.CostoPorHora = LeerDecimalPositivo($"Costo por hora (${dron.CostoPorHora:N2}) -> Nuevo: ", min: 0);
                }
                else if (recurso is RoverTerrestre rover)
                {
                    Console.WriteLine($"\nParámetros del Rover Terrestre:");
                    rover.Autonomia = LeerDecimalPositivo($"Autonomía ({rover.Autonomia} km) -> Nueva: ", min: 0.1m);
                    rover.CapacidadCarga = LeerDecimalPositivo($"Capacidad de carga ({rover.CapacidadCarga} kg) -> Nueva: ", min: 0);
                    rover.CostoPorKilometro = LeerDecimalPositivo($"Costo por km (${rover.CostoPorKilometro:N2}) -> Nuevo: ", min: 0);
                }
                else if (recurso is EstacionSensores estacion)
                {
                    Console.WriteLine($"\nParámetros de la Estación de Sensores:");
                    estacion.CantidadSensores = LeerEnteroPositivo($"Cantidad de sensores ({estacion.CantidadSensores}) -> Nueva: ", min: 1);
                    estacion.ConsumoEnergetico = LeerDecimalPositivo($"Consumo energético ({estacion.ConsumoEnergetico} kW) -> Nuevo: ", min: 0);
                    estacion.CostoDiario = LeerDecimalPositivo($"Costo diario (${estacion.CostoDiario:N2}) -> Nuevo: ", min: 0);
                }

                recursoRepo.Modificar(recurso);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{recurso.Codigo}' modificado exitosamente.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] No se pudo modificar el recurso: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void CambiarEstadoRecurso()
        {
            if (!sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Solo los Administradores pueden cambiar manualmente el estado de los recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CAMBIAR ESTADO DE RECURSO ===");
            Console.ResetColor();

            try
            {
                string codigo = LeerTextoObligatorio("Código del recurso (ej. EST-003): ", 20);
                var recurso = recursoRepo.ObtenerPorCodigo(codigo);

                if (recurso == null)
                {
                    Console.WriteLine("Recurso no encontrado.");
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nRecurso seleccionado: {recurso}");
                Console.WriteLine($"Estado actual: {recurso.Estado}");
                Console.WriteLine("\nEstados permitidos para cambio manual:");
                Console.WriteLine("  0. Disponible");
                Console.WriteLine("  2. Mantenimiento");
                Console.WriteLine("  (Nota: 'Asignado' solo es asignado automáticamente por el sistema al vincular el recurso a una misión)");

                EstadoRecurso nuevoEstado;
                while (true)
                {
                    Console.Write("\nSeleccione nuevo estado (0 o 2): ");
                    string entrada = Console.ReadLine()?.Trim();

                    if (entrada == "0")
                    {
                        nuevoEstado = EstadoRecurso.Disponible;
                        break;
                    }
                    if (entrada == "2")
                    {
                        nuevoEstado = EstadoRecurso.Mantenimiento;
                        break;
                    }
                    if (entrada == "1")
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("[ERROR] El estado 'Asignado' no puede asignarse manualmente; se asigna automáticamente al vincular el recurso a una misión.");
                        Console.ResetColor();
                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Opción inválida. Ingrese 0 para Disponible o 2 para Mantenimiento.");
                    Console.ResetColor();
                }

                recursoRepo.CambiarEstado(recurso.Id, nuevoEstado);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Estado del recurso '{recurso.Codigo}' actualizado a: {nuevoEstado}");
                Console.ResetColor();
            }
            catch (InvalidOperationException ioex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[RESTRICCIÓN DE NEGOCIO] {ioex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        #endregion

        #region Módulo de Gestión de Usuarios

        private static void GestionUsuarios()
        {
            if (!sesion.TienePermiso(RolUsuario.Administrador))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[DENEGADO] Solo los Administradores tienen acceso a la gestión de usuarios.");
                Console.ResetColor();
                Pausar();
                return;
            }

            while (true)
            {
                LimpiarConsola();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("=== GESTIÓN DE USUARIOS DEL SISTEMA ===");
                Console.ResetColor();
                Console.WriteLine("1. Consultar todos los usuarios");
                Console.WriteLine("2. Registrar nuevo usuario");
                Console.WriteLine("3. Cambiar estado de un usuario (Activar / Desactivar)");
                Console.WriteLine("0. Volver al menú principal");
                Console.Write("\nSeleccione una opción: ");

                string op = Console.ReadLine()?.Trim();
                switch (op)
                {
                    case "1":
                        ListarUsuarios();
                        break;
                    case "2":
                        RegistrarNuevoUsuario();
                        break;
                    case "3":
                        CambiarEstadoUsuario();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Opción no válida.");
                        Pausar();
                        break;
                }
            }
        }

        private static void ListarUsuarios()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== USUARIOS REGISTRADOS EN EL SISTEMA ===");
            Console.ResetColor();

            try
            {
                var usuarios = usuarioRepo.ObtenerTodos();
                if (usuarios.Count == 0)
                {
                    Console.WriteLine("No hay usuarios registrados.");
                }
                else
                {
                    foreach (var u in usuarios)
                    {
                        Console.WriteLine($"  * [ID: {u.Id}] {u}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al consultar usuarios: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void RegistrarNuevoUsuario()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== REGISTRAR NUEVO USUARIO ===");
            Console.ResetColor();

            try
            {
                string nombreUsuario = LeerTextoObligatorio("Nombre de usuario (login, máx 50 caracteres): ", 50);

                var existente = usuarioRepo.ObtenerPorNombreUsuario(nombreUsuario);
                if (existente != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] Ya existe un usuario registrado con el nombre '{nombreUsuario}'.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                string contrasena = LeerTextoObligatorio("Contraseña (máx 100 caracteres): ", 100);
                RolUsuario rol = LeerRolUsuario();

                var nuevoUsuario = new Usuario(nombreUsuario, contrasena, rol);
                usuarioRepo.Registrar(nuevoUsuario);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Usuario '{nuevoUsuario.NombreUsuario}' registrado como {nuevoUsuario.Rol} con estado Activo.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] No se pudo registrar el usuario: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        private static void CambiarEstadoUsuario()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CAMBIAR ESTADO DE USUARIO ===");
            Console.ResetColor();

            try
            {
                var usuarios = usuarioRepo.ObtenerTodos();
                foreach (var u in usuarios)
                {
                    Console.WriteLine($"  * [ID: {u.Id}] {u.NombreUsuario} | Rol: {u.Rol} | Estado: {u.Estado}");
                }

                int idUsuario = LeerEnteroPositivo("\nIngrese el ID del usuario cuyo estado desea cambiar: ");
                var usuario = usuarioRepo.ObtenerPorId(idUsuario);

                if (usuario == null)
                {
                    Console.WriteLine("Usuario no encontrado.");
                    Pausar();
                    return;
                }

                // Regla de seguridad: un usuario no puede desactivar su propia cuenta activa
                Usuario actual = sesion.ObtenerUsuarioActual();
                if (usuario.Id == actual.Id)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[DENEGADO] Por motivos de seguridad del sistema, no puede modificar el estado de su propio usuario activo.");
                    Console.ResetColor();
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nUsuario: {usuario.NombreUsuario} (Estado actual: {usuario.Estado})");
                Console.WriteLine("Estados:");
                Console.WriteLine("  0. Activo");
                Console.WriteLine("  1. Inactivo");

                EstadoUsuario nuevoEstado;
                while (true)
                {
                    Console.Write("Seleccione nuevo estado (0 o 1): ");
                    string op = Console.ReadLine()?.Trim();
                    if (op == "0") { nuevoEstado = EstadoUsuario.Activo; break; }
                    if (op == "1") { nuevoEstado = EstadoUsuario.Inactivo; break; }
                    Console.WriteLine("Opción inválida. Ingrese 0 o 1.");
                }

                usuarioRepo.CambiarEstado(usuario.Id, nuevoEstado);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Estado del usuario '{usuario.NombreUsuario}' cambiado a: {nuevoEstado}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] No se pudo cambiar el estado del usuario: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        #endregion

        #region Auditoría de Asignaciones

        private static void ListarAsignacionesActivas()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== ASIGNACIONES ACTIVAS (VISTA DE AUDITORÍA) ===");
            Console.ResetColor();

            try
            {
                var asignaciones = asignacionRepo.ObtenerAsignacionesActivas();
                if (asignaciones.Count == 0)
                {
                    Console.WriteLine("No hay asignaciones activas en este momento.");
                }
                else
                {
                    foreach (var a in asignaciones)
                    {
                        Console.WriteLine($"  * Misión: {a.codigo_mision} ({a.nombre_mision}) -> Recurso: {a.codigo_recurso} [{a.tipo_recurso} - {a.modelo_recurso}] desde {a.fecha_asignacion:yyyy-MM-dd HH:mm}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al consultar asignaciones activas: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        #endregion

        #region Panel de Control ORBITA (Estadísticas)

        private static void MostrarPanelDeControl()
        {
            LimpiarConsola();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine("                    PANEL DE CONTROL ORBITA - ESTADÍSTICAS                     ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();

            try
            {
                var misiones = misionRepo.ObtenerTodas();
                var recursos = recursoRepo.ObtenerTodos();

                int totalMisiones = misiones.Count;
                int planificadas = misiones.Count(m => m.Estado == EstadoMision.Planificada);
                int enEjecucion = misiones.Count(m => m.Estado == EstadoMision.EnEjecucion);
                int finalizadas = misiones.Count(m => m.Estado == EstadoMision.Finalizada);
                int canceladas = misiones.Count(m => m.Estado == EstadoMision.Cancelada);

                int disponibles = recursos.Count(r => r.Estado == EstadoRecurso.Disponible);
                int asignados = recursos.Count(r => r.Estado == EstadoRecurso.Asignado);
                int mantenimiento = recursos.Count(r => r.Estado == EstadoRecurso.Mantenimiento);

                Console.WriteLine("\n[MISIONES]");
                Console.WriteLine($"  * Cantidad total de misiones registradas : {totalMisiones}");
                Console.WriteLine($"  * Misiones planificadas                  : {planificadas}");
                Console.WriteLine($"  * Misiones actualmente en ejecución      : {enEjecucion}");
                Console.WriteLine($"  * Misiones finalizadas                   : {finalizadas}");
                Console.WriteLine($"  * Misiones canceladas                    : {canceladas}");

                Console.WriteLine("\n[RECURSOS DE EXPLORACIÓN]");
                Console.WriteLine($"  * Cantidad de recursos disponibles       : {disponibles}");
                Console.WriteLine($"  * Cantidad de recursos asignados         : {asignados}");
                Console.WriteLine($"  * Cantidad de recursos en mantenimiento  : {mantenimiento}");
                Console.WriteLine($"  * Total de recursos registrados          : {recursos.Count}");

                Console.WriteLine("\n--------------------------------------------------------------------------------");
                Console.Write("¿Desea consultar el costo estimado total de una misión específica? (s/n): ");
                string respuesta = Console.ReadLine()?.Trim().ToLower();

                if (respuesta == "s" || respuesta == "si")
                {
                    string cod = LeerTextoObligatorio("Ingrese código de la misión: ", 20);
                    var m = misiones.FirstOrDefault(x => x.Codigo.Equals(cod, StringComparison.OrdinalIgnoreCase));
                    if (m != null)
                    {
                        decimal costo = m.CalcularCostoEstimado();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n>> Misión '{m.Nombre}' [{m.Codigo}]");
                        Console.WriteLine($">> Costo estimado total de operación: ${costo:N2}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine("Misión no encontrada.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al generar panel de control: {ex.Message}");
                Console.ResetColor();
            }

            Pausar();
        }

        #endregion

        private static void Pausar()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            while (Console.KeyAvailable) { Console.ReadKey(true); }
            Console.ReadKey(true);
            while (Console.KeyAvailable) { Console.ReadKey(true); }
        }

        private static void LimpiarConsola()
        {
            try
            {
                Console.Clear();
                Console.Write("\x1b[3J\x1b[H\x1b[2J");
            }
            catch
            {}
        }
    }
}
