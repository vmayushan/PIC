using System;

namespace PIC2
{
    /// <summary>
    /// Узел сетки
    /// </summary>
    class Cell
    {
        public Cell(PIC2.ParticleInCell.Function electricField)
        {
            this.electricField = electricField;
        }
        /// <summary>
        /// Плотность заряда
        /// </summary>
        public double Density;

        /// <summary>
        /// Напряженность
        /// </summary>
        private PIC2.ParticleInCell.Function electricField;

        /// <summary>
        /// Напряженность пучка
        /// </summary>
        private double electricFieldParticle;

        /// <summary>
        /// Координата узла сетки
        /// </summary>
        public double X;

        /// <summary>
        /// Вывод информации об узле
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Узел сетки X={0,7}", X);
        }

        /// <summary>
        /// Суммая напряженность
        /// </summary>
        /// <param name="x">x</param>
        /// <returns></returns>
        public double GetE(int x = 0)
        {
            return electricField(x) + electricFieldParticle;
        }
        public void SetElectricFieldParticle(double electricField)
        {
            electricFieldParticle = electricField;
        }
    }
}
