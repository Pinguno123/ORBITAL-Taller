using ORBITAL.App;
using ORBITAL.Datos.Repositorios;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using ORBITAL.Dominio.Excepciones;
using System;
using System.Collections.Generic;
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
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine("        SISTEMA ORBITA CONTROL - CENTRO DE CONTROL DE MISIONES                 ");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
            Console.WriteLine("\nCredenciales de prueba del taller:");
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

            Console.Clear();
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
            Console.WriteLine("3.  Asignar recurso a misión");
            Console.WriteLine("4.  Retirar recurso de misión");
            Console.WriteLine("5.  Iniciar misión (Protocolo de Seguridad ORBITA)");
            Console.WriteLine("6.  Finalizar misión");
            Console.WriteLine("7.  Cancelar misión");
            Console.WriteLine("8.  Calcular costo estimado de operación de una misión");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("9.  Consultar todos los recursos");
            Console.WriteLine("10. Consultar recursos disponibles");
            Console.WriteLine("11. Registrar nuevo recurso (Dron, Rover, Estación)");
            Console.WriteLine("12. Cambiar estado de un recurso");
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("13. Consultar usuarios del sistema");
            Console.WriteLine("14. Panel de Control ORBITA (Estadísticas)");
            Console.WriteLine("0.  Cerrar sesión");
            Console.Write("\nSeleccione una opción: ");

            string opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1": ListarMisiones(); break;
                case "2": CrearMision(); break;
                case "3": AsignarRecursoAMision(); break;
                case "4": RetirarRecursoDeMision(); break;
                case "5": IniciarMision(); break;
                case "6": FinalizarMision(); break;
                case "7": CancelarMision(); break;
                case "8": CalcularCostoMision(); break;
                case "9": ListarRecursos(soloDisponibles: false); break;
                case "10": ListarRecursos(soloDisponibles: true); break;
                case "11": RegistrarRecurso(); break;
                case "12": CambiarEstadoRecurso(); break;
                case "13": ListarUsuarios(); break;
                case "14": MostrarPanelDeControl(); break;
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
            Console.WriteLine("1. Consultar misiones");
            Console.WriteLine("2. Crear nueva misión");
            Console.WriteLine("3. Asignar recurso a misión");
            Console.WriteLine("4. Retirar recurso de misión");
            Console.WriteLine("5. Iniciar misión (Protocolo de Seguridad ORBITA)");
            Console.WriteLine("6. Finalizar misión");
            Console.WriteLine("7. Calcular costo estimado de operación de una misión");
            Console.WriteLine("8. Consultar recursos disponibles");
            Console.WriteLine("9. Panel de Control ORBITA");
            Console.WriteLine("0. Cerrar sesión");
            Console.Write("\nSeleccione una opción: ");

            string opcion = Console.ReadLine()?.Trim();
            switch (opcion)
            {
                case "1": ListarMisiones(); break;
                case "2": CrearMision(); break;
                case "3": AsignarRecursoAMision(); break;
                case "4": RetirarRecursoDeMision(); break;
                case "5": IniciarMision(); break;
                case "6": FinalizarMision(); break;
                case "7": CalcularCostoMision(); break;
                case "8": ListarRecursos(soloDisponibles: true); break;
                case "9": MostrarPanelDeControl(); break;
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

        #region Operaciones de Misiones

        private static void ListarMisiones()
        {
            Console.Clear();
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
                                Console.WriteLine($"   * {r}");
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CREAR NUEVA MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión (ej. MIS-005): ");
                string codigo = Console.ReadLine()?.Trim();

                Console.Write("Nombre de la misión: ");
                string nombre = Console.ReadLine()?.Trim();

                Console.Write("Descripción: ");
                string descripcion = Console.ReadLine()?.Trim();

                Console.Write("Fecha de inicio (YYYY-MM-DD): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime fechaInicio))
                {
                    Console.WriteLine("Fecha inválida.");
                    Pausar();
                    return;
                }

                Console.Write("Fecha fin estimada (YYYY-MM-DD): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime fechaFin))
                {
                    Console.WriteLine("Fecha inválida.");
                    Pausar();
                    return;
                }

                Console.WriteLine("Prioridad: 0. Baja | 1. Media | 2. Alta");
                Console.Write("Seleccione prioridad (0-2): ");
                if (!int.TryParse(Console.ReadLine(), out int prioridadNum) || prioridadNum < 0 || prioridadNum > 2)
                {
                    prioridadNum = 1; // Media por defecto
                }

                Usuario responsable = sesion.ObtenerUsuarioActual();

                var nuevaMision = new Mision(
                    codigo,
                    nombre,
                    descripcion,
                    fechaInicio,
                    fechaFin,
                    (PrioridadMision)prioridadNum,
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== ASIGNAR RECURSO A MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión (ej. MIS-001): ");
                string codigoMision = Console.ReadLine()?.Trim();
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
                }
                else
                {
                    foreach (var d in disponibles)
                    {
                        Console.WriteLine($"  * [{d.Id}] {d}");
                    }
                }

                Console.Write("\nIngrese el código del recurso a asignar (ej. DRN-002): ");
                string codigoRecurso = Console.ReadLine()?.Trim();
                var recurso = recursoRepo.ObtenerPorCodigo(codigoRecurso);

                if (recurso == null)
                {
                    Console.WriteLine("Recurso no encontrado.");
                    Pausar();
                    return;
                }

                // Validación de dominio (lanza RecursoNoDisponibleException si no está disponible)
                mision.AsignarRecurso(recurso);

                // Persistir en base de datos
                asignacionRepo.RegistrarAsignacion(mision.Id, recurso.Id);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{recurso.Codigo}' asignado exitosamente a la misión '{mision.Codigo}'.");
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== RETIRAR RECURSO DE MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión: ");
                string codigoMision = Console.ReadLine()?.Trim();
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

                Console.Write("\nIngrese el código del recurso a retirar: ");
                string codigoRecurso = Console.ReadLine()?.Trim();
                var recurso = mision.Recursos.FirstOrDefault(r => r.Codigo == codigoRecurso);

                if (recurso == null)
                {
                    Console.WriteLine("El recurso no pertenece a la misión seleccionada.");
                    Pausar();
                    return;
                }

                mision.RetirarRecurso(recurso);
                asignacionRepo.LiberarRecursoDeMision(mision.Id, recurso.Id);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Recurso '{recurso.Codigo}' retirado y devuelto al estado Disponible.");
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== INICIAR MISIÓN - PROTOCOLO DE SEGURIDAD ORBITA ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión a iniciar (ej. MIS-002): ");
                string codigo = Console.ReadLine()?.Trim();
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                Console.WriteLine($"\nValidando Protocolo de Seguridad para misión '{mision.Nombre}'...");

                // Este método ejecuta automáticamente las 6 condiciones del Protocolo de Seguridad
                // y lanza MisionInvalidaException o RecursoNoDisponibleException si alguna falla
                mision.Iniciar();

                // Persistir cambio a EnEjecucion en la base de datos
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
                Console.WriteLine($"\n[ERROR INESPERADO] {ex.Message}");
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== FINALIZAR MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión a finalizar: ");
                string codigo = Console.ReadLine()?.Trim();
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                mision.Finalizar();
                misionRepo.CambiarEstado(mision.Id, EstadoMision.Finalizada);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{mision.Codigo}' finalizada. Los recursos asignados han sido liberados en la base de datos.");
                Console.ResetColor();
            }
            catch (MisionInvalidaException mex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR DE REGLA] {mex.Message}");
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
                Console.WriteLine("[DENEGADO] Solo los Administradores pueden cancelar misiones.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CANCELAR MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión a cancelar: ");
                string codigo = Console.ReadLine()?.Trim();
                var mision = misionRepo.ObtenerPorCodigo(codigo);

                if (mision == null)
                {
                    Console.WriteLine("Misión no encontrada.");
                    Pausar();
                    return;
                }

                mision.Cancelar();
                misionRepo.CambiarEstado(mision.Id, EstadoMision.Cancelada);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[ÉXITO] Misión '{mision.Codigo}' cancelada y recursos liberados.");
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
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CÁLCULO POLIMÓRFICO DE COSTO ESTIMADO DE MISIÓN ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código de la misión: ");
                string codigo = Console.ReadLine()?.Trim();
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
                Console.WriteLine($"Duración estimada: {dias} día(s)");

                Console.WriteLine($"\nRecursos asignados ({mision.Recursos.Count}):");
                foreach (var r in mision.Recursos)
                {
                    // Cada recurso ejecuta polimórficamente su propio cálculo
                    decimal costoIndividual = r.CalcularCostoOperacion(dias);
                    Console.WriteLine($"  * {r.Codigo} ({r.Modelo}) -> Costo estimado: ${costoIndividual:N2}");
                }

                // Invocación polimórfica sin if ni switch
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
            Console.Clear();
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
                Console.WriteLine("[DENEGADO] Solo los Administradores pueden dar de alta nuevos recursos.");
                Console.ResetColor();
                Pausar();
                return;
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== REGISTRAR NUEVO RECURSO DE EXPLORACIÓN ===");
            Console.ResetColor();

            try
            {
                Console.WriteLine("Seleccione el tipo de recurso:");
                Console.WriteLine("1. Dron de Exploración");
                Console.WriteLine("2. Rover Terrestre");
                Console.WriteLine("3. Estación de Sensores");
                Console.Write("Tipo (1-3): ");
                string tipo = Console.ReadLine()?.Trim();

                Console.Write("Código único (ej. DRN-004, ROV-004, EST-004): ");
                string codigo = Console.ReadLine()?.Trim();

                Console.Write("Modelo: ");
                string modelo = Console.ReadLine()?.Trim();

                RecursoExploracion nuevo = null;

                switch (tipo)
                {
                    case "1":
                        Console.Write("Autonomía de vuelo en horas: ");
                        decimal.TryParse(Console.ReadLine(), out decimal autonomiaVuelo);
                        Console.Write("Alcance en kilómetros: ");
                        decimal.TryParse(Console.ReadLine(), out decimal alcance);
                        Console.Write("Costo por hora ($): ");
                        decimal.TryParse(Console.ReadLine(), out decimal costoHora);

                        nuevo = new Dron(codigo, modelo, autonomiaVuelo, alcance, costoHora);
                        break;

                    case "2":
                        Console.Write("Autonomía en kilómetros: ");
                        decimal.TryParse(Console.ReadLine(), out decimal autonomiaRover);
                        Console.Write("Capacidad de carga en kg: ");
                        decimal.TryParse(Console.ReadLine(), out decimal carga);
                        Console.Write("Costo por kilómetro ($): ");
                        decimal.TryParse(Console.ReadLine(), out decimal costoKm);

                        nuevo = new RoverTerrestre(codigo, modelo, autonomiaRover, carga, costoKm);
                        break;

                    case "3":
                        Console.Write("Cantidad de sensores: ");
                        int.TryParse(Console.ReadLine(), out int sensores);
                        Console.Write("Consumo energético (kW): ");
                        decimal.TryParse(Console.ReadLine(), out decimal consumo);
                        Console.Write("Costo diario ($): ");
                        decimal.TryParse(Console.ReadLine(), out decimal costoDiario);

                        nuevo = new EstacionSensores(codigo, modelo, sensores, consumo, costoDiario);
                        break;

                    default:
                        Console.WriteLine("Tipo de recurso inválido.");
                        Pausar();
                        return;
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

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== CAMBIAR ESTADO DE RECURSO ===");
            Console.ResetColor();

            try
            {
                Console.Write("Código del recurso (ej. EST-003): ");
                string codigo = Console.ReadLine()?.Trim();
                var recurso = recursoRepo.ObtenerPorCodigo(codigo);

                if (recurso == null)
                {
                    Console.WriteLine("Recurso no encontrado.");
                    Pausar();
                    return;
                }

                Console.WriteLine($"Recurso actual: {recurso}");
                Console.WriteLine("Estados disponibles: 0. Disponible | 1. Asignado | 2. Mantenimiento");
                Console.Write("Nuevo estado (0-2): ");

                if (int.TryParse(Console.ReadLine(), out int nuevoEstado) && nuevoEstado >= 0 && nuevoEstado <= 2)
                {
                    recursoRepo.CambiarEstado(recurso.Id, (EstadoRecurso)nuevoEstado);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[ÉXITO] Estado del recurso actualizado a: {(EstadoRecurso)nuevoEstado}");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("Estado inválido.");
                }
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

        #region Auditoría y Usuarios

        private static void ListarUsuarios()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("=== USUARIOS REGISTRADOS EN EL SISTEMA ===");
            Console.ResetColor();

            try
            {
                var usuarios = usuarioRepo.ObtenerTodos();
                foreach (var u in usuarios)
                {
                    Console.WriteLine($"  * [ID: {u.Id}] {u}");
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

        private static void ListarAsignacionesActivas()
        {
            Console.Clear();
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

        #region Consulta Especial: Panel de Control ORBITA (Sección 14)

        private static void MostrarPanelDeControl()
        {
            Console.Clear();
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

                if (respuesta == "s")
                {
                    Console.Write("Ingrese código de la misión: ");
                    string cod = Console.ReadLine()?.Trim();
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
            Console.ReadKey(true);
        }
    }
}
