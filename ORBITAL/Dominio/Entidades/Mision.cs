using ORBITAL.Dominio.Enumeraciones;
using ORBITAL.Dominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class Mision
    {
        // Atributos
        private int id;
        private string codigo;
        private string nombre;
        private string descripcion;
        private DateTime fechaInicio;
        private DateTime fechaFinEstimada;
        private PrioridadMision prioridad;
        private EstadoMision estado;
        private Usuario responsable;
        private List<RecursoExploracion> recursos =
            new List<RecursoExploracion>();
        private List<AsignacionRecurso> asignaciones =
            new List<AsignacionRecurso>();

        // Getters y Setters
        public int Id
        {
            get => id;
            set => id = value;
        }

        public string Codigo
        {
            get => codigo;
            set => codigo = value;
        }

        public string Nombre
        {
            get => nombre;
            set => nombre = value;
        }

        public string Descripcion
        {
            get => descripcion;
            set => descripcion = value;
        }

        public DateTime FechaInicio
        {
            get => fechaInicio;
            set => fechaInicio = value;
        }

        public DateTime FechaFinEstimada
        {
            get => fechaFinEstimada;
            set => fechaFinEstimada = value;
        }

        public PrioridadMision Prioridad
        {
            get => prioridad;
            set => prioridad = value;
        }

        public EstadoMision Estado
        {
            get => estado;
            set => estado = value;
        }

        public Usuario Responsable
        {
            get => responsable;
            set => responsable = value;
        }

        public List<RecursoExploracion> Recursos
        {
            get => recursos;
            set => recursos =
                value ?? new List<RecursoExploracion>();
        }

        public List<AsignacionRecurso> Asignaciones
        {
            get => asignaciones;
            set => asignaciones =
                value ?? new List<AsignacionRecurso>();
        }

        // Constructores públicos
        public Mision(
            string codigo,
            string nombre,
            Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.responsable = responsable;

            this.estado = EstadoMision.Planificada;

            this.recursos =
                new List<RecursoExploracion>();

            this.asignaciones =
                new List<AsignacionRecurso>();
        }

        public Mision(
            string codigo,
            string nombre,
            string descripcion,
            DateTime fechaInicio,
            DateTime fechaFin,
            PrioridadMision prioridad,
            Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.descripcion = descripcion;
            this.fechaInicio = fechaInicio;
            this.fechaFinEstimada = fechaFin;
            this.prioridad = prioridad;
            this.responsable = responsable;

            this.estado = EstadoMision.Planificada;

            this.recursos =
                new List<RecursoExploracion>();

            this.asignaciones =
                new List<AsignacionRecurso>();
        }

        public Mision(
            int id,
            string codigo,
            string nombre,
            string descripcion,
            DateTime fechaInicio,
            DateTime fechaFinEstimada,
            PrioridadMision prioridad,
            EstadoMision estado,
            Usuario responsable)
        {
            this.id = id;
            this.codigo = codigo;
            this.nombre = nombre;
            this.descripcion = descripcion;
            this.fechaInicio = fechaInicio;
            this.fechaFinEstimada = fechaFinEstimada;
            this.prioridad = prioridad;
            this.estado = estado;
            this.responsable = responsable;

            this.recursos =
                new List<RecursoExploracion>();

            this.asignaciones =
                new List<AsignacionRecurso>();
        }

        // Métodos públicos
        public void AsignarRecurso(
            RecursoExploracion recurso)
        {
            AsignarRecurso(recurso, 1);
        }

        public void AsignarRecurso(
            RecursoExploracion recurso,
            decimal cantidadOperacion)
        {
            if (recurso == null)
            {
                throw new ArgumentNullException(
                    nameof(recurso),
                    "El recurso a asignar no puede ser nulo."
                );
            }

            if (!recurso.EstaDisponible())
            {
                throw new RecursoNoDisponibleException(
                    $"No se puede asignar el recurso " +
                    $"{recurso.Codigo} ({recurso.Modelo}). " +
                    $"Su estado actual es: {recurso.Estado}."
                );
            }

            if (!recursos.Contains(recurso))
            {
                recursos.Add(recurso);

                var asignacion =
                    new AsignacionRecurso(
                        this,
                        recurso,
                        cantidadOperacion
                    );

                asignaciones.Add(asignacion);
            }
        }

        public void RetirarRecurso(
            RecursoExploracion recurso)
        {
            if (recurso != null &&
                recursos.Contains(recurso))
            {
                recursos.Remove(recurso);

                var asigActiva =
                    asignaciones.FirstOrDefault(
                        a =>
                            a.Recurso != null &&
                            a.Recurso.Codigo ==
                            recurso.Codigo &&
                            a.EstaActiva()
                    );

                if (asigActiva != null)
                {
                    asigActiva.Liberar();
                }
                else
                {
                    recurso.Liberar();
                }
            }
        }

        public void Iniciar()
        {
            if (this.estado ==
                EstadoMision.EnEjecucion)
            {
                throw new MisionInvalidaException(
                    $"La misión {codigo} ya se " +
                    $"encuentra en ejecución."
                );
            }

            if (this.estado ==
                    EstadoMision.Finalizada ||
                this.estado ==
                    EstadoMision.Cancelada)
            {
                throw new MisionInvalidaException(
                    $"No se puede iniciar la misión " +
                    $"{codigo} porque su estado es " +
                    $"{estado}."
                );
            }

            ValidarProtocoloSeguridad();

            this.estado =
                EstadoMision.EnEjecucion;

            var recursosActivos =
                asignaciones != null &&
                asignaciones.Count > 0
                    ? asignaciones
                        .Where(
                            a =>
                                a.EstaActiva() &&
                                a.Recurso != null
                        )
                        .Select(
                            a => a.Recurso
                        )
                        .ToList()
                    : recursos;

            foreach (var recurso in recursosActivos)
            {
                if (recurso.Estado ==
                    EstadoRecurso.Disponible)
                {
                    recurso.CambiarEstado(
                        EstadoRecurso.Asignado
                    );
                }
            }
        }

        public void Finalizar()
        {
            if (this.estado !=
                EstadoMision.EnEjecucion)
            {
                throw new MisionInvalidaException(
                    $"Únicamente las misiones en " +
                    $"estado EnEjecución pueden ser " +
                    $"finalizadas. Estado actual de " +
                    $"{codigo}: {estado}."
                );
            }

            this.estado =
                EstadoMision.Finalizada;

            foreach (var asig in asignaciones)
            {
                if (asig.EstaActiva())
                {
                    asig.Liberar();
                }
            }

            foreach (var recurso in recursos)
            {
                recurso.Liberar();
            }
        }

        public void Cancelar()
        {
            if (this.estado ==
                EstadoMision.Finalizada)
            {
                throw new MisionInvalidaException(
                    "No se puede cancelar una " +
                    "misión que ya ha finalizado."
                );
            }

            this.estado =
                EstadoMision.Cancelada;

            foreach (var asig in asignaciones)
            {
                if (asig.EstaActiva())
                {
                    asig.Liberar();
                }
            }

            foreach (var recurso in recursos)
            {
                recurso.Liberar();
            }
        }

        public bool ValidarProtocoloSeguridad()
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "El código de la misión es " +
                    "obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "El nombre de la misión es " +
                    "obligatorio."
                );
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "La descripción de la misión es " +
                    "obligatoria."
                );
            }

            if (fechaFinEstimada < fechaInicio)
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "La fecha de finalización estimada " +
                    "no puede ser anterior a la fecha " +
                    "de inicio."
                );
            }

            if (responsable == null)
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "La misión debe tener un " +
                    "responsable asignado."
                );
            }

            if (!responsable.EstaActivo())
            {
                throw new MisionInvalidaException(
                    $"Protocolo de seguridad ORBITA: " +
                    $"El responsable " +
                    $"{responsable.NombreUsuario} " +
                    $"no se encuentra activo."
                );
            }

            var recursosActivos =
                asignaciones != null &&
                asignaciones.Count > 0
                    ? asignaciones
                        .Where(
                            a =>
                                a.EstaActiva() &&
                                a.Recurso != null
                        )
                        .Select(
                            a => a.Recurso
                        )
                        .ToList()
                    : recursos;

            if (recursosActivos == null ||
                recursosActivos.Count == 0)
            {
                throw new MisionInvalidaException(
                    "Protocolo de seguridad ORBITA: " +
                    "La misión debe contener al menos " +
                    "un recurso asignado."
                );
            }

            foreach (var recurso in recursosActivos)
            {
                if (recurso.Estado ==
                    EstadoRecurso.Mantenimiento)
                {
                    throw new
                        RecursoNoDisponibleException(
                            $"Protocolo de seguridad " +
                            $"ORBITA: El recurso " +
                            $"{recurso.Codigo} " +
                            $"({recurso.Modelo}) se " +
                            $"encuentra en mantenimiento."
                        );
                }
            }

            return true;
        }

        public decimal CalcularCostoEstimado()
        {
            if (asignaciones == null ||
                asignaciones.Count == 0)
            {
                return 0;
            }

            decimal total = 0;

            bool tieneActivas =
                asignaciones.Any(
                    a => a.EstaActiva()
                );

            foreach (var asig in asignaciones)
            {
                if (asig.Recurso != null &&
                    (!tieneActivas ||
                     asig.EstaActiva()))
                {
                    total +=
                        asig.CalcularCosto();
                }
            }

            return total;
        }

        public override string ToString()
        {
            return
                $"Misión: {nombre} [{codigo}] | " +
                $"Estado: {estado} | " +
                $"Prioridad: {prioridad} | " +
                $"Responsable: " +
                $"{(responsable != null " +
                    "? responsable.NombreUsuario " +
                    ": \"Sin asignar\")} | " +
                $"Recursos: {recursos.Count}";
        }
    }
}
