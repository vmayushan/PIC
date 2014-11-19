
namespace PIC2
{
    /// <summary>
    /// физические константы
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Скорость света
        /// </summary>
        public const double Velocity = 299792458.0;

        /// <summary>
        /// Заряд электрона
        /// </summary>
        public const double ECharge = -1.602176565E-19;

        /// <summary>
        /// Масса электрона
        /// </summary>
        public const double EMass = 9.10938291E-31;

        /// <summary>
        /// Электрическая постоянная
        /// </summary>
        public const double Epsilon = 8.85418782E-12;

        /// <summary>
        /// q/(m*c^2)
        /// </summary>
        public const double Alfa = -1.9569512693314196E-6;
    }
    public enum ParticleMethod
    {
        NGP,
        CIC
    }
}
