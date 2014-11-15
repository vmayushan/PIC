using System;
using System.Diagnostics;

namespace PIC2
{
    public class TriDiagonalMatrixF
    {
        /// <summary>
        /// Диагональ под главной. A[0] не используется.
        /// </summary>
        public double[] A;

        /// <summary>
        /// Главная диагональ
        /// </summary>
        public double[] B;

        /// <summary>
        /// Диагональ над главной. C[C.Length-1] не используется.
        /// </summary>
        public double[] C;

        /// <summary>
        /// Размерность матрицы
        /// </summary>
        public int N
        {
            get { return (A != null ? A.Length : 0); }
        }

        /// <summary>
        /// Индексатор
        /// </summary>
        public double this[int row, int col]
        {
            get
            {
                int di = row - col;

                if (di == 0)
                {
                    return B[row];
                }
                else if (di == -1)
                {
                    Debug.Assert(row < N - 1);
                    return C[row];
                }
                else if (di == 1)
                {
                    Debug.Assert(row > 0);
                    return A[row];
                }
                else return 0;
            }
            set
            {
                int di = row - col;

                if (di == 0)
                {
                    B[row] = value;
                }
                else if (di == -1)
                {
                    Debug.Assert(row < N - 1);
                    C[row] = value;
                }
                else if (di == 1)
                {
                    Debug.Assert(row > 0);
                    A[row] = value;
                }
                else
                {
                    throw new ArgumentException("Могут быть изменены только 3 диагонали.");
                }
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TriDiagonalMatrixF(int n)
        {
            this.A = new double[n];
            this.B = new double[n];
            this.C = new double[n];
        }

        /// <summary>
        /// Решает СЛАУ
        /// </summary>
        /// <remarks>
        /// Метод прогонки
        /// </remarks>
        /// <param name="d">Вектор-столбец значений.</param>
        public double[] Solve(double[] d)
        {

            int n = this.N;

            if (d.Length != n)
            {
                throw new ArgumentException("Размерности столбца и матрицы должны совпадать.");
            }

            // cPrime
            double[] cPrime = new double[n];
            cPrime[0] = C[0] / B[0];

            for (int i = 1; i < n; i++)
            {
                cPrime[i] = C[i] / (B[i] - cPrime[i - 1] * A[i]);
            }

            // dPrime
            double[] dPrime = new double[n];
            dPrime[0] = d[0] / B[0];

            for (int i = 1; i < n; i++)
            {
                dPrime[i] = (d[i] - dPrime[i - 1] * A[i]) / (B[i] - cPrime[i - 1] * A[i]);
            }

            // Back substitution
            double[] x = new double[n];
            x[n - 1] = dPrime[n - 1];

            for (int i = n - 2; i >= 0; i--)
            {
                x[i] = dPrime[i] - cPrime[i] * x[i + 1];
            }
            return x;
        }
    }
}
