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
        private List<RecursoExploracion> recursos = new List<RecursoExploracion>();

        // Getters y Setters
        public int Id { get => id; set => id = value; }
        public string Codigo { get => codigo; set => codigo = value; }
        public string Nombre { get => nombre; set => nombre = value; }
        public string Descripcion { get => descripcion; set => descripcion = value; }
        public DateTime FechaInicio { get => fechaInicio; set => fechaInicio = value; }
        public DateTime FechaFinEstimada { get => fechaFinEstimada; set => fechaFinEstimada = value; }
        public PrioridadMision Prioridad { get => prioridad; set => prioridad = value; }
        public EstadoMision Estado { get => estado; set => estado = value; }
        public Usuario Responsable { get => responsable; set => responsable = value; }
        public List<RecursoExploracion> Recursos { get => recursos; set => recursos = value ?? new List<RecursoExploracion>(); }

        // Constructores públicos
        public Mision(string codigo, string nombre, Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.responsable = responsable;
            this.estado = EstadoMision.Planificada; // Estado inicial por defecto
            this.recursos = new List<RecursoExploracion>();
        }

        public Mision(string codigo, string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFin, PrioridadMision prioridad, Usuario responsable)
        {
            this.codigo = codigo;
            this.nombre = nombre;
            this.descripcion = descripcion;
            this.fechaInicio = fechaInicio;
            this.fechaFinEstimada = fechaFin;
            this.prioridad = prioridad;
            this.responsable = responsable;
            this.estado = EstadoMision.Planificada;
            this.recursos = new List<RecursoExploracion>();
        }

        public Mision(int id, string codigo, string nombre, string descripcion, DateTime fechaInicio, DateTime fechaFinEstimada, PrioridadMision prioridad, EstadoMision estado, Usuario responsable)
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
            this.recursos = new List<RecursoExploracion>();
        }

        // Métodos públicos
        public void AsignarRecurso(RecursoExploracion recurso)
        {
            if (recurso == null)
            {
                throw new ArgumentNullException(nameof(recurso), "El recurso a asignar no puede ser nulo.");
            }

            if (!recurso.EstaDisponible())
            {
                throw new RecursoNoDisponibleException($"No se puede asignar el recurso {recurso.Codigo} ({recurso.Modelo}). Su estado actual es: {recurso.Estado}.");
            }

            if (!recursos.Contains(recurso))
            {
                recursos.Add(recurso);
                recurso.Asignar(this);
            }
        }

        public void RetirarRecurso(RecursoExploracion recurso)
        {
            if (recurso != null && recursos.Contains(recurso))
            {
                recursos.Remove(recurso);
                recurso.Liberar();
            }
        }

        public void Iniciar()
        {
            if (this.estado == EstadoMision.EnEjecucion)
            {
                throw new MisionInvalidaException($"La misión {codigo} ya se encuentra en ejecución.");
            }

            if (this.estado == EstadoMision.Finalizada || this.estado == EstadoMision.Cancelada)
            {
                throw new MisionInvalidaException($"No se puede iniciar la misión {codigo} porque su estado es {estado}.");
            }

            // Comprobación automática del Protocolo de Seguridad ORBITA
            ValidarProtocoloSeguridad();

            this.estado = EstadoMision.EnEjecucion;

            // Asegurar que todos los recursos asignados reflejen el estado Asignado
            foreach (var recurso in recursos)
            {
                if (recurso.Estado == EstadoRecurso.Disponible)
                {
                    recurso.CambiarEstado(EstadoRecurso.Asignado);
                }
            }
        }

        public void Finalizar()
        {
            if (this.estado != EstadoMision.EnEjecucion && this.estado != EstadoMision.Planificada)
            {
                throw new MisionInvalidaException($"No se puede finalizar la misión {codigo} en su estado actual ({estado}).");
            }

            this.estado = EstadoMision.Finalizada;

            // Liberar los recursos utilizados por la misión
            foreach (var recurso in recursos)
            {
                recurso.Liberar();
            }
        }

        public void Cancelar()
        {
            if (this.estado == EstadoMision.Finalizada)
            {
                throw new MisionInvalidaException($"No se puede cancelar una misión que ya ha finalizado.");
            }

            this.estado = EstadoMision.Cancelada;

            // Liberar los recursos
            foreach (var recurso in recursos)
            {
                recurso.Liberar();
            }
        }

        public bool ValidarProtocoloSeguridad()
        {
            // 1. Datos obligatorios completos
            if (string.IsNullOrWhiteSpace(codigo))
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: El código de la misión es obligatorio.");

            if (string.IsNullOrWhiteSpace(nombre))
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: El nombre de la misión es obligatorio.");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: La descripción de la misión es obligatoria.");

            // 2. Fecha final >= fecha inicial
            if (fechaFinEstimada < fechaInicio)
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: La fecha de finalización estimada no puede ser anterior a la fecha de inicio.");

            // 3. Responsable activo
            if (responsable == null)
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: La misión debe tener un responsable asignado.");

            if (!responsable.EstaActivo())
                throw new MisionInvalidaException($"Protocolo de seguridad ORBITA: El responsable {responsable.NombreUsuario} no se encuentra activo.");

            // 4. Contener al menos un recurso
            if (recursos == null || recursos.Count == 0)
                throw new MisionInvalidaException("Protocolo de seguridad ORBITA: La misión debe contener al menos un recurso asignado.");

            // 5. Ningún recurso en mantenimiento
            foreach (var recurso in recursos)
            {
                if (recurso.Estado == EstadoRecurso.Mantenimiento)
                {
                    throw new RecursoNoDisponibleException($"Protocolo de seguridad ORBITA: El recurso {recurso.Codigo} ({recurso.Modelo}) se encuentra en mantenimiento.");
                }
            }

            return true;
        }

        public decimal CalcularCostoEstimado()
        {
            decimal duracionDias = (decimal)Math.Max(1, Math.Ceiling((fechaFinEstimada - fechaInicio).TotalDays));
            return CalcularCostoEstimado(duracionDias);
        }

        public decimal CalcularCostoEstimado(decimal cantidadOperacion)
        {
            decimal costoTotal = 0;
            // Cálculo polimórfico sin if/switch de tipos
            foreach (RecursoExploracion recurso in recursos)
            {
                costoTotal += recurso.CalcularCostoOperacion(cantidadOperacion);
            }
            return costoTotal;
        }

        public override string ToString()
        {
            return $"Misión: {nombre} [{codigo}] | Estado: {estado} | Prioridad: {prioridad} | Responsable: {(responsable != null ? responsable.NombreUsuario : "Sin asignar")} | Recursos: {recursos.Count}";
        }
    }
}
