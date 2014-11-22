using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        private double dt;

        /// <summary>
        /// Время "выпуска" новых частиц
        /// </summary>
        private double tImp;

        /// <summary>
        /// Напряжение анода
        /// </summary>
        private double uAnode;

        /// <summary>
        /// Стартовая энергия частицы
        /// </summary>
        private double e0;

        /// <summary>
        /// Стартовый импульс
        /// </summary>
        private double p0;

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

        double W = new double();

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

        private ParticleMethod particleMethod;

        private Stopwatch densitiesTime = new Stopwatch();
        private Stopwatch electricFieldTime = new Stopwatch();
        private Stopwatch forcesTime = new Stopwatch();
        private Stopwatch integrateTime = new Stopwatch();
        private object lockCells = new object();

        private int iterations;

        public double debugE = 0;
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
        /// <param name="e0">Стартовая энергия частицы</param>
        public ParticleInCell(double length, int nCell, double stepTime, double tImp, double uAnode, double e0, ParticleMethod particleMethod)
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
            this.particleMethod = particleMethod;
            this.dt = Constant.Velocity * stepTime;

            double gamma = 1 - Constant.Alfa * e0;
            double beta = Math.Sqrt(gamma * gamma - 1) / gamma;
            this.p0 = beta / Math.Sqrt(1 - beta * beta);
        }

        /// <summary>
        /// Вызывается после интегирования
        /// </summary>
        public void Report()
        {
            Console.WriteLine("Завершено за {0} итераций", iterations);
            Console.WriteLine("Время полета частиц: {0}", t);
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
            var particle = new Particle();
            particle.P = p0;
            particles.Add(particle);
            particleCount++;
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
        }

        #endregion

        #region Интегрирование
        /// <summary>
        /// Само интегрирование
        /// </summary>
        public void Run()
        {
            iterations = 0;
            MatrixPoisson();
            //время
            //переменная для выхода из цикла
            bool work = true;
            while (work)
            {
                if (impulse)
                {
                    AddParticle();
                }
                Forces();

                if (impulse)
                {
                    foreach (var particle in particles.Where(p => p.First))
                    {
                        particle.P = particle.P - 0.5 * dt * Constant.Alfa * particle.E;
                        particle.First = false;
                    }
                }

                //проверка условия выпуска новых частиц
                ImpulseCondition();

                sw.WriteLine(string.Format("{0:F15};{1:F15}", eСathode.ToString(), cells[0].Density));
                sw.Flush();

                integrateTime.Start();
                foreach (var particle in particles)
                {
                    particle.P = particle.P + dt * Constant.Alfa * particle.E;
                    particle.X = particle.X + dt * particle.Beta;
                    if(particle.X >= length)
                    {
                        Console.WriteLine(particle.W);
                    }
                }

                //удаление долетевших частиц
                particles.RemoveAll(x => x.X > length);
                if (particles.Count() == 0)
                {
                    work = false;
                }
                //добавляем к времени шаг интегрирования
                t += stepTime;
                iterations++;
                integrateTime.Stop();
            }
        }

        private void Forces()
        {
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

                if (particleMethod == ParticleMethod.CIC)
                {
                    //CIC
                    //particle.near = cells.Where(p => Math.Abs(particle.X - p.X) <= gridStep).ToArray();
                    int leftCell = (int)(particle.X / gridStep);
                    particle.near = new Cell[] { cells[leftCell], cells[leftCell + 1] };
                    if (leftCell >= 0 && leftCell < nCell - 1)
                    {
                        for (int i = 0; i < particle.near.Length; i++)
                        {
                            W = 1 - Math.Abs(particle.X - particle.near[i].X) / gridStep;
                            particle.near[i].Density += I * stepTime * W / gridStep; //q*Wi/h CIC
                        }
                    }
                    else
                    {
                        throw new ApplicationException("CIC error");
                    }
                }
                else if (particleMethod == ParticleMethod.NGP)
                {
                    particle.NGP = cells.Where(p => Math.Abs(particle.X - p.X) <= gridStep).OrderBy(p => Math.Abs(particle.X - p.X)).First(); //delete LINQ
                    particle.NGP.Density += I * stepTime / gridStep; //q/h NGP
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
                if (i == 0)
                {
                    cells[i].ElectricFieldParticle = Derivative.DerivativeForward(potential, i, gridStep);
                }
                else if (i == nCell - 1)
                {
                    cells[i].ElectricFieldParticle = Derivative.DerivativeBackward1(potential, i, gridStep);
                }
                else
                {
                    cells[i].ElectricFieldParticle = Derivative.DerivativeCentral1(potential, i, gridStep);
                }
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

                if (particleMethod == ParticleMethod.CIC)
                {
                    for (int i = 0; i < particle.near.Length; i++)
                    {
                        //потенциал поля + потенциал поля пучка
                        W = 1 - Math.Abs(particle.X - particle.near[i].X) / gridStep;
                        particle.E += (particle.near[i].ElectricField(particle.X) + particle.near[i].ElectricFieldParticle) * W;

                    }
                }
                else if (particleMethod == ParticleMethod.NGP)
                {
                    particle.E += (particle.NGP.ElectricField(particle.X) + particle.NGP.ElectricFieldParticle);
                }
                if (warn && particle.E > 0)
                {

                    Console.WriteLine("Начались отрицательные");
                    warn = false;

                }
            }

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
                    //foreach(var c in cells)
                    //{
                    //    sw.WriteLine(string.Format("{0:F15};{1:F15}", c.Density, c.ElectricFieldParticle));
                    //    sw.Flush();
                    //}

                    debugE = newEСathode / cells[0].ElectricField(0);
                    impulse = false;
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
