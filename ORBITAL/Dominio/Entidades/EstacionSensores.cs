using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORBITAL.Dominio.Entidades
{
    public class EstacionSensores : RecursoExploracion
    {
        // Atributos
        private int cantidadSensores;
        private decimal consumoEnergetico;
        private decimal costoDiario;

        // Getters y Setters
        public int CantidadSensores { get => cantidadSensores; set => cantidadSensores = value; }
        public decimal ConsumoEnergetico { get => consumoEnergetico; set => consumoEnergetico = value; }
        public decimal CostoDiario { get => costoDiario; set => costoDiario = value; }

        // Constructores
        public EstacionSensores(string codigo, string modelo) : base(codigo, modelo) { }

        public EstacionSensores(string codigo, string modelo, int cantidadSensores, decimal consumo, decimal costoDiario) : base(codigo, modelo)
        {
            this.cantidadSensores = cantidadSensores;
            this.consumoEnergetico = consumo;
            this.costoDiario = costoDiario;
        }

        // Métodos públicos
        public override decimal CalcularCostoOperacion(decimal dias)
        {
            return dias * this.costoDiario;
        }
    }
}
