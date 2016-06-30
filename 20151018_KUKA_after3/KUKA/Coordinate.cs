using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KUKA
{
    class Coordinate_XYZABC
    {
        float _X, _Y, _Z, _A, _B, _C;

        public float X
        {
            get { return _X; }
            set { _X = value; }
        }

        public float Y
        {
            get { return _Y; }
            set { _Y = value; }
        }

        public float Z
        {
            get { return _Z; }
            set { _Z = value; }
        }

        public float A
        {
            get { return _A; }
            set { _A = value; }
        }

        public float B
        {
            get { return _B; }
            set { _B = value; }
        }

        public float C
        {
            get { return _C; }
            set { _C = value; }
        }

        public Coordinate_XYZABC()
        {
            _X = 0;
            _Y = 0;
            _Z = 0;
            _A = 0;
            _B = 0;
            _C = 0;
        }
    }

    class Coordinate_Axis
    {

        float _A1, _A2, _A3, _A4, _A5, _A6;

        public float A1
        {
            get { return _A1; }
            set { _A1 = value; }
        }

        public float A2
        {
            get { return _A2; }
            set { _A2 = value; }
        }

        public float A3
        {
            get { return _A3; }
            set { _A3 = value; }
        }

        public float A4
        {
            get { return _A4; }
            set { _A4 = value; }
        }

        public float A5
        {
            get { return _A5; }
            set { _A5 = value; }
        }

        public float A6
        {
            get { return _A6; }
            set { _A6 = value; }
        }

        public Coordinate_Axis()
        {
            _A1 = 0;
            _A2 = 0;
            _A3 = 0;
            _A4 = 0;
            _A5 = 0;
            _A6 = 0;
        }
    }
}
