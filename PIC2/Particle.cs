using System;

namespace PIC2
{
    /// <summary>
    /// Класс частица
    /// </summary>
    class Particle
    {
        /// <summary>
        /// Координата
        /// </summary>
        public double X;

        /// <summary>
        /// Энергия в электрон-вольтах
        /// </summary>
        public double W;

        /// <summary>
        /// Приведенная скорость
        /// </summary>
        public double Beta;

        /// <summary>
        /// Напряженность, действующая на частицу
        /// </summary>
        public double E;

        /// <summary>
        /// Приведенный импульс, dx/dtau = p/sqrt(1+p^2)
        /// </summary>
        public double P;

        /// <summary>
        /// Два ближайших узла сетки
        /// </summary>
        public Cell[] near;

        public Cell NGP;

        /// <summary>
        /// Вывод информации о частице
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("частица: х={0}, p={1:F7}, beta={2:F7}, W={3:F7}", X, P, Beta, W);
        }
    }
}
