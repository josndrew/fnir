
namespace GUI
{
    class processedData
    {
        private double xCord;
        private double yCord1;
        private double yCord2;
        private double yCord1_P;
        private double yCord2_P;

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

        public double getYCord1_P()
        {
            return yCord1_P;
        }

        public double getYCord2_P()
        {
            return yCord2_P;
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
            calcPerData();
        }

        private void calcPerData()
        {
            yCord1_P = (yCord1 + yCord2)/2 * .4;
            yCord2_P = (yCord1 + yCord2)/3 * 1.6;
        }

    }
}
