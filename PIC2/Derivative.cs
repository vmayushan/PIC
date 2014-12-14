
namespace PIC2
{
    /// <summary>
    /// Численное дифференциирование
    /// </summary>
    static class Derivative
    {

        public static double DerivativeForward1(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 0 || i > y.Length - 3 || h == 0)
                return double.NaN;
            return (-3 * y[i] + 4 * y[i + 1] - y[i + 2]) / 2 / h;
        }

        public static double DerivativeCentral1(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 1 || i > y.Length - 2 || h == 0)
                return double.NaN;
            return (y[i + 1] - y[i - 1]) / 2 / h;
        }
        public static double DerivativeBackward1(double[] y, int i, double h)
        {
            if (y == null || y.Length < 3 || i < 0 || i < 3 || h == 0)
                return double.NaN;
            return (3 * y[i] - 4 * y[i - 1] + y[i - 2]) / 2 / h;
        }
        public static double DerivativeRichardson1(double[] y, int i, double h, string flag)
        {
            double result = double.NaN;
            switch (flag)
            {
                case "Backward":
                    result = (4 * DerivativeBackward1(y, i, h / 2) - DerivativeBackward1(y, i, h)) / 3;
                    break;
                case "Forward":
                    result = (4 * DerivativeForward1(y, i, h / 2) - DerivativeForward1(y, i, h)) / 3;
                    break;
                case "Central":
                    result = (4 * DerivativeCentral1(y, i, h / 2) - DerivativeCentral1(y, i, h)) / 3;
                    break;
                default:
                    result = double.NaN;
                    break;
            }
            return result;
        }
        public static double Central(double[] y, int i, double h)
        {
            if (y == null || y.Length < 2 || i < 0 || i > y.Length - 1 || h == 0)
                return double.NaN;
            return (y[i + 1] - y[i]) / h;
        }
    }
}
