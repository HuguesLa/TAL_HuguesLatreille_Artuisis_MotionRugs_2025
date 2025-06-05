[System.Serializable]
public class SerializableRandom
{
    //Name of the algorithm used in this file MRG32k3a
    protected double x_1;
    protected double x_2;
    protected double x_3;

    protected double y_1;
    protected double y_2;
    protected double y_3;

    protected double mx = 4294967087.0;
    protected double ax = 1403580.0;
    protected double bx = 810728.0;

    protected double my = 4294944443.0;
    protected double ay = 527612.0;
    protected double by = 1370589.0;

    protected double x;
    protected double y;
    protected double u;

    private double seed;


    public SerializableRandom()
    {
        this.seed = double.Parse(System.DateTime.Now.ToString("yyyyMMddHHmmss"));

        this.x_1 = this.seed;
        this.x_2 = this.seed;
        this.x_3 = this.seed;
        this.y_1 = this.seed;
        this.y_2 = this.seed;
        this.y_3 = this.seed;
    }

    public SerializableRandom(double seed)
    {
        this.x_1 = seed;
        this.x_2 = seed;
        this.x_3 = seed;
        this.y_1 = seed;
        this.y_2 = seed;
        this.y_3 = seed;

        this.seed = seed;
    }

    public double Rand(double min, double max)
    {
        return (U() * (max - min)) + min;
    }

    protected double X()
    {
        // Compute Xn
        x = (ax * x_2 - bx * x_3) % mx;

        // Update Xn-1, Xn-2 and Xn-3
        x_3 = x_2; x_2 = x_1; x_1 = x;

        return x;
    }

    protected double Y()
    {
        // Compute Yn
        y = (ay * y_1 - by * y_3) % my;

        // Update Yn-1, Yn-2 and Yn-3
        y_3 = y_2; y_2 = y_1; y_1 = y;

        return y;
    }

    protected double U()
    {
        // Compute Un
        u = ((X() - Y()) % mx) / mx;
        if (u < 0) u += 1;

        return u;
    }

    public SerializableRandom Clone()
    {
        return (SerializableRandom) this.MemberwiseClone();
    }
}
