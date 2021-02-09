using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GCNWii
{
    public class Ank1Header
    {
        public char[] tag; //'ANK1'
        public UInt32 sizeOfSection;

        /*
          0 - play once
          2 - loop
        */
        public byte loopFlags;

        public byte angleMultiplier; //all angles have to multiplied by pow(2, angleMultiplyer)

        public UInt16 animationLength; //in time units

        public UInt16 numJoints; //that many animated joints at offsetToJoints
        public UInt16 scaleCount; //that many floats at offsetToScales
        public UInt16 rotCount;   //that many s16s at offsetToRots
        public UInt16 transCount; //that many floats at offsetToTrans

        public UInt32 offsetToJoints;
        public UInt32 offsetToScales;
        public UInt32 offsetToRots;
        public UInt32 offsetToTrans;
    }


    //TODO: the following two structs have really silly names, rename them
    public class AnimIndex
    {
        public UInt16 count;
        public UInt16 index;
        public UInt16 zero; //always 0?? -> no (biawatermill01.bck) TODO: find out what it means
    }

    public class AnimComponent
    {
        public AnimIndex s = new AnimIndex(); //scale
        public AnimIndex r = new AnimIndex(); //rotation
        public AnimIndex t = new AnimIndex(); //translation
    }

    public class AnimatedJoint
    {
        /*
        if count > 1, count*3 floats/shorts stored at index
        (time, value, unk [interpolation info, e.g. tangent??])?

        for shorts, time is a "real" short, no fixedpoint
        */
        public AnimComponent x = new AnimComponent();
        public AnimComponent y = new AnimComponent();
        public AnimComponent z = new AnimComponent();
    }
}