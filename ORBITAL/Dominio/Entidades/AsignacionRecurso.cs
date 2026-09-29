using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class AsignacionRecurso
    {
        // Atributos
        private int id;
        private Mision mision;
        private RecursoExploracion recurso;
        private DateTime fechaAsignacion;
        private DateTime? fechaLiberacion;
        private bool activa;
        private decimal cantidadOperacion;

        // Getters y Setters
        public int Id { get => id; set => id = value; }
        public Mision Mision { get => mision; set => mision = value; }
        public RecursoExploracion Recurso { get => recurso; set => recurso = value; }
        public DateTime FechaAsignacion { get => fechaAsignacion; set => fechaAsignacion = value; }
        public DateTime? FechaLiberacion { get => fechaLiberacion; set => fechaLiberacion = value; }
        public bool Activa { get => activa; set => activa = value; }

        public decimal CantidadOperacion
        {
            get => cantidadOperacion;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(
                        nameof(CantidadOperacion),
                        "La cantidad de operación debe ser mayor que cero."
                    );

                cantidadOperacion = value;
            }
        }

        // Constructores
        public AsignacionRecurso(Mision mision, RecursoExploracion recurso)
            : this(mision, recurso, 1)
        {
        }

        public AsignacionRecurso(
            Mision mision,
            RecursoExploracion recurso,
            decimal cantidadOperacion)
        {
            if (mision == null)
                throw new ArgumentNullException(nameof(mision));

            if (recurso == null)
                throw new ArgumentNullException(nameof(recurso));

            if (cantidadOperacion <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadOperacion),
                    "La cantidad de operación debe ser mayor que cero."
                );

            this.mision = mision;
            this.recurso = recurso;
            this.cantidadOperacion = cantidadOperacion;
            this.fechaAsignacion = DateTime.Now;
            this.fechaLiberacion = null;
            this.activa = true;

            // Asegurar que el recurso pase a asignado
            recurso.Asignar(mision);
        }

        public AsignacionRecurso(
            int id,
            Mision mision,
            RecursoExploracion recurso,
            DateTime fechaAsignacion,
            DateTime? fechaLiberacion,
            bool activa,
            decimal cantidadOperacion = 1)
        {
            if (cantidadOperacion <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(cantidadOperacion),
                    "La cantidad de operación debe ser mayor que cero."
                );

            this.id = id;
            this.mision = mision;
            this.recurso = recurso;
            this.fechaAsignacion = fechaAsignacion;
            this.fechaLiberacion = fechaLiberacion;
            this.activa = activa;
            this.cantidadOperacion = cantidadOperacion;
        }

        // Métodos
        public decimal CalcularCosto()
        {
            if (this.recurso == null)
                return 0;

            return this.recurso.CalcularCostoOperacion(
                this.cantidadOperacion
            );
        }

        public void Liberar()
        {
            if (!this.activa)
                return;

            this.activa = false;
            this.fechaLiberacion = DateTime.Now;

            if (this.recurso != null)
            {
                this.recurso.Liberar();
            }
        }

        public bool EstaActiva()
        {
            return this.activa;
        }

        public override string ToString()
        {
            return $"{Mision.Codigo} - {Recurso.Codigo} " +
                   $"(Uso: {cantidadOperacion}) - Activa: {activa}";
        }
    }
}
