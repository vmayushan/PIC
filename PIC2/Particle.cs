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
        public double W
        {
            get
            {
                double gamma = 1 / Math.Sqrt(1 - Beta * Beta);
                return (-1 / Constant.Alfa) * (gamma - 1);
            }
        }

        /// <summary>
        /// Приведенная скорость
        /// </summary>
        public double Beta
        {
            get
            {
                return P / Math.Sqrt(1 + P * P);
            }
        }

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
        /// Заряд частицы
        /// </summary>
        public double Q;

        public bool First = true;

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
