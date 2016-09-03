
namespace GUI
{
    class processedData
    {
        private double xCord;
        private double yCord1;
        private double yCord2;

        public processedData() { }

        public double getXCord()
        {
            return xCord;
        }

        public double getYCord1()
        {
            return yCord1;
        }

        public double getYCord2()
        {
            return yCord2;
        }

        public void setXCord(double alpha)
        {
            xCord = alpha;
        }

        public void setYCord1(double alpha)
        {
            yCord1 = alpha;
        }
        
        public void setYCord2(double alpha)
        {
            yCord2 = alpha;
        }
    }
}
