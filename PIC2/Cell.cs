using System;

namespace PIC2
{
    /// <summary>
    /// Класс узла сетки
    /// </summary>
    class Cell
    {
        /// <summary>
        /// Плотность заряда
        /// </summary>
        public double Density;

        /// <summary>
        /// Напряженность
        /// </summary>
        public PIC2.ParticleInCell.Function ElectricField;

        /// <summary>
        /// Напряженность пучка
        /// </summary>
        public double ElectricFieldParticle;

        /// <summary>
        /// Координата узла сетки
        /// </summary>
        public double X;

        /// <summary>
        /// Вывод информации о ячейке
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Узел сетки X={0,7}", X);
        }
    }
}
