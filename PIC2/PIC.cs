using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PIC2
{
    /// <summary>
    /// Метод крупных частиц в ячейках
    /// </summary>
    public class ParticleInCell
    {
        #region Поля и свойства
        /// <summary>
        /// Длина промежутка
        /// </summary>
        private double length;

        /// <summary>
        /// Количество ячеек
        /// </summary>
        private int nCell;

        /// <summary>
        /// Шаг интегрирования
        /// </summary>
        private double stepTime;

        /// <summary>
        /// Время "выпуска" новых частиц
        /// </summary>
        private double tImp;

        /// <summary>
        /// Напряжение анода
        /// </summary>
        private double uAnode;

        /// <summary>
        /// Стартовая энергия частицы (еще не использовал)
        /// </summary>
        private double e0;

        /// <summary>
        /// Шаг сетки
        /// </summary>
        private double gridStep;
        
        /// <summary>
        /// "Тип" для функции напряженности в ячейке
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public delegate double Function(double x);

        /// <summary>
        /// Список всех частиц
        /// </summary>
        private List<Particle> particles;

        /// <summary>
        /// Список всех ячеек
        /// </summary>
        private List<Cell> cells;

        /// <summary>
        /// Матрица для вычисленния потенциалов
        /// </summary>
        private TriDiagonalMatrixF poissonMatrix;

        /// <summary>
        /// Время
        /// </summary>
        private double t = 0;

        /// <summary>
        /// Запись в файл, для построения графиков
        /// </summary>
        private StreamWriter sw = new StreamWriter("debug.csv");



        /// <summary>
        /// Сила тока, формула Чайлда-Ленгмюра (закон трех вторых)
        /// </summary>
        private double I
        {
            get
            {
                return 2.33E-6 * (1 / (length * length)) * Math.Pow(uAnode, 1.5);
            }
        }

        /// <summary>
        /// Напряженность на аноде
        /// </summary>
        private double eСathode;

        /// <summary>
        /// Выпускать ли следующую частицу
        /// </summary>
        private bool impulse = true;
        private double impulsetEps = 0.00001;
        private int particleCount = 0;
        private bool warn = true;

        private Stopwatch densitiesTime = new Stopwatch();
        private Stopwatch electricFieldTime = new Stopwatch();
        private Stopwatch forcesTime = new Stopwatch();
        private Stopwatch integrateTime = new Stopwatch();

        #endregion

        #region Конструктор
        /// <summary>
        /// Конструктор, вычисляет шаг сетки
        /// </summary>
        /// <param name="length">Длина промежутка</param>
        /// <param name="nCell">Количество ячеек</param>
        /// <param name="stepTime">Шаг интегрирования</param>
        /// <param name="tImp">Время выпуска новых частиц</param>
        /// <param name="uAnode">Напряжение на аноде</param>
        /// <param name="e0">Стартовая энергия частицы (еще не использовал)</param>
        public ParticleInCell(double length, int nCell, double stepTime, double tImp, double uAnode, double e0)
        {
            this.length = length;
            this.nCell = nCell;
            this.stepTime = stepTime;
            this.tImp = tImp;
            this.uAnode = uAnode;
            this.e0 = e0;
            this.gridStep = length / (nCell - 1);
            particles = new List<Particle>((int)(tImp / stepTime) + 1);
            cells = new List<Cell>(nCell);
            AddCells(x => -uAnode / length);
        }

        /// <summary>
        /// Вызывается после интегирования
        /// </summary>
        public void Report()
        {
            Console.WriteLine("Работа завершена");
            Console.WriteLine("Время {0}", t);
            Console.WriteLine("Всего частиц выпущено: {0}", particleCount);
            Console.WriteLine("Время затраченное на интегрирование: {0} мс\n на расчет плотностей зарядов: {1} мс\n на расчет сил: {2} мс\n на расчет собственных напряженностей пучка: {3} мс\n",
                integrateTime.ElapsedMilliseconds, densitiesTime.ElapsedMilliseconds, forcesTime.ElapsedMilliseconds, electricFieldTime.ElapsedMilliseconds);
        }
        #endregion

        #region Добавление частицы и узлов сетки
        /// <summary>
        /// Добавляет частицу в точке 0, с зарядом и массой электрона
        /// </summary>
        private void AddParticle()
        {

            //создаем новую частицу
            var particle = new Particle() { X = 0, P = 0 };
            //добавляем в список
            particles.Add(particle);
            particleCount++;
            //Console.WriteLine("Добавлена {0}", particle);
        }

        /// <summary>
        /// Добавляет ячейки с заданной напряженность
        /// </summary>
        /// <param name="function">Функция напряженности ячейки</param>
        private void AddCells(Function function)
        {
            for (int i = 0; i < nCell; i++)
            {
                //создаем новую ячейку
                var cell = new Cell() { ElectricField = function, Density = 0, X = gridStep * i };
                //добавляем в список
                cells.Add(cell);
            }
            Console.WriteLine("Добавлена сетка из {0} узлов", nCell);
        }

        #endregion

        #region Интегрирование
        /// <summary>
        /// Само интегрирование
        /// </summary>
        public void Run()
        {
            MatrixPoisson();
            //время
            //переменная для выхода из цикла
            bool work = true;
            //tau = c*t
            double step = Constant.Velocity * stepTime;
            while (work)
            {
                if (impulse) AddParticle();

                //пересчет зарядов в ячейках и напряженностей на частицах и добавление частиц
                densitiesTime.Start();
                CalculateDensities();
                densitiesTime.Stop();

                //собственное поле пучка
                electricFieldTime.Start();
                CalculateElectricField();
                electricFieldTime.Stop();

                // напряженности на частицу
                forcesTime.Start();
                CalculateForces();
                forcesTime.Stop();
                //проверка условия выпуска новых частиц
                ImpulseCondition();

                integrateTime.Start();
                //интегрирование каждой частицы, которая не долетела
                foreach (var particle in particles)
                {
                    //новая координата X
                    //dx/dtau = p/sqrt(1+p^2)
                    particle.X = particle.X + step * particle.P / Math.Sqrt(1 + particle.P * particle.P);
                    particle.Beta = particle.P / Math.Sqrt(1 + particle.P * particle.P);
                    double gamma = 1 / Math.Sqrt(1 - particle.Beta * particle.Beta);
                    particle.W = (-1 / Constant.Alfa) * (gamma - 1);
                    //dp/dtau = alfa*E(x) новый импульс частицы
                    particle.P = particle.P + step * Constant.Alfa * particle.E;
                    //if (particle.X > length)
                    //{
                    //    sw.WriteLine(string.Format("{0:F15};{1:F15}", particle.W, particle.Beta));
                    //    sw.Flush();
                    //}

                }
                //удаление долетевших частиц
                particles.RemoveAll(x => x.X > length);
                if (particles.Count() == 0)
                {
                    work = false;
                }
                //добавляем к времени шаг интегрирования
                t += stepTime;
                integrateTime.Stop();
            }
        }

        #endregion

        #region Плотности заряда, CIC
        /// <summary>
        /// Расчет плотностей зарядов
        /// </summary>
        private void CalculateDensities()
        {
            //пересчитываем плотности
            //обнуляем плотность по всех ячейках
            cells.ForEach(x => { x.Density = 0; });
            foreach (var particle in particles)
            {
                particle.E = 0;
                //ищем две ближайшие ячейки как |x-Xp| < Шага сетки изменяем
                particle.near = cells.Where(p => Math.Abs(particle.X - p.X) <= gridStep).ToArray();
                // CIC
                double[] W = particle.near.Select(p => (1 - Math.Abs(particle.X - p.X) / gridStep)).ToArray();

                if (particle.near[0].X == 0.0)
                {
                    cells[1].Density += I * stepTime / gridStep; //q/h NGP
                }
                else if (particle.near.Length == 2 && particle.near[1].X == length)
                {
                    cells[nCell - 2].Density += I * stepTime / gridStep;
                }
                else
                {
                    for (int i = 0; i < particle.near.Length; i++)
                    {
                        particle.near[i].Density += I * stepTime * W[i] / gridStep; //q*Wi/h CIC
                    }
                }
            }
        }

        /// <summary>
        /// Расчет напряженности собственного поля пучка
        /// </summary>
        private void CalculateElectricField()
        {
            // находим потенциалы
            double[] potential = Poisson();

            //считаем разностные производные
            for (int i = 0; i < nCell - 1; i++)
            {
                cells[i].ElectricFieldParticle = Derivative.DerivativeForward(potential, i, gridStep);
            }
        }

        /// <summary>
        /// пересчет напряженностей на частицах
        /// </summary>
        private void CalculateForces()
        {
            // для всех частиц считаем напряженность
            foreach (var particle in particles)
            {
                double[] W = particle.near.Select(p => (1 - Math.Abs(particle.X - p.X) / gridStep)).ToArray();
                if (particle.near[0].X == 0.0)
                {
                    particle.E += (particle.near[1].ElectricField(particle.X) + particle.near[1].ElectricFieldParticle);
                }
                else
                {
                    for (int i = 0; i < particle.near.Length; i++)
                    {
                        //потенциал поля + потенциал поля пучка
                        particle.E += (particle.near[i].ElectricField(particle.X) + particle.near[i].ElectricFieldParticle) * W[i];
                    }
                }

                if (warn && particle.E > 0)
                {

                    Console.WriteLine("Начались отрицательные");
                    warn = false;

                }
            }
            //debug.Add(string.Format("{0:F15}", eСathode.ToString()));
            //sw.WriteLine(string.Format("{0:F15};{1:F15}", eСathode.ToString(), cells[1].Density));
            //sw.Flush();
        }

        /// <summary>
        /// Проверка, нужно ли выпускать новые частицы
        /// </summary>
        private void ImpulseCondition()
        {
            double newEСathode = cells[0].ElectricFieldParticle + cells[0].ElectricField(0);
            double deltaE = Math.Abs((newEСathode - eСathode) / newEСathode);
            if (t > tImp)
            {
                if (impulse && deltaE < impulsetEps)
                {
                    Console.WriteLine("Установилась напряженность {0}", newEСathode);
                    impulse = false;
                    Console.WriteLine(particles.Count);
                }
            }
            this.eСathode = newEСathode;
        }

        #endregion

        #region Пуассон

        /// <summary>
        /// Составление трехдиагональной матрицы для нахождения потенциалов, выполняется один раз в начале
        /// </summary>
        private void MatrixPoisson()
        {
            poissonMatrix = new TriDiagonalMatrixF(nCell);
            poissonMatrix[0, 0] = 1; //граничное условие
            poissonMatrix[nCell - 1, nCell - 1] = 1; //граничное условие
            for (int i = 1; i < nCell - 1; i++)
            {
                //главная диагональ
                poissonMatrix.B[i] = -2 / (gridStep * gridStep);
                //диагональ ниже главной
                poissonMatrix.A[i] = 1 / (gridStep * gridStep);
                //диагональ выше главной
                poissonMatrix.C[i] = 1 / (gridStep * gridStep);
            }
        }

        /// <summary>
        /// Составление столбца для нахождения потенциалов, выполняется каждую итерацию
        /// </summary>
        /// <returns></returns>
        private double[] Poisson()
        {
            double[] B = new double[nCell];
            for (int i = 1; i < nCell - 2; i++)
            {
                // -p/epsilon0
                B[i] = -cells[i].Density / Constant.Epsilon;
            }
            return poissonMatrix.Solve(B);
        }

        #endregion
    }
}
