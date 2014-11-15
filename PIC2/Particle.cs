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
        /// Конечная энергия
        /// </summary>
        public double W;

        /// <summary>
        /// Конечная приведенная скорость
        /// </summary>
        public double Beta;

        /// <summary>
        /// E(x), dp/dtau = alfa*E(x)
        /// </summary>
        public double E;

        /// <summary>
        /// "Приведенный" импульс частицы,  dx/dtau = p/sqrt(1+p^2)
        /// </summary>
        public double P;

        /// <summary>
        /// Долетела ли частица
        /// </summary>
       // public bool Finished;

        /// <summary>
        /// Два ближайших узла сетки
        /// </summary>
        public Cell[] near;


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
