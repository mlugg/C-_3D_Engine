using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine
{
    class Mesh
    {
        public Mesh()
        {
            faces = new Face[0];
            position = new Vector3(0, 0, 0);
            rotation = new Vector3(0, 0, 0);
        }

        public Face[] faces;

        public Vector3 position;
        public Vector3 rotation;
    }

    class Face
    {
        public Face() { }

        public Face(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
    }
}
