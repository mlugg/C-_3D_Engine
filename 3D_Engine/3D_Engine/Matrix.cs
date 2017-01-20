using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine
{
    class Matrix
    {
        public Matrix()
        {
            matrix = new float[4][];
            matrix[0] = matrix[1] = matrix[2] = matrix[3] = new float[4];

            input = new float[4];
        }
        public float[][] matrix;
        public float[] input;

        public float[] output { get {
                float[] x = new float[4];
                for(int i = 0; i < 4; i++)
                {
                    float a = matrix[i][0] * input[0];
                    float b = matrix[i][1] * input[1];
                    float c = matrix[i][2] * input[2];
                    float d = matrix[i][3] * input[3];
                    x[i] = a + b + c + d;
                }
                return x; } }

    }
}
