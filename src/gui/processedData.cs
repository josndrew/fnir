
namespace Fall_Detector
{
    class processedData
    {
        private double xCord;
        private double avg;

        public processedData() { }

        public double getXCord()
        {
            return xCord;
        }

        public double getAvg()
        {
            return avg;
        }

        public void setXCord(double alpha)
        {
            xCord = alpha;
        }

        public void setAvg(double alpha)
        {
            avg = alpha;
        }
    }
}
