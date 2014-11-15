
namespace PIC2
{
    /// <summary>
    /// Численное дифференциирование
    /// </summary>
    static class Derivative
    {
        public static double DerivativeOh4(double[] y, int i, double h)
        {
            if (y == null || y.Length < 5 || i < 2 || i > y.Length - 3 || h == 0)
                return 0;
            return (y[i - 2] - 8 * y[i - 1] + 8 * y[i + 1] - y[i + 2]) / 12 / h;
        }

        public static double DerivativeForward(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 0 || i > y.Length - 3 || h == 0)
                return 0;
            return (-3 * y[i] + 4 * y[i + 1] - y[i + 2]) / 2 / h;
        }
        public static double DerivativeOh1(double[] y, int i, double h)
        {
            if (y == null || y.Length < 2 || i < 0 || i > y.Length - 1 || h == 0)
                return 0;
            return ( y[i + 1]-y[i])  / h;
        }
        public static double DerivativeCentral1(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 1 || i > y.Length - 2 || h == 0)
                return 0;
            return (y[i + 1] - y[i - 1]) / 2 / h;
        }
    }
}
