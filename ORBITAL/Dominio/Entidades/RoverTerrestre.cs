using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class RoverTerrestre : RecursoExploracion
    {
        // Atributos
        private decimal autonomia;
        private decimal capacidadCarga;
        private decimal costoPorKilometro;

        // Getters y Setters
        public decimal Autonomia { get => autonomia; set => autonomia = value; }
        public decimal CapacidadCarga { get => capacidadCarga; set => capacidadCarga = value; }
        public decimal CostoPorKilometro { get => costoPorKilometro; set => costoPorKilometro = value; }

        // Constructores
        public RoverTerrestre(string codigo, string modelo) : base(codigo, modelo) { }

        public RoverTerrestre(string codigo, string modelo, decimal autonomia, decimal capacidadCarga, decimal costoKm) : base(codigo, modelo)
        {
            this.autonomia = autonomia;
            this.capacidadCarga = capacidadCarga;
            this.costoPorKilometro = costoKm;
        }

        // Métodos públicos
        public override decimal CalcularCostoOperacion(decimal kilometros)
        {
            return kilometros * this.costoPorKilometro;
        }
    }
}
