using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KUKA
{

    class PoseRecognitionSystem
    {


        public float _X = 0;
        public float _Y = 0;
        public float _Z = 0;
        public float _A = 0;
        public float _B = 0;
        public float _C = 0;
        public int _State = 0;
        public int _ObjectType = 0;

       // public float[] CaptureImage_Position = new float[6] { (float)440, (float)10, (float)500, (float)177, (float)-75, (float)3.27 };
        public float[] CaptureImage_Position = new float[6] { (float)440, (float)10, (float)500, (float)100, (float)-75, (float)78.27 };
        //public float[] CaptureImage_Position = new float[6] { (float)440, (float)10, (float)440, (float)100, (float)-75, (float)78.27 };
        public float[] GraspInitial_Position = new float[6] { (float)440, (float)10, (float)300, (float)100, (float)-90, (float)78.27 };


    }
}
